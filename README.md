# CifraShop

Интернет-магазин, построенный по архитектуре **Clean Architecture** на стеке **.NET 10**.

| Компонент | Решение |
|-----------|---------|
| Бэкенд | ASP.NET Core Web API (minimal hosting) |
| Фронтенд | Blazor WebAssembly |
| База данных | SQL Server |
| ORM | Entity Framework Core 10 |
| UI | Bootstrap 5.3.8 + Bootstrap Icons |
| Контейнеризация | Docker (multi-stage build) |

---

## Содержание

- [Как запустить](#как-запустить)
- [Структура решения](#структура-решения)
- [Проекты](#проекты)
- [API эндпоинты](#api-эндпоинты)
- [Админ-панель](#админ-панель)
- [Компоненты клиента](#компоненты-клиента)
- [Архитектура](#архитектура)
- [Известные проблемы](#известные-проблемы)
- [Дорожная карта](#дорожная-карта)
- [Порты](#порты)

---

## Как запустить

### Что нужно

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server (локальный или Docker)
- Строка подключения в User Secrets или `appsettings.json`

### Пошагово

```bash
# 1. Создать и применить миграцию
dotnet ef migrations add InitialCreate --project CifraShop.Infrastructure --startup-project CifraShop.API
dotnet ef database update --project CifraShop.Infrastructure --startup-project CifraShop.API

# 2. Запустить API (порт 5000)
dotnet run --project CifraShop.API

# 3. Запустить клиент (порт 5001)
dotnet run --project CifraShop.Client
```

### Что откроется

| Страница | Адрес |
|----------|-------|
| Магазин | `http://localhost:5001` |
| Админ-панель | `http://localhost:5001/admin` |
| API (OpenAPI) | `https://localhost:5000/openapi/v1.json` |

### Docker

```bash
docker build -t cifrashop -f CifraShop.API/Dockerfile .
docker run -p 8085:8085 -p 8086:8086 cifrashop
```

---

## Структура решения

```
CifraShop/
|-- CifraShop.API/               Точка входа, контроллеры, DI
|-- CifraShop.Client/            Blazor WASM SPA (фронтенд)
|-- CifraShop.Application/       Бизнес-логика (сервисы)
|-- CifraShop.Domain/            Ядро (сущности, enums)
|-- CifraShop.Infrastructure/    EF Core, репозитории
|-- CifraShop.Contracts/         DTO (Request/Response), маппинг
|-- CifraShop.Tests/             Тесты (заготовка)
|-- CifraShop.slnx               Файл решения (.NET 10 XML-формат)
```

**Поток зависимостей:**

```
API ──> Application ──> Domain
Infrastructure ──> Domain
Client ──(HTTP)──> API
```

---

## Проекты

### Domain — Ядро

Сущности и перечисления. Зависит только от .NET BCL.

```
Domain/Entities/       User, Product, Order, OrderItem, AdminAction
Domain/Enums/          UserRole, StatusProduct, StatusOrder
```

| Сущность | Ключевые поля |
|----------|---------------|
| `User` | Id, Email, Password, Balance (short?), Role |
| `Product` | Id, Name, Description, Price (short), Quantity (short), Status, ThePathToTheImage |
| `Order` | Id, Status, Sum (short), DateOfPurchase, CustomerLogin, CustomerId |
| `OrderItem` | Id, OrderId, ProductId, Quantity, Price |
| `AdminAction` | Id, ActionType, Details, CreatedAt |

| Enum | Значения |
|------|----------|
| `UserRole` | Student, Admin |
| `StatusProduct` | InStock, OutOfStock, OnSaleSoon |
| `StatusOrder` | Pending, AwaitingPayment, PaidFor, ManufacturedBy, Completed |

> `Product` реализует `INotifyPropertyChanged` — намеренно для привязки данных в Blazor.

---

### Infrastructure — Инфраструктура

EF Core контекст (`ApplicationContext`), Fluent API конфигурации, реализации репозиториев.

```
Infrastructure/Data/
|-- ApplicationContext.cs                    DbContext
|-- Configurations/                          Fluent API (пока не применяются)
|-- Repositories/Interfaces/                 IUserRepository, IProductRepository, ...
|-- Repositories/Implementations/            UserRepositoryEfCore, ...
```

**Связи таблиц:**

```
User 1 ────* Order (CustomerId, Restrict)
Order 1 ────* OrderItem (CASCADE)
Product 1 ────* OrderItem (RESTRICT)
```

---

### Application — Бизнес-логика

Интерфейсы и реализации сервисов.

```
Application/Services/
|-- Interfaces/      IUserService, IProductService, IOrderService, IOrderItemService, IAdminActionService
|-- Implementations/ UserService, ProductService, OrderService, OrderItemService, AdminActionService
```

Каждый сервис принимает интерфейс репозитория через DI и делегирует ему CRUD-операции.

---

### Contracts — Контракты

DTO для обмена данными между API и клиентом.

```
Contracts/
|-- Requests/        CreateProductRequest, UppdateProductRequest, CreateOrderRequest, ...
|-- Responses/       ProductResponce, OrderResponse, UserResponse, AdminActionResponse, ...
|-- Mappings/        ProductMapper, OrderMapping, OrderItemMapping, UserMapping
```

Маппинг — extension-методы: `product.ToResponse()` конвертирует сущность в DTO.

---

### API — Веб-API

Точка входа. Минимальный хостинг без `Startup.cs`.

```
API/
|-- Program.cs                       DI, CORS, middleware
|-- Controllers/                     Product, Order, OrderItem, User, AdminAction
|-- appsettings.json
|-- Dockerfile
```

**DI регистрирует:** 5 репозиториев + 5 сервисов.

---

### Client — Blazor WebAssembly

Одностраничное приложение.

```
Client/
|-- Program.cs                       HttpClient (BaseAddress: localhost:5000)
|-- Models/ApiModels.cs              DTO: ProductDto, OrderDto, OrderItemDto, UserDto, AdminActionDto
|-- Pages/
|   |-- Home.razor                   Главная
|   |-- Admin.razor                  Админ-панель
|   |-- NotFound.razor               404
|-- Components/                      Переиспользуемые компоненты
|   |-- MetricCard.razor             Карточка метрики
|   |-- SearchBar.razor              Поиск + фильтр
|   |-- Pagination.razor             Пагинация
|   |-- Spinner.razor                Индикатор загрузки
|   |-- EmptyState.razor             «Ничего не найдено»
|-- wwwroot/index.html               Точка монтирования Blazor
```

**Правила клиента:**
- Никакого JavaScript (`IJSRuntime` запрещён)
- Только Bootstrap CSS (никаких кастомных стилей)
- Все размеры в `rem` (никаких `px` в inline-стилях)

---

## API эндпоинты

### Товары

| Метод | Маршрут | Описание |
|-------|---------|----------|
| `GET` | `api/Product/all` | Все товары |
| `GET` | `api/Product/by-id?id=X` | Товар по ID |
| `GET` | `api/Product/by-name?name=X` | По имени |
| `GET` | `api/Product/by-price?price=X` | По цене |
| `GET` | `api/Product/by-quantity?quantity=X` | По количеству |
| `GET` | `api/Product/by-status?statusProduct=X` | По статусу |
| `POST` | `api/Product/create-product` | Создать товар |
| `PUT` | `api/Product/update-product?id=X` | Обновить товар |
| `DELETE` | `api/Product/delete-product?id=X` | Удалить товар |

### Заказы

| Метод | Маршрут | Описание |
|-------|---------|----------|
| `GET` | `api/Order/all` | Все заказы (с позициями) |
| `GET` | `api/Order/by-id?id=X` | Заказ по ID |
| `GET` | `api/Order/by-login?login=X` | По логину клиента |
| `GET` | `api/Order/by-status?status=X` | По статусу |
| `GET` | `api/Order/by-sum?sum=X` | По сумме |
| `POST` | `api/Order/create-order` | Создать заказ |
| `PUT` | `api/Order/update-order?id=X` | Обновить статус/сумму |
| `DELETE` | `api/Order/delete-order?id=X` | Удалить заказ + позиции |

### Позиции заказов

| Метод | Маршрут | Описание |
|-------|---------|----------|
| `GET` | `api/OrderItem/by-id?id=X` | Позиция по ID |
| `GET` | `api/OrderItem/by-orderid?id=X` | Позиции по ID заказа |
| `POST` | `api/OrderItem/create-orderitem` | Создать позицию |
| `PUT` | `api/OrderItem/update-orderitem?id=X` | Обновить позицию |
| `DELETE` | `api/OrderItem/delete-orderItem?id=X` | Удалить позицию |

### Пользователи

| Метод | Маршрут | Описание |
|-------|---------|----------|
| `GET` | `api/User/all` | Все пользователи |
| `GET` | `api/User/all-admins` | Только админы |
| `GET` | `api/User/all-students` | Только студенты |
| `GET` | `api/User/by-id?id=X` | По ID |
| `GET` | `api/User/by-email?email=X` | По email |
| `POST` | `api/User/create-admin` | Создать админа |
| `POST` | `api/User/create-student` | Создать студента |
| `PUT` | `api/User?id=X` | Обновить пользователя |
| `DELETE` | `api/User?id=X` | Удалить пользователя |

### История действий

| Метод | Маршрут | Описание |
|-------|---------|----------|
| `GET` | `api/AdminAction/last?count=50` | Последние N действий |
| `POST` | `api/AdminAction` | Записать действие |

---

## Админ-панель

Страница `/admin` — полноценная панель управления магазином.

### Метрики

Три карточки вверху: количество товаров, заказов и пользователей. Обновляются автоматически каждые 30 секунд.

### Товары

| Возможность | Описание |
|-------------|----------|
| Поиск | По названию и описанию, мгновенно при вводе |
| Фильтр | По статусу: все / В наличии / Нет / Скоро |
| Сортировка | По любому столбцу (клик по заголовку), ▲/▼ |
| Пагинация | 8 элементов на страницу |
| Создание | Модальное окно: название, описание, цена, количество |
| Редактирование | Модальное окно с заполненными данными |
| Количество | Изменение прямо в таблице (input) |
| Статус | Клик по бейджу переключает «В наличии» / «Нет» |
| Массовые действия | Выбрать несколько → Показать / Скрыть / Удалить |
| Удаление | С подтверждением через модальное окно |

### Заказы

| Возможность | Описание |
|-------------|----------|
| Поиск | По номеру заказа и email покупателя |
| Фильтр | По статусу (5 вариантов) |
| Сортировка | По номеру, покупателю, сумме, статусу |
| Статус | Изменение через выпадающий список в таблице |
| Детали | Клик по строке раскрывает состав заказа (товары, количество, суммы) |
| Массовые действия | Чекбоксы + смена статуса для выбранных |
| Дата | Колонка с датой покупки |

### Пользователи

| Возможность | Описание |
|-------------|----------|
| Поиск | По email |
| Фильтр | По роли: все / Студенты / Админы |
| Сортировка | По email, балансу, роли |
| Удаление | С подтверждением через модальное окно |

### История действий

Сворачиваемая панель внизу страницы. Показывает все операции (создание, изменение, удаление) с таймстампами и цветовыми бейджами. Данные хранятся на бэкенде — видны всем администраторам.

### Уведомления

Всплывающие сообщения об успешных операциях и ошибках. Автоскрытие через 4 секунды. Детальные сообщения об ошибках от API.

---

## Компоненты клиента

| Компонент | Назначение |
|-----------|-----------|
| `MetricCard` | Карточка метрики (название, число, иконка, цвет) |
| `SearchBar` | Поле поиска + выпадающий фильтр |
| `Pagination` | Навигация по страницам ( prev / номера / next ) |
| `Spinner` | Индикатор загрузки |
| `EmptyState` | Заглушка «Ничего не найдено» |

---

## Архитектура

### Принципы

```
┌─────────────────────────────────────────┐
│         Presentation (API, Client)      │
├─────────────────────────────────────────┤
│         Application (Services)          │
├─────────────────────────────────────────┤
│         Domain (Entities, Enums)        │
├─────────────────────────────────────────┤
│      Infrastructure (EF Core, Repos)    │
└─────────────────────────────────────────┘
```

Каждый слой зависит только от нижележащих. Domain — самый низкий (ничего не зависит). Presentation — самый верхний.

### Repository pattern

Каждая сущность: интерфейс в `Infrastructure/Interfaces/`, реализация в `Implementations/`. Репозиторий инкапсулирует работу с одной таблицей.

### Dependency Injection

Все репозитории и сервисы регистрируются как `Scoped` в `API/Program.cs`.

### DTO-маппинг

Extension-методы в `Contracts/Mappings/`: `product.ToResponse()` конвертирует доменную сущность в DTO.

---

## Известные проблемы

### Критические

| Проблема | Файл | Влияние |
|----------|------|---------|
| Нет `AddDbContext` в DI | `API/Program.cs` | API не работает с БД |
| `OrderItemService` не сохраняет Quantity | `OrderItemService.cs` | Позиции заказов = 0 шт |
| `OrderController` путает количество | `OrderController.cs` | Заказывает весь склад |
| `UserResponse` отдаёт пароль | `UserResponse.cs` | Утечка данных |

### Архитектурные

| Проблема | Влияние |
|----------|---------|
| Application зависит от Infrastructure | Нарушение Clean Architecture |
| Контроллеры inject репозитории напрямую | Обход слоя сервисов |
| Fluent API конфигурации не применяются | Мёртвый код |
| Пароли в открытом виде | Нет безопасности |
| Нет аутентификации | Любой может управлять магазином |

### Мелочи

| Проблема | Где |
|----------|-----|
| Опечатка `AddPoduct` (пропущена 'r') | IProductRepository, реализации |
| Опечатка `UppdateProductRequest` (двойная 'p') | Contracts/Requests/Products/ |
| Опечатка `ProductResponce` / `UpdateOrderItemResponce` | Contracts/Responses/ |
| Файл ≠ имя класса (`ApplicationDbContext.cs` vs `ApplicationContext`) | Infrastructure/Data/ |
| Domain csproj содержит лишние NuGet-пакеты | Domain.csproj |
| `Class1.cs` placeholder | Application/ |

---

## Дорожная карта

### Приоритет 1 — Критические исправления

Без этих пунктов приложение не работает корректно.

**1. Регистрация ApplicationContext в DI**

В `Program.cs` нет `AddDbContext<ApplicationContext>(...)`. Без него ни один репозиторий не создаётся.

```csharp
builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

**2. Исправить OrderItemService.CreateOrderItem**

Параметр `quantity` не присваивается позиции заказа.

```csharp
Quantity = quantity  // добавить эту строку
```

**3. Исправить OrderController.Create**

Использует `product.Quantity` (остаток) вместо `itemReq.Quantity` (заказано).

```csharp
Quantity = itemReq.Quantity  // заменить product.Quantity
```

**4. Убрать пароль из UserResponse**

Удалить поле `Password` из `UserResponse.cs`.

---

### Приоритет 2 — Безопасность

**5. Хеширование паролей**

ASP.NET Identity с bcrypt. Пароли не должны храниться открыто.

```csharp
await _userManager.CreateAsync(user, password); // хешируется автоматически
```

**6. JWT аутентификация + авторизация**

Защитить API-эндпоинты. Разделить роли: Student (только покупки), Admin (управление).

```csharp
[Authorize(Roles = "Admin")]
[HttpPost("create-product")]
public async Task<IActionResult> CreateProduct(...) { ... }
```

---

### Приоритет 3 — Архитектура

**7. Исправить Clean Architecture**

Перенести интерфейсы репозиториев из Infrastructure в Domain. Application не должен ссылаться на Infrastructure.

**8. Убрать репозитории из контроллеров**

`OrderController` и `OrderItemController` инжектят репозитории напрямую — обходят слой сервисов.

**9. Валидация запросов**

FluentValidation или DataAnnotations. Сейчас можно создать товар с пустым именем.

**10. Глобальная обработка ошибок**

Middleware для перехвата исключений. Сейчас stack traces утекают клиенту.

**11. Логирование**

`ILogger<T>` в сервисах. Без логов невозможно отлаживать в продакшене.

---

### Приоритет 4 — Улучшения клиента

**12. Реальное время (SignalR)**

Заменить 30-секундный опрос на push-уведомления. Требует JS на клиенте.

**13. Расширить типы Price/Quantity**

Заменить `short` (макс. 32 767) на `int` (макс. 2 млрд) везде в Domain и DTO.

**14. Тесты**

单元 тесты сервисов (с моками), интеграционные тесты контроллеров (InMemory DB), тесты валидации.

**15. Исправить опечатки**

`AddPoduct` → `AddProduct`, `UppdateProductRequest` → `UpdateProductRequest`, `ProductResponce` → `ProductResponse`.

**16. Очистить Domain csproj**

Убрать JWT, Components, DevServer пакеты — Domain должен быть чистым.

**17. Применить или удалить Fluent API конфигурации**

Сейчас конфиги в `Configurations/` не используются (закомментированы). Либо применить, либо удалить.

---

## Порты

| Сервис | Протокол | Порт | URL |
|--------|----------|------|-----|
| API | HTTPS | 5000 | `https://localhost:5000` |
| Client | HTTP | 5001 | `http://localhost:5001` |
| Docker (HTTPS) | HTTPS | 8085 | `https://localhost:8085` |
| Docker (HTTP) | HTTP | 8086 | `http://localhost:8086` |
