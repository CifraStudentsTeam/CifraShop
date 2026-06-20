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
  <img src="https://img.shields.io/badge/Bootstrap-5.3-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white" alt="Bootstrap 5.3">
  <img src="https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white" alt="Docker">
</p>

<p align="center">
  <a href="#возможности">Возможности</a> •
  <a href="#быстрый-старт">Быстрый старт</a> •
  <a href="#структура-решения">Структура</a> •
  <a href="#api-эндпоинты">API</a> •
  <a href="#админ-панель">Админ-панель</a> •
  <a href="#архитектура">Архитектура</a>
</p>

---

## Возможности

<table>
  <tr>
    <td><b>Товары</b></td>
    <td>Каталог с поиском, фильтрами, пагинацией. Управление остатками, статусами, фотографиями</td>
  </tr>
  <tr>
    <td><b>Заказы</b></td>
    <td>Создание, смена статуса, просмотр состава. Фильтр по дате и статусу</td>
  </tr>
  <tr>
    <td><b>Пользователи</b></td>
    <td>CRUD, редактирование баланса, назначение ролей</td>
  </tr>
  <tr>
    <td><b>Филиалы</b></td>
    <td>Настройки уведомлений по филиалам: email, Telegram, порог остатков</td>
  </tr>
  <tr>
    <td><b>История</b></td>
    <td>Лог всех действий с фильтрацией по типу и филиалу</td>
  </tr>
  <tr>
    <td><b>Batch-операции</b></td>
    <td>Массовая смена статуса, массовое удаление — один HTTP-запрос</td>
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

Лаунчер автоматически проверит зависимости, настроит БД, применит миграции и запустит серверы.

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

### Docker

```bash
docker compose up -d
# API: https://localhost:8085
# Client: http://localhost:8086
```

---

## Структура решения

```
CifraShop/
├── CifraShop.slnx                         XML-формат решения (.NET 10)
│
├── CifraShop.Domain/                      Ядро — сущности, перечисления, интерфейсы репозиториев
│   ├── Entities/                          User, Product, Order, OrderItem, AdminAction, NotificationSettings, ProductImage
│   ├── Enums/                             UserRole, StatusProduct, StatusOrder
│   └── Repositories/                      IProductRepository, IOrderRepository, ...
│
├── CifraShop.Infrastructure/              Инфраструктура — EF Core, SQL Server, реализации репозиториев
│   └── Data/
│       ├── ApplicationDbContext.cs        ApplicationContext (DbContext)
│       ├── Configurations/                Fluent API: AdminAction, NotificationSettings, Order, OrderItem, Product, User
│       └── Repositories/Implementations/  EF Core реализации всех репозиториев
│
├── CifraShop.Application/                 Бизнес-логика — сервисы и их интерфейсы
│   └── Services/
│       ├── Interfaces/                    IOrderService, IProductService, IUserService, ...
│       └── Implementations/               OrderService, ProductService, UserService, ...
│
├── CifraShop.Contracts/                   DTO (Request/Response) + маппинг
│   ├── Requests/                          CreateOrderRequest, CreateProductRequest, ...
│   ├── Responses/                         OrderResponse, ProductResponse, PagedResponse<T>, ...
│   └── Mappings/                          Extension-методы: ToResponse()
│
├── CifraShop.API/                         ASP.NET Core Web API — точка входа
│   ├── Program.cs                         DI, CORS, middleware pipeline
│   ├── Controllers/                       7 контроллеров: Order, Product, User, OrderItem, AdminAction, NotificationSettings, ProductImage
│   └── Middleware/                        ExceptionHandlerMiddleware
│
├── CifraShop.Client/                      Blazor WebAssembly — SPA-клиент
│   ├── Pages/Admin.razor                  Админ-панель (1600+ строк)
│   ├── Pages/Home.razor                   Главная страница
│   ├── Components/                        MetricCard, SearchBar, Pagination, Spinner, EmptyState
│   ├── Models/ApiModels.cs                Клиентские DTO
│   └── wwwroot/bootstrap-icons/           Bootstrap Icons (локально)
│
├── CifraShop.Launcher/                    Консольный лаунчер — автоматизация запуска
└── CifraShop.Tests/                       MSTest (заготовка)
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
| `User` | Пользователь | Email, Password, Balance, Role, Branch? |
| `Product` | Товар (INotifyPropertyChanged) | Name, Description, Price, Quantity, Status, ImageUrl |
| `Order` | Заказ | Status, Sum, DateOfPurchase, CustomerId |
| `OrderItem` | Позиция заказа | OrderId, ProductId, Quantity, Price |
| `AdminAction` | Действие администратора | ActionType, Details, Branch, CreatedAt |
| `NotificationSettings` | Настройки филиала | Branch, Email, TelegramBotToken, AdminEmails |
| `ProductImage` | Фото товара | ProductId, FileName, IsPrimary, SortOrder |

**Перечисления:**

| Enum | Значения |
|------|----------|
| `UserRole` | Student, Admin |
| `StatusProduct` | InStock (0), OutOfStock (1), ComingSoon (2) |
| `StatusOrder` | Pending (0), AwaitingPayment (1), Paid (2), Manufactured (3), Completed (4) |

### Infrastructure — Инфраструктура

«Мост» между C# кодом и БД. Repository pattern: каждая сущность имеет интерфейс + EF Core реализацию.

```csharp
public interface IProductRepository
{
    Task<List<Product>> GetAll();
    Task<(List<Product> Items, int TotalCount)> GetAllPaged(int page, int pageSize, string? search, StatusProduct? status);
    Task<Product?> GetProductById(int id);
    Task AddProduct(Product productToAdd);
    Task UpdateProduct(Product productToUpdate);
    Task DeleteProduct(Product productToDelete);
    Task DeleteRange(List<int> ids);
}
```

### Application — Бизнес-логика

Сервисы — посредники между контроллерами и репозиториями. Содержат валидацию и бизнес-правила.

```csharp
public async Task<Order> CreateOrder(string customerEmail, List<(int ProductId, short Quantity)> items)
{
    var user = await _userRepository.GetUserByEmail(customerEmail);
    if (user == null) throw new KeyNotFoundException("Пользователь не найден");

    // Проверка остатков, расчёт суммы, создание в транзакции
    return await _repository.CreateOrderInTransaction(order, orderItems, stockUpdates);
}
```

### Contracts — DTO

Клиент не знает про сущности — работает с DTO. Маппинг через extension-методы.

| Request DTO | Response DTO | Mapping |
|-------------|-------------|---------|
| `CreateProductRequest` | `ProductResponse` | `ProductMapper.ToResponse()` |
| `CreateOrderRequest` | `OrderResponse` | `OrderMapping.ToResponse()` |
| `CreateUserRequest` | `UserResponse` | `UserMapping.ToResponse()` |
| `CreateAdminActionRequest` | `AdminActionResponse` | — |
| `CreateNotificationSettingsRequest` | `NotificationSettingsResponse` | — |
| `BatchUpdateOrderStatusRequest` | `PagedResponse<T>` | — |
| `BatchDeleteProductsRequest` | | |

### API — Точка входа

- **Minimal hosting** (нет `Startup.cs`)
- **ExceptionHandlerMiddleware** — единая обработка ошибок
- **CORS** политика `ClientCORS`

### Client — Blazor WebAssembly

- DTO создаются локально в `Models/ApiModels.cs`
- **Компоненты:** MetricCard (метрики), SearchBar (поиск + фильтр), Pagination, Spinner, EmptyState
- **Bootstrap 5.3.8 + Bootstrap Icons** — загружены локально через libman
- HttpClient.BaseAddress: `https://localhost:5000/`

### Launcher — Консольный лаунчер

Автоматизирует запуск проекта:
1. Проверка .NET SDK и Docker
2. Создание docker-compose.yml и строки подключения
3. Запуск SQL Server через Docker
4. Применение миграций (с автосинхронизацией)
5. Запуск API и клиента
6. Интерактивное меню управления

---

## API эндпоинты

### Товары

| Метод | Маршрут | Описание |
|-------|---------|----------|
| `GET` | `api/Product/all` | Все товары |
| `GET` | `api/Product/paged?page=0&pageSize=8` | С пагинацией, поиском и фильтром |
| `GET` | `api/Product/by-id?id=X` | По ID |
| `GET` | `api/Product/by-name?name=X` | По имени |
| `GET` | `api/Product/by-price?price=X` | По цене |
| `GET` | `api/Product/by-quantity?quantity=X` | По количеству |
| `GET` | `api/Product/by-status?statusProduct=X` | По статусу |
| `POST` | `api/Product/create-product` | Создать |
| `PUT` | `api/Product/update-product?id=X` | Обновить |
| `DELETE` | `api/Product/delete-product?id=X` | Удалить |
| `POST` | `api/Product/batch-delete` | Массовое удаление |

### Заказы

| Метод | Маршрут | Описание |
|-------|---------|----------|
| `GET` | `api/Order/all` | Все заказы |
| `GET` | `api/Order/paged?page=0&pageSize=8` | С пагинацией, поиском, фильтрами по статусу и дате |
| `GET` | `api/Order/by-id?id=X` | По ID |
| `GET` | `api/Order/by-email?email=X` | По email покупателя |
| `GET` | `api/Order/by-status?status=X` | По статусу |
| `GET` | `api/Order/by-sum?sum=X` | По сумме |
| `GET` | `api/Order/by-date?from=&to=` | По диапазону дат |
| `POST` | `api/Order/create-order` | Создать |
| `PUT` | `api/Order/update-order?id=X` | Обновить статус |
| `POST` | `api/Order/batch-update-status` | Массовая смена статуса |
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
| `GET` | `api/User/paged?page=0&pageSize=8` | С пагинацией и фильтром по роли |
| `GET` | `api/User/all-admins` | Только админы |
| `GET` | `api/User/all-students` | Только студенты |
| `GET` | `api/User/by-id?id=X` | По ID |
| `GET` | `api/User/by-email?email=X` | По email |
| `POST` | `api/User/create-admin` | Создать админа |
| `POST` | `api/User/create-student` | Создать студента |
| `PUT` | `api/User?id=X` | Обновить (email, пароль, баланс) |
| `DELETE` | `api/User?id=X` | Удалить |

### Настройки филиалов и уведомлений

| Метод | Маршрут | Описание |
|-------|---------|----------|
| `GET` | `api/NotificationSettings/all` | Все настройки |
| `GET` | `api/NotificationSettings/by-id?id=X` | По ID |
| `GET` | `api/NotificationSettings/by-branch?branch=X` | По названию филиала |
| `POST` | `api/NotificationSettings` | Создать |
| `PUT` | `api/NotificationSettings?id=X` | Обновить |
| `DELETE` | `api/NotificationSettings?id=X` | Удалить |

### История действий

| Метод | Маршрут | Описание |
|-------|---------|----------|
| `GET` | `api/AdminAction/last?count=50&branch=X` | Последние N действий (фильтр по филиалу) |
| `POST` | `api/AdminAction` | Записать действие |

---

## Админ-панель

Страница `/admin` — полноценная панель управления магазином.

### Метрики (4 карточки)

| Метрика | Показывает |
|---------|-----------|
| **Товаров** | Количество + предупреждение о низком остатке (≤5) |
| **Заказы** | Количество + общая выручка |
| **Пользователи** | Количество + сколько админов |
| **Филиалы** | Количество филиалов + сколько с настроенными каналами |

### Товары

- **Поиск** — debounce 300мс, по названию и описанию
- **Фильтр** — по статусу (В наличии / Нет / Скоро)
- **Пагинация** — серверная, по 8 товаров
- **Создание** — модалка (название, описание, цена, количество, URL изображения)
- **Редактирование** — модалка + управление фотографиями товара
- **Количество** — инлайн-редактирование в таблице
- **Статус** — клик по бейджу: цикл В наличии → Нет → Скоро
- **Низкий остаток** — строки с ≤5 штук подсвечиваются жёлтым
- **Массовые действия** — чекбоксы: Показать / Скрыть / Удалить

### Заказы

- **Поиск** — debounce 300мс, по номеру и email
- **Фильтр** — по статусу и диапазону дат
- **Пагинация** — серверная, по 8 заказов
- **Статус** — выпадающий список в таблице
- **Детали** — клик по строке раскрывает состав с датой и статусом
- **Массовые действия** — чекбоксы + batch-смена статуса
- **Создание** — модалка (автокомплит email + выбор товаров)

### Пользователи

- **Поиск** — debounce 300мс, по email
- **Фильтр** — по роли (Студенты / Админы)
- **Пагинация** — серверная, по 8 пользователей
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
- **Привязка к филиалу** — каждое действие логируется с указанием филиала

### Уведомления (Toast)

Всплывающие сообщения: зелёное (успех), красное (ошибка), жёлтое (предупреждение). Автоскрытие 4 сек.

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

## Порты

| Сервис | Протокол | Порт | URL |
|--------|----------|------|-----|
| API | HTTPS | 5000 | `https://localhost:5000` |
| Client | HTTP | 5001 | `http://localhost:5001` |
| Docker (HTTPS) | HTTPS | 8085 | `https://localhost:8085` |
| Docker (HTTP) | HTTP | 8086 | `http://localhost:8086` |

> **Примечание:** Клиент использует HTTP, т.к. .NET 10 WasmAppHost dev-сервер не поддерживает HTTPS.

---

## Конвенции

| Правило | Описание |
|---------|----------|
| **Язык** | Имена классов/методов на английском, комментарии и UI на русском |
| **Nullable reference types** | Включены во всех проектах |
| **Implicit usings** | Включены |
| **Типы** | `short` для Price/Quantity, `int` для ID |
| **Стили** | Только Bootstrap CSS (никакого кастомного CSS) |
| **Иконки** | Bootstrap Icons (загружены локально через libman) |
| **Коммиты** | Описание на русском языке |
| **Формат решения** | `.slnx` (XML-based, .NET 10) |

---

## Дорожная карта

### Сделано

- [x] CRUD для товаров, заказов, пользователей
- [x] Админ-панель с поиском, фильтрами, пагинацией
- [x] Batch-операции (массовое удаление, смена статуса)
- [x] Серверная пагинация
- [x] Debounce поиска (300мс)
- [x] История действий с фильтрацией по типу и филиалу
- [x] ExceptionHandlerMiddleware (единая обработка ошибок)
- [x] DateTime.UtcNow (все временные метки в UTC)
- [x] Настройки филиалов (уведомления, привязка админов)
- [x] Фотографии товаров (загрузка, выбор главного)
- [x] Консольный лаунчер (автоматизация запуска)
- [x] Bootstrap Icons локально (без CDN)
- [x] Визуальные улучшения (метрики, индикаторы, предупреждения)
- [x] Исправлены баги (OrderId, валидация, маппинг)

### Осталось

- [ ] Аутентификация (JWT)
- [ ] Хеширование паролей
- [ ] Валидация DTO
- [ ] Unit-тесты
- [ ] Seed-данные
- [ ] Страница каталога для студентов
- [ ] Корзина и оформление заказа
- [ ] Реальные уведомления (email/Telegram)

---

<p align="center">
  Проект разработан в рамках курса <b>Cifra</b>
</p>
