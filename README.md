<h1 align="center">
  <br>
  CifraShop
  <br>
</h1>

<h4 align="center">Полнофункциональный интернет-магазин на .NET 10 с реалтайм-обновлениями</h4>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 10">
  <img src="https://img.shields.io/badge/Blazor-WASM-512BD4?style=for-the-badge&logo=blazor&logoColor=white" alt="Blazor WASM">
  <img src="https://img.shields.io/badge/SignalR-0078D4?style=for-the-badge&logo=microsoft&logoColor=white" alt="SignalR">
  <img src="https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white" alt="SQL Server">
  <img src="https://img.shields.io/badge/EF_Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt="EF Core">
  <img src="https://img.shields.io/badge/Bootstrap_5.3-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white" alt="Bootstrap 5.3">
  <img src="https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white" alt="Docker">
</p>

<p align="center">
  <a href="#возможности">Возможности</a> &bull;
  <a href="#быстрый-старт">Быстрый старт</a> &bull;
  <a href="#структура-проекта">Структура</a> &bull;
  <a href="#api-эндпоинты">API</a> &bull;
  <a href="#админ-панель">Админ-панель</a> &bull;
  <a href="#архитектура">Архитектура</a> &bull;
  <a href="#лаунчер">Лаунчер</a>
</p>

---

## Возможности

<table>
  <tr>
    <td><b>Обновления в реальном времени</b></td>
    <td>SignalR — все изменения (товары, заказы, пользователи) отображаются мгновенно на всех подключённых вкладках админки без перезагрузки. Автопереподключение с fallback-опросом каждые 5 минут</td>
  </tr>
  <tr>
    <td><b>Товары</b></td>
    <td>Полный CRUD, управление фотографиями, инлайн-редактирование количества, переключение статуса, batch-операции (смена статуса, удаление). Серверный поиск с debounce 300мс</td>
  </tr>
  <tr>
    <td><b>Заказы</b></td>
    <td>Создание с автокомплитом email, управление статусами, развёрнутый состав с изображениями товаров, фильтр по дате, batch-смена статуса</td>
  </tr>
  <tr>
    <td><b>Пользователи</b></td>
    <td>CRUD с назначением ролей (Студент/Админ), редактирование баланса</td>
  </tr>
  <tr>
    <td><b>Уведомления</b></td>
    <td>Настройки уведомлений по филиалам: email, Telegram Bot, пороги остатков. Привязка админов к филиалам</td>
  </tr>
  <tr>
    <td><b>История действий</b></td>
    <td>Полный аудит-лог с фильтрацией по типу и филиалу. Автоматическая временная метка</td>
  </tr>
  <tr>
    <td><b>Поиск и фильтры</b></td>
    <td>Подсветка совпадений, серверная пагинация, URL-фильтры (можно поделиться ссылкой)</td>
  </tr>
  <tr>
    <td><b>Лаунчер</b></td>
    <td>Запуск одной командой: Docker, БД, миграции, API, клиент. Интерактивное меню с мониторингом и авто-рестартом</td>
  </tr>
</table>

---

## Быстрый старт

### Требования

1. **[.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)**
2. **Docker Desktop** (для SQL Server)

### Вариант А: Через лаунчер (рекомендуется)

```bash
dotnet run --project CifraShop.Launcher
```

Лаунчер автоматически:
- Проверит .NET SDK и Docker
- Создаст `docker-compose.yml` и строку подключения
- Запустит SQL Server в Docker
- Применит EF Core миграции
- Запустит API и клиент
- Предоставит интерактивное меню управления

Быстрый режим (пропуск выполненных фаз):
```bash
dotnet run --project CifraShop.Launcher -- --quick
```

### Вариант Б: Ручной запуск

```bash
# Запуск SQL Server
docker compose up -d db

# Применение миграций
dotnet ef database update --project CifraShop.Infrastructure --startup-project CifraShop.API

# Запуск API (порт 5000)
dotnet run --project CifraShop.API --urls http://localhost:5000

# Запуск клиента (порт 5001)
dotnet run --project CifraShop.Client
```

Откройте **http://localhost:5001/admin** — админ-панель.

---

## Структура проекта

```
CifraShop/
├── CifraShop.slnx                     Решение .NET 10 (XML-формат)
│
├── CifraShop.Domain/                  Ядро — сущности, перечисления, интерфейсы репозиториев
│   ├── Entities/                      User, Product, Order, OrderItem, AdminAction и др.
│   ├── Enums/                         UserRole, StatusProduct, StatusOrder
│   └── Repositories/                  IProductRepository, IOrderRepository, ...
│
├── CifraShop.Infrastructure/          EF Core, SQL Server, реализации репозиториев
│   └── Data/
│       ├── ApplicationDbContext.cs    ApplicationContext (DbContext)
│       ├── Configurations/            Конфигурации Fluent API
│       └── Repositories/Implementations/
│
├── CifraShop.Application/             Бизнес-логика — сервисы
│   └── Services/
│       ├── Interfaces/                IOrderService, IProductService, ...
│       └── Implementations/           OrderService, ProductService, ...
│
├── CifraShop.Contracts/               DTO (Request/Response) + маппинг
│   ├── Requests/                      CreateProductRequest, BatchUpdateRequest, ...
│   ├── Responses/                     ProductResponse, PagedResponse<T>, ...
│   └── Mappings/                      Extension-методы: ToResponse()
│
├── CifraShop.API/                     ASP.NET Core Web API — точка входа
│   ├── Program.cs                     DI, CORS, SignalR, middleware
│   ├── Controllers/                   7 контроллеров + ExceptionHandlerMiddleware
│   ├── Hubs/                          AdminHub (SignalR)
│   └── Dockerfile                     Мультистадийная сборка Docker
│
├── CifraShop.Client/                  Blazor WebAssembly — SPA-клиент
│   ├── Pages/
│   │   ├── Admin.razor                Админ-панель (оркестратор)
│   │   └── Components/                5 дочерних компонентов:
│   │       ├── AdminProductsPanel     CRUD товаров + изображения
│   │       ├── AdminOrdersPanel       CRUD заказов + статусы
│   │       ├── AdminUsersPanel        CRUD пользователей + баланс
│   │       ├── AdminNotificationsPanel Настройки уведомлений филиалов
│   │       └── AdminHistoryPanel      Аудит действий
│   ├── Services/SignalRService.cs     SignalR-клиент с автопереподключением
│   ├── Models/ApiModels.cs            Клиентские DTO
│   └── wwwroot/                       Bootstrap 5.3.8 + Icons (локально)
│
├── CifraShop.Launcher/                Консольный лаунчер — автоматизация запуска
│   └── Program.cs                     6 фаз + меню + мониторинг
│
├── CifraShop.Tests/                   MSTest (заготовка)
└── README.md
```

### Поток данных

```
┌──────────────┐  HTTP/WS   ┌──────────────┐
│   Клиент     │ <─────────> │   API        │
│   (Blazor)   │  SignalR    │  Контроллеры │
└──────────────┘             └──────┬───────┘
                                    │
                             ┌──────▼───────┐
                             │ Application  │
                             │  Сервисы     │
                             └──────┬───────┘
                                    │
                      ┌─────────────┼─────────────┐
               ┌──────▼──────┐           ┌───────▼──────┐
               │   Domain    │           │Infrastructure│
               │  Сущности   │           │   EF Core    │
               └─────────────┘           └──────┬───────┘
                                                │
                                         ┌──────▼───────┐
                                         │  SQL Server   │
                                         └──────────────┘
```

---

## API Эндпоинты

### Товары

| Метод | Роут | Описание |
|-------|------|----------|
| `GET` | `api/Product/all` | Все товары |
| `GET` | `api/Product/paged?page=0&pageSize=8` | С пагинацией, поиском и фильтром |
| `POST` | `api/Product/create-product` | Создать |
| `PUT` | `api/Product/update-product?id=X` | Обновить |
| `DELETE` | `api/Product/delete-product?id=X` | Удалить |
| `POST` | `api/Product/batch-delete` | Массовое удаление |
| `POST` | `api/Product/batch-update-status` | Массовая смена статуса |

### Заказы

| Метод | Роут | Описание |
|-------|------|----------|
| `GET` | `api/Order/paged?page=0&pageSize=8` | С пагинацией и фильтрами |
| `POST` | `api/Order/create-order` | Создать (CustomerEmail, Items) |
| `PUT` | `api/Order/update-order?id=X` | Обновить статус |
| `POST` | `api/Order/batch-update-status` | Массовая смена статуса |

### Пользователи

| Метод | Роут | Описание |
|-------|------|----------|
| `GET` | `api/User/paged?page=0&pageSize=8` | С пагинацией |
| `POST` | `api/User/create-admin` | Создать админа |
| `POST` | `api/User/create-student` | Создать студента |
| `PUT` | `api/User?id=X` | Обновить |
| `DELETE` | `api/User?id=X` | Удалить |

### Уведомления, изображения, история

Полная документация API — в исходных контроллерах.

---

## Админ-панель

### Статус подключения

| Бейдж | Значение |
|-------|----------|
| <span style="color:green">● Online</span> | SignalR подключён — все изменения отображаются мгновенно |
| <span style="color:red">● Offline</span> | SignalR отключён — fallback-опрос каждые 5 минут. Перезагрузите страницу для подключения |

### Разделы

**Товары:**
- Поиск с подсветкой, фильтр по статусу, серверная пагинация
- Инлайн-редактирование количества, переключение статуса (клик по бейджу)
- Загрузка фотографий
- Batch-операции: смена статуса, удаление (выбор чекбоксами)
- Предупреждение о низком остатке (подсветка строк)

**Заказы:**
- Автокомплит email, фильтр по дате
- Развёрнутые строки с деталями и изображениями товаров
- Выпадающий список статусов, batch-смена

**Пользователи:**
- Фильтр по роли (Студенты/Админы)
- Редактирование баланса через модалку

**Уведомления:**
- По филиалам: email, Telegram, порог остатков
- Привязка админов к филиалам

**История:**
- Сворачиваемая шкала времени
- Фильтр по типу действия и филиалу

### URL-фильтры

Все поисковые запросы и фильтры сохраняются в URL query-параметрах (`ps`, `pf`, `os`, `of`, `us`, `uf`, `odf`, `odt`). Можно поделиться ссылкой с результатами поиска.

---

## Архитектура

### Чистая архитектура

```
┌─────────────────────────────────────────────────────┐
│  Представление                                       │
│  ├── Контроллеры API + SignalR Hub                   │
│  └── Blazor WASM клиент (5 компонентов)             │
├─────────────────────────────────────────────────────┤
│  Приложение                                          │
│  ├── Интерфейсы сервисов                             │
│  └── Реализации сервисов                             │
├─────────────────────────────────────────────────────┤
│  Контракты                                           │
│  ├── DTO запросов/ответов                            │
│  └── Маппинг (extension-методы)                     │
├─────────────────────────────────────────────────────┤
│  Домен (без зависимостей)                            │
│  ├── Сущности + перечисления                         │
│  └── Интерфейсы репозиториев                         │
├─────────────────────────────────────────────────────┤
│  Инфраструктура                                      │
│  ├── EF Core + Fluent API                            │
│  └── Реализации репозиториев                         │
└─────────────────────────────────────────────────────┘
```

### Интеграция SignalR

| Компонент | Роль |
|-----------|------|
| `AdminHub` | Пустой хаб — сервер шлёт, клиенты слушают |
| Контроллеры | `IHubContext<AdminHub>` → `SendAsync("Notify", entity, action)` после мутаций |
| `SignalRService` | Клиентское подключение с автопереподключением (0с, 2с, 5с, 10с) |
| `Admin.razor` | `HandleSignalRNotify` → перезагружает нужный раздел данных |

События: `product` (created/updated/deleted), `order` (created/updated), `user` (created/updated/deleted), `notification` (updated).

### Обработка ошибок

| Исключение | HTTP-код |
|------------|----------|
| `ArgumentException` | 400 Bad Request |
| `KeyNotFoundException` | 404 Not Found |
| `InvalidOperationException` | 409 Conflict |
| Остальные | 500 Internal Server Error |

---

## Лаунчер

### 6 фаз запуска

| Фаза | Описание |
|------|----------|
| 1 | Проверка .NET SDK и Docker (авто-запуск Docker Desktop) |
| 2 | Создание `docker-compose.yml` и строки подключения |
| 3 | Запуск SQL Server в Docker |
| 4 | Применение EF Core миграций |
| 5 | Запуск API |
| 6 | Запуск Blazor-клиента |

### Интерактивное меню

```
╔══════════════════════════════════════════════════╗
║  МЕНЮ УПРАВЛЕНИЯ                                 ║
║  [1] Открыть админ-панель                         ║
║  [2] Открыть главную страницу                     ║
║  ──────────────────────────────────────────────── ║
║  [3] Перезапустить API                            ║
║  [4] Перезапустить клиент                         ║
║  [5] Перезапустить всё                            ║
║  ──────────────────────────────────────────────── ║
║  [6] Остановить всё                               ║
║  ──────────────────────────────────────────────── ║
║  [7] Пересобрать API (Docker)                     ║
║  [8] Очистить Docker-образы                       ║
║  ──────────────────────────────────────────────── ║
║  [9] Показать логи                                ║
║  [0] Выход                                        ║
╚══════════════════════════════════════════════════╝
```

### CLI-флаги

| Флаг | Описание |
|------|----------|
| `--quick` | Быстрый режим — пропуск выполненных фаз |
| `--skip-docker` | Принудительно локальный режим |
| `--port-api N` | Переопределение порта API |
| `--port-client N` | Переопределение порта клиента |
| `--reset` | Полный сброс: остановка контейнеров, очистка образов |

### Мониторинг

- Фоновая проверка процессов каждые 5 секунд
- Авто-рестарт при падении (до 3 раз)
- Буфер логов с маркерами перезапуска

---

## Порты

| Сервис | Протокол | Порт | URL |
|--------|----------|------|-----|
| API | HTTP | 5000 | `http://localhost:5000` |
| Клиент | HTTP | 5001 | `http://localhost:5001` |
| SQL Server | TCP | 1433 | `localhost:1433` |

---

## Дорожная карта

### Сделано

- [x] CRUD для товаров, заказов, пользователей
- [x] Админ-панель с поиском, фильтрами, серверной пагинацией
- [x] Batch-операции (смена статуса, удаление)
- [x] **SignalR — обновления в реальном времени**
- [x] Загрузка и управление фотографиями
- [x] История действий с фильтрацией
- [x] URL-фильтры
- [x] Консольный лаунчер с авто-рестартом
- [x] Интеграция с Docker
- [x] ExceptionHandlerMiddleware
- [x] Исправлено 30+ багов

### Запланировано

- [ ] Аутентификация (JWT)
- [ ] Хеширование паролей
- [ ] Валидация DTO (FluentValidation)
- [ ] Unit-тесты (MSTest)
- [ ] Страница каталога для студентов
- [ ] Корзина и оформление заказа
- [ ] Реальные уведомления (email/Telegram)
- [ ] Серверная сортировка

---

<p align="center">
  Проект разработан в рамках курса <b>Cifra</b>
</p>
