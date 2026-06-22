<h1 align="center">
  <br>
  CifraShop
  <br>
</h1>

<h4 align="center">Запускай и управляй — интернет-магазин на .NET 10</h4>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 10">
  <img src="https://img.shields.io/badge/Blazor-WASM-512BD4?style=for-the-badge&logo=blazor&logoColor=white" alt="Blazor WASM">
  <img src="https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white" alt="SQL Server">
  <img src="https://img.shields.io/badge/EF_Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt="EF Core">
  <img src="https://img.shields.io/badge/Bootstrap_5.3-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white" alt="Bootstrap 5.3">
  <img src="https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white" alt="Docker">
</p>

<p align="center">
  <a href="#возможности">Возможности</a> •
  <a href="#быстрый-старт">Быстрый старт</a> •
  <a href="#структура-решения">Структура</a> •
  <a href="#api-эндпоинты">API</a> •
  <a href="#админ-панель">Админ-панель</a> •
  <a href="#архитектура">Архитектура</a> •
  <a href="#лаунчер">Лаунчер</a>
</p>

---

## Возможности

<table>
  <tr>
    <td><b>Товары</b></td>
    <td>Каталог с поиском, фильтрами, серверной пагинацией. Управление остатками, статусами, фотографиями. Batch-операции (смена статуса, удаление) — один HTTP-запрос</td>
  </tr>
  <tr>
    <td><b>Заказы</b></td>
    <td>Создание (автокомплит email, выбор товаров), смена статуса, просмотр состава. Фильтр по дате и статусу. Batch-смена статуса</td>
  </tr>
  <tr>
    <td><b>Пользователи</b></td>
    <td>CRUD, редактирование баланса, назначение ролей (Студент/Админ)</td>
  </tr>
  <tr>
    <td><b>Филиалы</b></td>
    <td>Настройки уведомлений по филиалам: email, Telegram, порог остатков. Привязка админов к филиалам</td>
  </tr>
  <tr>
    <td><b>История</b></td>
    <td>Лог всех действий с фильтрацией по типу и филиалу. Автопривязка к филиалу</td>
  </tr>
  <tr>
    <td><b>Подсветка поиска</b></td>
    <td>Найденный текст выделяется жирным с подчёркиванием во всех таблицах</td>
  </tr>
  <tr>
    <td><b>URL-фильтры</b></td>
    <td>Все поисковые запросы и фильтры сохраняются в URL — можно поделиться ссылкой с результатами</td>
  </tr>
  <tr>
    <td><b>Лаунчер</b></b></td>
    <td>Консольный лаунчер: автоматизация запуска, Docker, миграции, мониторинг с авто-рестартом</td>
  </tr>
</table>

---

## Быстрый старт

### Установка

1. **[.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)**
2. **SQL Server** (локальный или через Docker)

### Запуск через Launcher (рекомендуется)

```bash
dotnet run --project CifraShop.Launcher
```

Лаунчер автоматически:
- Проверит .NET SDK и Docker
- Создаст `docker-compose.yml` и строку подключения
- Запустит SQL Server в Docker
- Применит EF Core миграции (с авто-синхронизацией модели)
- Запустит API и клиент
- Предоставит интерактивное меню управления

Быстрый режим (пропуск завершённых фаз):
```bash
dotnet run --project CifraShop.Launcher -- --quick
```

### Ручной запуск

```bash
# Настройка строки подключения
cd CifraShop.API
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=.;Database=CifraShopDb;Trusted_Connection=True;TrustServerCertificate=True"

# Миграция
dotnet ef database update --project CifraShop.Infrastructure --startup-project CifraShop.API

# Запуск API (порт 5000)
dotnet run --project CifraShop.API

# Запуск клиента (порт 5001)
dotnet run --project CifraShop.Client
```

Откройте `http://localhost:5001/admin` — админ-панель.

---

## Структура решения

```
CifraShop/
├── CifraShop.slnx                         XML-формат решения (.NET 10)
│
├── CifraShop.Domain/                      Ядро — сущности, перечисления, интерфейсы репозиториев
│   ├── Entities/                          User, Product, Order, OrderItem, AdminAction, NotificationSettings, ProductImage
│   ├── Enums/                             UserRole, StatusProduct, StatusOrder
│   └── Repositories/                      IProductRepository, IOrderRepository, IUserRepository, ...
│
├── CifraShop.Infrastructure/              Инфраструктура — EF Core, SQL Server, реализации репозиториев
│   └── Data/
│       ├── ApplicationDbContext.cs        ApplicationContext (DbContext)
│       ├── Configurations/                Fluent API: все сущности
│       └── Repositories/Implementations/  EF Core реализации репозиториев
│
├── CifraShop.Application/                 Бизнес-логика — сервисы и их интерфейсы
│   └── Services/
│       ├── Interfaces/                    IOrderService, IProductService, IUserService, ...
│       └── Implementations/               OrderService, ProductService, UserService, ...
│
├── CifraShop.Contracts/                   DTO (Request/Response) + маппинг
│   ├── Requests/                          CreateProductRequest, BatchUpdateProductStatusRequest, ...
│   ├── Responses/                         ProductResponse, OrderResponse, PagedResponse<T>, ...
│   └── Mappings/                          Extension-методы: ToResponse()
│
├── CifraShop.API/                         ASP.NET Core Web API — точка входа
│   ├── Program.cs                         DI, CORS, middleware pipeline
│   ├── Controllers/                       7 контроллеров + ExceptionHandlerMiddleware
│   ├── Middleware/                         Единая обработка ошибок
│   └── Dockerfile                         multi-stage сборка для Docker
│
├── CifraShop.Client/                      Blazor WebAssembly — SPA-клиент
│   ├── Pages/Admin.razor                  Админ-панель (1900+ строк)
│   ├── Pages/Home.razor                   Главная страница
│   ├── Components/                        MetricCard, SearchBar, Pagination, Spinner, EmptyState
│   ├── Models/ApiModels.cs                Клиентские DTO (ProductDto, OrderDto, UserDto, ...)
│   ├── Models/Enums.cs                    Клиентские перечисления (зеркалит Domain)
│   └── wwwroot/                           Bootstrap 5.3.8 + Bootstrap Icons (локально)
│
├── CifraShop.Launcher/                    Консольный лаунчер — автоматизация запуска
│   └── Program.cs                         1600+ строк: 6 фаз + меню + мониторинг
│
├── CifraShop.Tests/                       MSTest (заготовка)
└── README.md                              Эта документация
```

### Поток данных

```
┌─────────────┐    HTTP     ┌─────────────┐
│  Client     │ ──────────> │  API        │
│  (Blazor)   │             │  Controllers│
└─────────────┘             └──────┬──────┘
                                   │
                            ┌──────▼──────┐
                            │ Application  │
                            │  Services    │
                            └──────┬──────┘
                                   │
                     ┌─────────────┼─────────────┐
              ┌──────▼──────┐           ┌──────▼──────┐
              │  Domain      │           │Infrastructure│
              │  Entities    │           │  EF Core    │
              └─────────────┘           └──────┬──────┘
                                               │
                                        ┌──────▼──────┐
                                        │  SQL Server  │
                                        └─────────────┘
```

---

## Описание проектов

### Domain — Ядро

Не зависит ни от одного проекта. Описывает данные и бизнес-правила.

**Сущности:**

| Сущность | Описание | Ключевые поля |
|----------|----------|---------------|
| `User` | Пользователь | Email, Password, Balance (int?), Role, Branch? |
| `Product` | Товар (INotifyPropertyChanged для Blazor) | Name, Description, Price (int), Quantity (int), Status, ImageUrl |
| `Order` | Заказ | Status, Sum (int), DateOfPurchase, CustomerId |
| `OrderItem` | Позиция заказа | OrderId, ProductId, Quantity (int), Price (int) |
| `AdminAction` | Действие администратора | ActionType, Details, Branch, CreatedAt |
| `NotificationSettings` | Настройки филиала | Branch, Email, TelegramBotToken, AdminEmails, NotifyOnNewOrder/StatusChange/LowStock |
| `ProductImage` | Фото товара | ProductId, FileName, IsPrimary, SortOrder |

**Перечисления:**

| Enum | Значения |
|------|----------|
| `UserRole` | Student (0), Admin (1) |
| `StatusProduct` | InStock (0), OutOfStock (1), ComingSoon (2) |
| `StatusOrder` | Pending (0), AwaitingPayment (1), Paid (2), Manufactured (3), Completed (4) |

### Infrastructure — Инфраструктура

Repository pattern: каждая сущность имеет интерфейс + EF Core реализацию. Ключевые методы:

```csharp
// Серверная пагинация
Task<(List<T> Items, int TotalCount)> GetAllPaged(int page, int pageSize, string? search, Status? status);

// Batch-операции (один SQL запрос вместо N)
Task UpdateStatusRange(List<int> ids, Status newStatus);  // ExecuteUpdateAsync
Task DeleteRange(List<int> ids);                          // ExecuteDeleteAsync
```

### Application — Бизнес-логика

Сервисы содержат валидацию и бизнес-правила. Контроллеры вызывают только сервисы (контроллер → сервис → репозиторий).

### Contracts — DTO

Клиент не знает про сущности — работает с DTO. Маппинг через extension-методы (`ToResponse()`).

| Request DTO | Response DTO | Mapping |
|-------------|-------------|---------|
| `CreateProductRequest` | `ProductResponse` | `ProductMapper.ToResponse()` |
| `CreateOrderRequest` | `OrderResponse` | `OrderMapping.ToResponse()` |
| `CreateUserRequest` | `UserResponse` | `UserMapping.ToResponse()` |
| `BatchUpdateProductStatusRequest` | — | — |
| `BatchUpdateOrderStatusRequest` | — | — |
| `BatchDeleteProductsRequest` | — | — |

### Client — Blazor WebAssembly

**Ключевые особенности:**
- DTO создаются локально в `Models/ApiModels.cs` (Client не ссылается на Contracts/Domain)
- **Bootstrap 5.3.8 + Bootstrap Icons** — загружены локально через libman
- **Никакого кастомного CSS и JS** — только Bootstrap utility classes + inline styles для свойств которых нет в Bootstrap
- `HttpClient.BaseAddress`: `https://localhost:5000/`
- Компоненты: `MetricCard`, `SearchBar` (debounce 300мс), `Pagination` (ellipsis для >7 страниц), `Spinner`, `EmptyState`

---

## API эндпоинты

### Товары

| Метод | Маршрут | Описание |
|-------|---------|----------|
| `GET` | `api/Product/all` | Все товары |
| `GET` | `api/Product/paged?page=0&pageSize=8&search=&status=` | С пагинацией, поиском и фильтром |
| `GET` | `api/Product/by-id?id=X` | По ID |
| `GET` | `api/Product/by-name?name=X` | По имени |
| `GET` | `api/Product/by-price?price=X` | По цене |
| `GET` | `api/Product/by-quantity?quantity=X` | По количеству |
| `GET` | `api/Product/by-status?statusProduct=X` | По статусу |
| `POST` | `api/Product/create-product` | Создать |
| `PUT` | `api/Product/update-product?id=X` | Обновить (Name, Description, Price, Quantity, Status) |
| `DELETE` | `api/Product/delete-product?id=X` | Удалить |
| `POST` | `api/Product/batch-delete` | Массовое удаление (ProductIds) |
| `POST` | `api/Product/batch-update-status` | Массовая смена статуса (ProductIds, NewStatus) |

### Заказы

| Метод | Маршрут | Описание |
|-------|---------|----------|
| `GET` | `api/Order/all` | Все заказы |
| `GET` | `api/Order/paged?page=0&pageSize=8&search=&status=&dateFrom=&dateTo=` | С пагинацией и фильтрами |
| `GET` | `api/Order/by-id?id=X` | По ID |
| `GET` | `api/Order/by-email?email=X` | По email покупателя |
| `GET` | `api/Order/by-status?status=X` | По статусу |
| `GET` | `api/Order/by-sum?sum=X` | По сумме |
| `GET` | `api/Order/by-date?from=&to=` | По диапазону дат |
| `POST` | `api/Order/create-order` | Создать (CustomerEmail, Items[{ProductId, Quantity}]) |
| `PUT` | `api/Order/update-order?id=X` | Обновить статус |
| `POST` | `api/Order/batch-update-status` | Массовая смена статуса (OrderIds, NewStatus) |
| `DELETE` | `api/Order/delete-order?id=X` | Удалить |

### Позиции заказов

| Метод | Маршрут | Описание |
|-------|---------|----------|
| `GET` | `api/OrderItem/by-id?id=X` | По ID |
| `GET` | `api/OrderItem/by-orderid?id=X` | По ID заказа |
| `POST` | `api/OrderItem/create-orderitem` | Создать |
| `PUT` | `api/OrderItem/update-orderitem?id=X` | Обновить |
| `DELETE` | `api/OrderItem/delete-order-item?id=X` | Удалить |

### Пользователи

| Метод | Маршрут | Описание |
|-------|---------|----------|
| `GET` | `api/User/all` | Все пользователи |
| `GET` | `api/User/paged?page=0&pageSize=8&search=&role=` | С пагинацией и фильтром по роли |
| `GET` | `api/User/all-admins` | Только админы |
| `GET` | `api/User/all-students` | Только студенты |
| `GET` | `api/User/by-id?id=X` | По ID |
| `GET` | `api/User/by-email?email=X` | По email |
| `POST` | `api/User/create-admin` | Создать админа |
| `POST` | `api/User/create-student` | Создать студента |
| `PUT` | `api/User?id=X` | Обновить (email, пароль, баланс) |
| `DELETE` | `api/User?id=X` | Удалить |

### Филиалы и уведомления

| Метод | Маршрут | Описание |
|-------|---------|----------|
| `GET` | `api/NotificationSettings/all` | Все настройки |
| `GET` | `api/NotificationSettings/by-id?id=X` | По ID |
| `GET` | `api/NotificationSettings/by-branch?branch=X` | По филиалу |
| `POST` | `api/NotificationSettings` | Создать |
| `PUT` | `api/NotificationSettings?id=X` | Обновить |
| `DELETE` | `api/NotificationSettings?id=X` | Удалить |

### Фотографии товаров

| Метод | Маршрут | Описание |
|-------|---------|----------|
| `GET` | `api/ProductImage/by-product?productId=X` | Все фото товара |
| `GET` | `api/ProductImage/file/{fileName}` | Получить файл изображения |
| `POST` | `api/ProductImage/upload` | Загрузить фото (multipart: file, productId, isPrimary) |
| `POST` | `api/ProductImage/set-primary?imageId=X` | Сделать главным |
| `DELETE` | `api/ProductImage?id=X` | Удалить |

### История действий

| Метод | Маршрут | Описание |
|-------|---------|----------|
| `GET` | `api/AdminAction/last?count=50&branch=X` | Последние N действий |
| `POST` | `api/AdminAction` | Записать действие (ActionType, Details, Branch) |

---

## Админ-панель

Страница `/admin` — полноценная панель управления магазином.

### Метрики (4 карточки)

| Метрика | Показывает | Источник данных |
|---------|-----------|-----------------|
| **Товаров** | Количество + предупреждение о низком остатке (≤5) + стоимость на складе | `api/Product/all` |
| **Заказы** | Количество + общая выручка | `api/Order/all` |
| **Пользователи** | Количество + сколько админов | `api/User/all` |
| **Филиалы** | Количество филиалов + сколько с настроенными каналами | `api/NotificationSettings/all` |

### Товары

- **Поиск** — debounce 300мс, по названию и описанию
- **Подсветка поиска** — найденный текст выделяется `<strong>` с подчёркиванием
- **Фильтр** — по статусу (В наличии / Нет в наличии / Скоро в продаже)
- **Пагинация** — серверная, размер страницы: 5/8/16/32
- **Ellipsis пагинация** — при >7 страниц: `1 ... 5 6 7 ... 10`
- **Создание** — модалка (название, описание, цена, количество, URL изображения)
- **Редактирование** — модалка + управление фотографиями товара
- **Загрузка фото** — Bootstrap-styled `InputFile` (клик для выбора)
- **Количество** — инлайн-редактирование в таблице (сразу отправляет PUT-запрос)
- **Статус** — клик по бейджу: цикл В наличии → Нет → Скоро
- **Низкий остаток** — строки с ≤5 штук подсвечиваются жёлтым
- **Batch-операции** — чекбоксы + batch-смена статуса (один запрос) / batch-удаление
- **Счётчик выбранных** — badge рядом с чекбоксом «выбрать все»

### Заказы

- **Поиск** — debounce 300мс, по номеру и email
- **Подсветка поиска** — найденный текст выделяется
- **Фильтр** — по статусу и диапазону дат
- **Пагинация** — серверная, ellipsis
- **Статус** — выпадающий список в таблице
- **Детали** — клик по строке раскрывает состав с датой и статусом
- **Batch-операции** — чекбоксы + batch-смена статуса (один запрос)
- **Создание** — модалка (автокомплит email + выбор товаров с расчётом итого)
- **Счётчик выбранных** — badge рядом с чекбоксом «выбрать все»

### Пользователи

- **Поиск** — debounce 300мс, по email
- **Подсветка поиска** — найденный текст выделяется
- **Фильтр** — по роли (Студенты / Админы)
- **Пагинация** — серверная, ellipsis
- **Создание** — модалка (email, пароль, роль)
- **Баланс** — клик по балансу → модалка редактирования

### Филиалы и уведомления

Настройки по каждому филиалу:
- Название филиала
- Email для уведомлений
- Telegram Bot Token + Chat ID
- Email администраторов филиала (привязка админов к филиалу)
- Типы уведомлений: новые заказы, смена статуса, низкий остаток
- Порог уведомления о низком запасе

### История действий

Сворачиваемая панель с фильтрацией:
- **По типу** — Создание / Изменение / Удаление
- **По филиалу** — выпадающий список филиалов
- **Временная шкала** — дата, время, цветовая полоса по типу действия

### Уведомления (Toast)

Всплывающие сообщения: зелёное (успех), красное (ошибка), жёлтое (предупреждение). Автоскрытие 4 сек. При новом уведомлении предыдущее отменяется.

### URL-фильтры

Все поисковые запросы и фильтры автоматически сохраняются в URL query-параметрах:
- `ps` — поиск товаров
- `pf` — фильтр товаров
- `os` — поиск заказов
- `of` — фильтр заказов
- `odf` — дата начала
- `odt` — дата окончания
- `us` — поиск пользователей
- `uf` — фильтр пользователей

При обновлении страницы фильтры восстанавливаются из URL. Можно поделиться ссылкой с результатами поиска.

### Горячие клавиши

- **Escape** — закрыть активное модальное окно / подтверждение

---

## Архитектура

### Clean Architecture

```
┌─────────────────────────────────────────────────────┐
│  Presentation                                        │
│  ├── API Controllers (REST endpoints)               │
│  └── Client Pages (Blazor WASM)                     │
├─────────────────────────────────────────────────────┤
│  Application                                         │
│  ├── Service Interfaces (IOrderService, ...)        │
│  └── Service Implementations (validation, business) │
├─────────────────────────────────────────────────────┤
│  Contracts                                           │
│  ├── Request DTOs                                    │
│  ├── Response DTOs                                   │
│  └── Mappings (extension methods)                    │
├─────────────────────────────────────────────────────┤
│  Domain (no dependencies)                            │
│  ├── Entities (User, Product, Order, ...)           │
│  ├── Enums (UserRole, StatusOrder, ...)             │
│  └── Repository Interfaces                           │
├─────────────────────────────────────────────────────┤
│  Infrastructure                                      │
│  ├── EF Core DbContext                               │
│  ├── Fluent API Configurations                       │
│  └── Repository Implementations                      │
└─────────────────────────────────────────────────────┘
```

### ExceptionHandlerMiddleware

Единая точка обработки необработанных исключений.

| Исключение | HTTP-код | Пример |
|-----------|----------|--------|
| `ArgumentException` | 400 Bad Request | Невалидные данные |
| `KeyNotFoundException` | 404 Not Found | Ресурс не найден |
| `UnauthorizedAccessException` | 401 Unauthorized | Доступ запрещён |
| `InvalidOperationException` | 409 Conflict | Недостаточно товара |
| `OverflowException` | 400 Bad Request | Сумма заказа слишком велика |
| Остальные | 500 Internal Server Error | Внутренняя ошибка |

---

## Ламчер

Консольный лаунчер (`CifraShop.Launcher`) автоматизирует запуск всего проекта.

### 6 фаз запуска

| Фаза | Описание |
|------|----------|
| 1 | Проверка .NET SDK и Docker |
| 2 | Создание `docker-compose.yml` и строки подключения (если нет) |
| 3 | Запуск SQL Server через Docker (или проверка локального) |
| 4 | Применение EF Core миграций (с авто-синхронизацией модели) |
| 5 | Запуск API (Docker или локально — автоматический выбор) |
| 6 | Запуск Blazor-клиента |

### Интерактивное меню

```
╭─ МЕНЮ УПРАВЛЕНИЯ ───────────────────────────────────────╮
│  [1] Открыть админ-панель                                │
│  [2] Открыть главную страницу                            │
│  ─────────────────────────────────────────────────────── │
│  [3] Перезапустить API                                   │
│  [4] Перезапустить клиент                                │
│  [5] Перезапустить всё                                   │
│  ─────────────────────────────────────────────────────── │
│  [6] Остановить всё                                      │
│  ─────────────────────────────────────────────────────── │
│  [7] Пересобрать API (Docker)                            │
│  [8] Очистить Docker-образы                              │
│  ─────────────────────────────────────────────────────── │
│  [9] Показать логи                                       │
│  [0] Выход                                               │
╰─────────────────────────────────────────────────────────╯
```

### Мониторинг и авто-рестарт

- Фоновый `MonitorProcessesAsync` проверяет процессы каждые 5 сек
- При падении API или клиента — автоматический перезапуск (до 3 раз)
- После 3 падений — отключение авто-рестарта с предупреждением
- Счётчики сбрасываются при ручном перезапуске через меню

### Быстрый режим

```bash
dotnet run --project CifraShop.Launcher -- --quick
```
Пропускает фазы, которые уже выполнены (БД запущена, миграции применены).

### CLI-флаги

| Флаг | Описание |
|------|----------|
| `--quick` | Быстрый режим — пропуск завершённых фаз |
| `--skip-docker` | Принудительно локальный режим (без Docker) |
| `--port-api N` | Переопределение порта API |
| `--port-client N` | Переопределение порта клиента |
| `--status` | Показать статус сервисов и выйти |
| `--version` | Показать версию и выйти |
| `--reset` | Полный сброс: остановка контейнеров, очистка образов, удаление маркеров |

### Health-check endpoint

API предоставляет `GET /health` для надёжной проверки состояния:
```json
{"status": "healthy", "timestamp": "2025-01-01T00:00:00Z"}
```

---

## Порты

| Сервис | Протокол | Порт | URL |
|--------|----------|------|-----|
| API | HTTPS | 5000 | `https://localhost:5000` |
| Client | HTTP | 5001 | `http://localhost:5001` |
| SQL Server | TCP | 1433 | `localhost:1433` |
| Docker API | HTTPS | 8085 | `https://localhost:8085` |

> **Примечание:** Клиент использует HTTP, т.к. .NET 10 WasmAppHost dev-сервер выбрасывает `InvalidOperationException` при HTTPS.

---

## Конвенции

| Правило | Описание |
|---------|----------|
| **Язык** | Имена классов/методов на английском, комментарии и UI на русском |
| **Nullable reference types** | Включены во всех проектах |
| **Implicit usings** | Включены |
| **Типы данных** | `int` для Price/Quantity/Sum/Balance (не short — переполнение) |
| **Стили** | **Запрещён кастомный CSS и JS.** Только Bootstrap utility classes. Inline `style=""` только для свойств которых нет в Bootstrap: `cursor:pointer`, `width/max-width` в `rem` для таблиц, `font-size` < `small`, `background-color` для модалок, `min-width`, `object-fit:cover` |
| **Иконки** | Bootstrap Icons (локально через libman) |
| **Коммиты** | Описание на русском языке |
| **Формат решения** | `.slnx` (XML-based, .NET 10) |
| **Пагинация** | Серверная, ellipsis при >7 страниц |
| **Поиск** | Debounce 300мс через CancellationTokenSource |
| **URL-фильтры** | Все фильтры сохраняются в URL query-параметрах |

---

## Исправленные баги

### Лаунчер
- `RunCmdAsync` возвращал non-null при ошибке команды — Docker daemon down ломал проверки
- Deadlock при последовательном чтении stdout/stderr — теперь параллельное
- Двойная граница в `PrintStatusLine` — `PadLine` уже записывал `│\n`
- Docker build проверка через строковый поиск — заменена на exit code
- `KillPortAsync` не убивал дерево процессов и не ждал освобождения порта

### Админ-панель
- **XSS через MarkupString** — `HighlightText` не экранировал HTML-теги в пользовательских данных
- Общий `_pageSize` для всех секций — заменён на `_productPageSize`, `_orderPageSize`, `_userPageSize`
- Сортировка работала только на текущей странице — client-side sort
- `_productTotalCount` отслеживался вручную — теперь перезагружается с сервера
- `SaveProduct` не отправлял `ImageUrl` при создании
- Race condition в `ShowNotification` — таймер без CancellationTokenSource
- `Dispose` не отменял CTS перед удалением — pending delays продолжали работать
- Batch-операции логировали даже при ошибке API
- `CloseProductModal` не сбрасывал `_saving` — модалка блокировалась
- `RefreshLoop` без try-catch — падал молча, автообновление прекращалось
- Даты заказов не синхронизировались с URL
- `_expandedOrderId` не сбрасывался при перезагрузке заказов
- `ToggleProductStatus` / `ChangeOrderStatus` — нет feedback при ошибке API
- Двойное добавление при создании товаров/пользователей — `Add` + `Load` → только `Load`

---

## Дорожная карта

### Сделано

- [x] CRUD для товаров, заказов, пользователей
- [x] Админ-панель с поиском, фильтрами, серверной пагинацией
- [x] Batch-операции: смена статуса (товары + заказы), удаление (товары)
- [x] Серверная пагинация с ellipsis
- [x] Debounce поиска (300мс) через CancellationTokenSource
- [x] Подсветка поиска (HTML-безопасная, через MarkupString + HtmlEncode)
- [x] URL-фильтры (NavigationManager, восстановление при загрузке)
- [x] Нативные тултипы (HTML title) на обрезанных текстах
- [x] Загрузка фотографий (Bootstrap-styled InputFile)
- [x] История действий с фильтрацией по типу и филиалу
- [x] ExceptionHandlerMiddleware (единая обработка ошибок)
- [x] DateTime.UtcNow (все временные метки в UTC)
- [x] Настройки филиалов (уведомления, привязка админов)
- [x] Консольный лаунчер (автоматизация запуска + мониторинг)
- [x] Bootstrap Icons локально (без CDN)
- [x] Исправлено 20+ багов (XSS, batch-логика, memory leaks, data consistency)

### Осталось

- [ ] Аутентификация (JWT)
- [ ] Хеширование паролей
- [ ] Валидация DTO (FluentValidation)
- [ ] Unit-тесты (MSTest)
- [ ] Страница каталога для студентов
- [ ] Корзина и оформление заказа
- [ ] Реальные уведомления (email/Telegram)
- [ ] Серверная сортировка (sortBy/sortDir параметры в API)
- [ ] Навигация клавиатурой (Tab между секциями, Enter для модалок)

---

<p align="center">
  Проект разработан в рамках курса <b>Cifra</b>
</p>
