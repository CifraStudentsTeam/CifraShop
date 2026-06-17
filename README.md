# CifraShop

Интернет-магазин, построенный по архитектуре **Clean Architecture** на стеке **.NET 10**. Бэкенд — ASP.NET Core Web API, фронтенд — Blazor WebAssembly, база данных — SQL Server через Entity Framework Core.

---

## Содержание

- [Технологии](#технологии)
- [Структура решения](#структура-решения)
- [Как запустить](#как-запустить)
- [Структура проектов](#структура-проектов)
- [API эндпоинты](#api-эндпоинты)
- [Админ-панель](#админ-панель)
- [Архитектура](#архитектура)
- [Известные проблемы](#известные-проблемы)
- [Дорожная карта](#дорожная-карта)

---

## Технологии

| Компонент | Технология |
|-----------|-----------|
| **Рантайм** | .NET 10 |
| **Бэкенд** | ASP.NET Core Web API (minimal hosting) |
| **Фронтенд** | Blazor WebAssembly |
| **База данных** | SQL Server |
| **ORM** | Entity Framework Core 10 |
| **UI-фреймворк** | Bootstrap 5.3.8 |
| **Иконки** | Bootstrap Icons |
| **Контейнеризация** | Docker (multi-stage build) |

---

## Структура решения

```
CifraShop/
|
|-- CifraShop.API/               # Точка входа — ASP.NET Core Web API
|-- CifraShop.Client/            # Blazor WebAssembly SPA (фронтенд)
|-- CifraShop.Application/       # Бизнес-логика — интерфейсы и реализации сервисов
|-- CifraShop.Domain/            # Ядро — сущности, enums, бизнес-правила
|-- CifraShop.Infrastructure/    # Инфраструктура — EF Core, репозитории, БД
|-- CifraShop.Contracts/         # Контракты — DTO (Request/Response), маппинг
|-- CifraShop.Tests/             # Тесты (заготовка)
|-- CifraShop.slnx               # Файл решения (XML-формат .NET 10)
```

**Поток зависимостей:**

```
API --> Application --> Domain
Infrastructure --> Domain
Client --> (HTTP) --> API
```

---

## Как запустить

### Предварительные требования

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server (локальный или Docker)
- Настроенная строка подключения (User Secrets или appsettings.json)

### 1. Настройка базы данных

```bash
# Добавить миграцию (из корня решения)
dotnet ef migrations add InitialCreate --project CifraShop.Infrastructure --startup-project CifraShop.API

# Применить миграцию
dotnet ef database update --project CifraShop.Infrastructure --startup-project CifraShop.API
```

### 2. Запуск API (порт 5000)

```bash
dotnet run --project CifraShop.API
```

### 3. Запуск клиента (порт 5001)

```bash
dotnet run --project CifraShop.Client
```

### 4. Открыть в браузере

- Клиент: `http://localhost:5001`
- Админ-панель: `http://localhost:5001/admin`
- API (Swagger/OpenAPI): `https://localhost:5000/openapi/v1.json`

### Через Docker

```bash
docker build -t cifrashop -f CifraShop.API/Dockerfile .
docker run -p 8085:8085 -p 8086:8086 cifrashop
```

---

## Структура проектов

### CifraShop.Domain — Ядро

Содержит сущности и перечисления. Зависит только от .NET BCL, никаких внешних пакетов.

```
Domain/
|-- Entities/
|   |-- User.cs              # Пользователь (Id, Email, Password, Balance, Role)
|   |-- Product.cs           # Товар (Id, Name, Description, Price, Quantity, Status, Image)
|   |-- Order.cs             # Заказ (Id, Status, Sum, DateOfPurchase, CustomerId)
|   |-- OrderItem.cs         # Позиция заказа (OrderId, ProductId, Quantity, Price)
|-- Enums/
|   |-- UserRole.cs          # Student, Admin
|   |-- StatusProduct.cs     # InStock, OutOfStock, OnSaleSoon
|   |-- StatusOrder.cs       # Pending, AwaitingPayment, PaidFor, ManufacturedBy, Completed
```

**Особенности:**
- `Product` реализует `INotifyPropertyChanged` — для привязки данных в Blazor
- Типы `short` для Price/Quantity (максимум 32 767)
- Пароли хранятся открыто (без хеширования) — **нужно исправить**

### CifraShop.Infrastructure — Инфраструктура

EF Core контекст, конфигурации Fluent API, реализации репозиториев.

```
Infrastructure/
|-- Data/
|   |-- ApplicationDbContext.cs    # ApplicationContext — DbContext
|   |-- Configurations/
|   |   |-- UserConfiguration.cs
|   |   |-- ProductConfiguration.cs
|   |   |-- OrderConfiguration.cs
|   |   |-- OrderItemConfiguration.cs
|   |-- Repositories/
|       |-- Interfaces/
|       |   |-- IUserRepository.cs
|       |   |-- IProductRepository.cs
|       |   |-- IOrderRepository.cs
|       |   |-- IOrderItemRepository.cs
|       |-- Implementations/
|           |-- UserRepositoryEfCore.cs
|           |-- ProductRepositoryEfCore.cs
|           |-- OrderRepositoryEfCore.cs
|           |-- OrderItemRepositoryEfCore.cs
```

**Паттерн:** Repository — каждый репозиторий инкапсулирует работу с одной сущностью.

**Связи таблиц:**
- `User` 1 ---> * `Order` (через `CustomerId`)
- `Order` 1 ---> * `OrderItem` (каскадное удаление)
- `Product` 1 ---> * `OrderItem` (RESTRICT при удалении)

### CifraShop.Application — Бизнес-логика

Интерфейсы и реализации сервисов. Сервисы — посредники между контроллерами и репозиториями.

```
Application/
|-- Services/
    |-- Interfaces/
    |   |-- IUserService.cs
    |   |-- IProductService.cs
    |   |-- IOrderService.cs
    |   |-- IOrderItemService.cs
    |-- Implementations/
        |-- UserService.cs
        |-- ProductService.cs
        |-- OrderService.cs
        |-- OrderItemService.cs
```

**Каждый сервис:**
- Принимает интерфейс репозитория через конструктор (DI)
- Delegates CRUD-операции в репозиторий
- Добавляет бизнес-логику (например, `CreateProductData` устанавливает статус `OnSaleSoon`)

### CifraShop.Contracts — Контракты

DTO (Data Transfer Objects) для обмена данными между API и клиентом.

```
Contracts/
|-- Requests/
|   |-- Products/   CreateProductRequest, UppdateProductRequest
|   |-- Orders/     CreateOrderRequest, UpdateOrderRequest
|   |-- OrderItem/  CreateOrderItemRequest, OrderItemRequest
|   |-- Users/      CreateUserRequest, UpdateUserRequest
|-- Responses/
|   |-- Products/   ProductResponce
|   |-- Orders/     OrderResponse
|   |-- OrderItem/  OrderResponse (OrderItem), UpdateOrderItemResponce
|   |-- User/       UserResponse
|-- Mappings/
    |-- ProductMapper.cs      # Product -> ProductResponce
    |-- OrderMapping.cs       # Order -> OrderResponse
    |-- OrderItemMapping.cs   # OrderItem -> OrderItemResponse
    |-- UserMapping.cs        # User -> UserResponse
```

### CifraShop.API — Веб-API

Точка входа приложения. Минимальный хостинг без `Startup.cs`.

```
API/
|-- Program.cs               # DI, CORS, middleware, маршруты
|-- Controllers/
|   |-- ProductController.cs  # CRUD товаров
|   |-- OrderController.cs    # CRUD заказов
|   |-- OrderItemController.cs # CRUD позиций заказов
|   |-- UserController.cs     # CRUD пользователей
|-- appsettings.json
|-- Dockerfile
```

**CORS:** разрешены `https://localhost:5001` и `http://localhost:5001`.

### CifraShop.Client — Blazor WebAssembly

Одностраничное приложение на Blazor WASM.

```
Client/
|-- Program.cs               # Регистрация HttpClient (BaseAddress: localhost:5000)
|-- App.razor                # Router
|-- Models/
|   |-- ApiModels.cs         # DTO: ProductDto, OrderDto, OrderItemDto, UserDto
|-- Pages/
|   |-- Home.razor           # Главная страница
|   |-- Admin.razor          # Админ-панель (770+ строк)
|   |-- NotFound.razor       # Страница 404
|-- wwwroot/
    |-- index.html           # Точка монтирования Blazor
    |-- bootstrap/           # Bootstrap 5.3.8 (CSS, JS, SCSS)
```

---

## API эндпоинты

### Товары

| Метод | Маршрут | Описание |
|-------|---------|----------|
| `GET` | `api/Product/all` | Все товары |
| `GET` | `api/Product/by-id?id=X` | Товар по ID |
| `GET` | `api/Product/by-name?name=X` | Поиск по имени |
| `GET` | `api/Product/by-price?price=X` | Фильтр по цене |
| `GET` | `api/Product/by-quantity?quantity=X` | Фильтр по количеству |
| `GET` | `api/Product/by-status?statusProduct=X` | Фильтр по статусу |
| `POST` | `api/Product/create-product` | Создание товара |
| `PUT` | `api/Product/update-product?id=X` | Обновление товара |
| `DELETE` | `api/Product/delete-product?id=X` | Удаление товара |

### Заказы

| Метод | Маршрут | Описание |
|-------|---------|----------|
| `GET` | `api/Order/all` | Все заказы (с позициями) |
| `GET` | `api/Order/by-id?id=X` | Заказ по ID |
| `GET` | `api/Order/by-login?login=X` | Заказы по логину клиента |
| `GET` | `api/Order/by-status?status=X` | Фильтр по статусу |
| `GET` | `api/Order/by-sum?sum=X` | Фильтр по сумме |
| `POST` | `api/Order/create-order` | Создание заказа |
| `PUT` | `api/Order/update-order?id=X` | Обновление статуса/суммы |
| `DELETE` | `api/Order/delete-order?id=X` | Удаление заказа + позиций |

### Пользователи

| Метод | Маршрут | Описание |
|-------|---------|----------|
| `GET` | `api/User/all` | Все пользователи |
| `GET` | `api/User/all-admins` | Только админы |
| `GET` | `api/User/all-students` | Только студенты |
| `GET` | `api/User/by-id?id=X` | Пользователь по ID |
| `GET` | `api/User/by-email?email=X` | Пользователь по email |
| `POST` | `api/User/create-admin` | Создание админа |
| `POST` | `api/User/create-student` | Создание студента |
| `PUT` | `api/User?id=X` | Обновление пользователя |
| `DELETE` | `api/User?id=X` | Удаление пользователя |

### Позиции заказов

| Метод | Маршрут | Описание |
|-------|---------|----------|
| `GET` | `api/OrderItem/by-id?id=X` | Позиция по ID |
| `GET` | `api/OrderItem/by-orderid?id=X` | Позиции по ID заказа |
| `POST` | `api/OrderItem/create-orderitem` | Создание позиции |
| `PUT` | `api/OrderItem/update-orderitem?id=X` | Обновление позиции |
| `DELETE` | `api/OrderItem/delete-orderItem?id=X` | Удаление позиции |

---

## Админ-панель

Страница `/admin` — полноценная панель управления магазином.

### Возможности

**Метрики:**
- Общее количество товаров, заказов и пользователей
- Автоматическое обновление каждые 30 секунд

**Управление товарами:**
- Таблица с поиском по названию/описанию и фильтрацией по статусу
- Сортировка по любому столбцу (клик по заголовку)
- Пагинация (8 элементов на страницу)
- Создание нового товара через модальное окно
- Редактирование товаров (название, описание, цена, количество)
- Изменение количества на складе прямо в таблице
- Массовые действия: показать, скрыть, удалить выбранные
- Чекбокс "выбрать все" на текущей странице
- Подтверждение удаления через модальное окно

**Управление заказами:**
- Таблица с поиском по номеру/email покупателя
- Фильтрация по статусу (ожидание, ожидает оплаты, оплачен, в производстве, выполнен)
- Сортировка по номеру, покупателю, сумме, статусу
- Быстрое изменение статуса через выпадающий список

**Управление пользователями:**
- Таблица с поиском по email
- Фильтрация по роли (студенты, админы)
- Сортировка по email, балансу, роли
- Удаление пользователей с подтверждением

**Уведомления:**
- Всплывающие сообщения об успешных операциях и ошибках
- Автоскрытие через 4 секунды
- Цветовая индикация (зеленый — успех, красный — ошибка)

---

## Архитектура

### Принципы

Проект следует паттерну **Clean Architecture** с разделением на слои:

```
┌─────────────────────────────────────────┐
│            Presentation Layer           │
│         (API Controllers, Client)       │
├─────────────────────────────────────────┤
│          Application Layer              │
│      (Services, Interfaces, DTOs)       │
├─────────────────────────────────────────┤
│           Domain Layer                  │
│     (Entities, Enums, Business Rules)   │
├─────────────────────────────────────────┤
│        Infrastructure Layer             │
│    (EF Core, Repositories, External)    │
└─────────────────────────────────────────┘
```

**Правило зависимостей:** каждый слой может зависеть только от нижележащих слоёв. Domain — самый низкий, Presentation — самый верхний.

### Паттерн Repository

Каждая сущность имеет интерфейс репозитория и EF Core реализацию:

```csharp
// Интерфейс (Infrastructure/Interfaces/)
public interface IProductRepository
{
    Task<List<Product>> UploadingProductData();
    Task<Product> GetProductsById(int id);
    Task AddPoduct(Product productToAdd);
    Task UpdateProduct(Product productToUpdate);
    Task DeleteProduct(Product productToDelete);
}

// Реализация (Infrastructure/Implementations/)
public class ProductRepositoryEfCore : IProductRepository
{
    private readonly ApplicationContext _context;
    // ... EF Core реализация
}
```

### Dependency Injection

Все зависимости регистрируются в `API/Program.cs`:

```csharp
// Репозитории
builder.Services.AddScoped<IProductRepository, ProductRepositoryEfCore>();

// Сервисы
builder.Services.AddScoped<IProductService, ProductService>();
```

### DTO-маппинг

Данные конвертируются между сущностями и DTO через extension-методы:

```csharp
// Contracts/Mappings/ProductMapper.cs
public static ProductResponce ToResponse(this Product product) { ... }
```

---

## Известные проблемы

### Критические

| Проблема | Где | Влияние |
|----------|-----|---------|
| **Нет регистрации `ApplicationContext` в DI** | `API/Program.cs` | API не может работать с БД |
| **`OrderItemService` не сохраняет Quantity** | `Application/OrderItemService.cs:41` | Все позиции заказов = 0 шт |
| **`OrderController` путает количество** | `API/OrderController.cs:114` | Заказывает остаток со склада вместо нужного |
| **`UserResponse` отдаёт пароль** | `Contracts/UserResponse.cs:12` | Утечка данных |

### Архитектурные

| Проблема | Где | Влияние |
|----------|-----|---------|
| Application зависит от Infrastructure | `Application.csproj` | Нарушение Clean Architecture |
| Контроллеры inject репозитории напрямую | `OrderController`, `OrderItemController` | Обход слоя сервисов |
| Fluent API конфигурации не применяются | `ApplicationContext.cs` | Мёртвый код в `Configurations/` |
| Пароли в открытом виде | `User.cs` | Нет безопасности |
| Нет аутентификации | Везде | Любой может управлять магазином |

### Мелочи

| Проблема | Где |
|----------|-----|
| Опечатка `AddPoduct` (пропущена 'r') | `IProductRepository`, реализации |
| Опечатка `UppdateProductRequest` (двойная 'p') | `Contracts/Requests/Products/` |
| Опечатка `ProductResponce`, `UpdateOrderItemResponce` | `Contracts/Responses/` |
| Имя файла ≠ имя класса (`ApplicationDbContext.cs` vs `ApplicationContext`) | `Infrastructure/Data/` |
| Domain csproj содержит лишние NuGet-пакеты | `Domain.csproj` |
| `Class1.cs` placeholder | `Application/` |

---

## Дорожная карта

### Приоритет 1 — Исправление критических багов

Эти проблемы необходимо решить в первую очередь, так без них приложение не работает корректно.

#### 1. Регистрация `ApplicationContext` в DI

В `API/Program.cs` отсутствует регистрация контекста базы данных. Без неё ни один репозиторий не может быть создан.

**Что должно быть:**

```csharp
// В Program.cs, перед регистрацией репозиториев
builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));
```

**Зачем:** EF Core DbContext — это основной инструмент работы с БД. Без его регистрации DI-контейнер не знает, как создать репозитории, которые зависят от `ApplicationContext`.

#### 2. Исправление бага с Quantity в OrderItemService

`CreateOrderItem` принимает параметр `quantity`, но не присваивает его позиции заказа.

**Что должно быть:**

```csharp
var orderItem = new OrderItem
{
    OrderId = order.Id,
    ProductId = product.Id,
    Price = product.Price,
    Quantity = quantity  // <-- добавить эту строку
};
```

**Зачем:** Без этого все позиции заказов создаются с количеством 0, что делает заказы бессмысленными.

#### 3. Исправление OrderController.Create

Контроллер использует `product.Quantity` (остаток на складе) вместо `itemReq.Quantity` (сколько заказал клиент).

**Что должно быть:**

```csharp
var orderItem = new OrderItem
{
    ProductId = product.Id,
    Quantity = itemReq.Quantity,  // <-- изменить с product.Quantity
    Price = product.Price
};
```

**Зачем:** Клиент заказал 5 штук, а в заказ записывается 100 (весь склад). Это критическая ошибка бизнес-логики.

#### 4. Убрать пароль из UserResponse

**Что должно быть:**

```csharp
public class UserResponse
{
    public int Id { get; set; }
    public string Email { get; set; }
    public short? Balance { get; set; }
    public UserRole Role { get; set; }
    // Password удалён
}
```

**Зачем:** Пароль в JSON-ответе API — прямая угроза безопасности. Даже если клиент его не показывает, его видит любой, кто перехватит трафик.

---

### Приоритет 2 — Архитектура бэкенда

#### 5. Хеширование паролей

**Как сейчас:** Пароли хранятся в открытом виде в БД.

**Как должно быть:** Использовать ASP.NET Identity с bcrypt/hashing:

```csharp
// В UserService
using Microsoft.AspNetCore.Identity;

public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;

    public async Task<User> CreateStudent(string email, string password)
    {
        var user = new User { Email = email, Role = UserRole.Student };
        // Пароль хешируется автоматически
        await _userManager.CreateAsync(user, password);
        return user;
    }
}
```

**Зачем:** Если базу данных взломают (или пароль утекает через API), хешированные пароли невозможно восстановить. Без хеширования — один взлом = все аккаунты скомпрометированы.

#### 6. Аутентификация и авторизация

**Как сейчас:** Любой может выполнить любой запрос к API. Нет проверки, кто отправил запрос.

**Как должно быть:**

```csharp
// Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

// В контроллерах
[ApiController]
[Route("api/[controller]")]
[Authorize]  // <-- требует токен для всех методов
public class ProductController : ControllerBase
{
    [Authorize(Roles = "Admin")]  // <-- только для админов
    [HttpPost("create-product")]
    public async Task<IActionResult> CreateProduct(...) { ... }
}
```

**Зачем:** Без аутентификации любой человек в интернете может удалить все товары, украсть пароли пользователей, изменить заказы. Авторизация определяет, кому что разрешено (студент — только покупать, админ — управлять).

#### 7. Исправление нарушения Clean Architecture

**Как сейчас:** `Application.csproj` ссылается на `Infrastructure.csproj`. Сервисы знают о конкретных реализациях репозиториев.

**Как должно быть:** Application зависит только от Domain. Интерфейсы репозиториев живут в Domain, а реализации — в Infrastructure.

```
Domain/
|-- Entities/
|-- Enums/
|-- Interfaces/           <-- сюда перенести интерфейсы репозиториев
|   |-- IUserRepository.cs
|   |-- IProductRepository.cs
|   |-- IOrderRepository.cs
|   |-- IOrderItemRepository.cs

Application/
|-- Services/
|-- CifraShop.Domain.csproj  <-- ссылка только на Domain

Infrastructure/
|-- Repositories/
|-- CifraShop.Domain.csproj  <-- ссылка на Domain (интерфейсы)
```

**Зачем:** Это позволяет заменить реализацию репозитория (например, перейти с EF Core на Dapper или MongoDB), не меняя код сервисов. Бизнес-логика не зависит от инструмента.

#### 8. Убрать прямые ссылки на репозитории из контроллеров

**Как сейчас:** `OrderController` и `OrderItemController` принимают `IUserRepository`, `IProductRepository` — обходят слой сервисов.

**Как должно быть:**

```csharp
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IOrderItemService _orderItemService;
    // больше никаких репозиториев

    [HttpPost("create-order")]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
    {
        // Вся логика поиска пользователя и продуктов — в OrderService
        var order = await _orderService.CreateOrder(request);
        ...
    }
}
```

**Зачем:** Контроллеры — тонкий слой, который только обрабатывает HTTP-запросы и вызывает сервисы. Вся бизнес-логика (валидация, транзакции, проверки) должна быть в сервисах.

#### 9. Валидация запросов

**Как сейчас:** Можно отправить пустой `CreateProductRequest` и получить товар с пустым именем.

**Как должно быть:**

```csharp
// Вариант 1: DataAnnotations
public class CreateProductRequest
{
    [Required(ErrorMessage = "Название обязательно")]
    [MaxLength(30)]
    public string Name { get; set; }

    [Required]
    [Range(0, short.MaxValue)]
    public short Price { get; set; }
}

// Вариант 2: FluentValidation (рекомендуется)
public class CreateProductValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название обязательно")
            .MaximumLength(30).WithMessage("Максимум 30 символов");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Цена не может быть отрицательной");
    }
}
```

**Зачем:** Валидация на уровне API не даёт некорректным данным попасть в базу. Это первая линия защиты от мусорных данных и ошибок клиентов.

#### 10. Глобальная обработка ошибок

**Как сейчас:** Исключения утекают клиенту в виде stack traces.

**Как должно быть:**

```csharp
// Middleware/ExceptionHandlingMiddleware.cs
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");

            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Внутренняя ошибка сервера"
            });
        }
    }
}

// Program.cs
app.UseMiddleware<ExceptionHandlingMiddleware>();
```

**Зачем:** Без middleware клиент видит внутренние детали ошибок (имена таблиц, запросы SQL, стек вызовов). Это и information leak, и некрасиво.

#### 11. Логирование

```csharp
// В сервисах
public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly ILogger<ProductService> _logger;

    public async Task<Product> CreateProductData(string name, string description, short price, short quantity)
    {
        _logger.LogInformation("Создание товара: {Name}, цена: {Price}", name, price);
        // ...
        _logger.LogInformation("Товар создан: Id={Id}", product.Id);
        return product;
    }
}
```

**Зачем:** Без логов невозможно понять, что произошло в продакшене. Логирование помогает отслеживать ошибки, аудитировать действия и отлаживать проблемы.

---

### Приоритет 3 — Улучшения админ-панели

#### 12. Реальное время (SignalR)

**Как сейчас:** Данные обновляются каждые 30 секунд опросом.

**Как должно быть:** SignalR hub на бэкенде отправляет уведомления при изменениях:

```csharp
// API/Hubs/AdminHub.cs
public class AdminHub : Hub
{
    public async Task NotifyProductChanged()
    {
        await Clients.All.SendAsync("ProductChanged");
    }
}

// В контроллерах, после изменения
await _hubContext.Clients.All.SendAsync("ProductChanged");
```

**Примечание:** SignalR требует JavaScript на клиенте. Если JS запрещён — остаётся опрос.

#### 13. Раздельные флаги загрузки

**Как сейчас:** Один `_loading` для всех секций.

**Как должно быть:**

```csharp
private bool _loadingProducts = true;
private bool _loadingOrders = true;
private bool _loadingUsers = true;
```

Каждая секция показывает свой спиннер независимо.

#### 14. Отображение даты заказа

Добавить столбец "Дата" в таблицу заказов:

```html
<td><small>@o.DateOfPurchase.ToString("dd.MM.yyyy HH:mm")</small></td>
```

#### 15. Расширение типов Price/Quantity

Заменить `short` (макс. 32 767) на `int` (макс. 2 млрд) в:

- `Product.cs` (Domain)
- `Order.cs` (Domain)
- `OrderItem.cs` (Domain)
- `ProductResponce.cs` (Contracts)
- `UppdateProductRequest.cs` (Contracts)
- `CreateProductRequest.cs` (Contracts)
- `ApiModels.cs` (Client)

**Зачем:** Цена в 50 000 рублей или количество 100 000 единиц не влезают в `short`.

#### 16. Детализация ошибок API

При ошибке показывать сообщение от сервера:

```csharp
else
{
    var error = await r.Content.ReadAsStringAsync();
    ShowNotification($"Ошибка: {error}", "danger");
}
```

---

### Приоритет 4 — Инфраструктура и код

#### 17. Тесты

Создать тесты для:
- Сервисов (单元测试 с моками репозиториев)
- Контроллеров (интеграционные тесты с InMemory DB)
- Валидации DTO

```csharp
[TestClass]
public class ProductServiceTests
{
    [TestMethod]
    public async Task CreateProduct_SetsStatusToOnSaleSoon()
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var service = new ProductService(mockRepo.Object);

        // Act
        var product = await service.CreateProductData("Тест", "Описание", 100, 10);

        // Assert
        Assert.AreEqual(StatusProduct.OnSaleSoon, product.Status);
    }
}
```

#### 18. Исправление опечаток

| Было | Стало | Файлы |
|------|-------|-------|
| `AddPoduct` | `AddProduct` | `IProductRepository`, `ProductRepositoryEfCore`, `ProductService` |
| `UppdateProductRequest` | `UpdateProductRequest` | `Contracts/Requests/Products/` |
| `ProductResponce` | `ProductResponse` | `Contracts/Responses/Products/`, `ProductMapper`, `ProductController` |
| `UpdateOrderItemResponce` | `UpdateOrderItemResponse` | `Contracts/Responses/OrderItem/`, `OrderItemController` |

#### 19. Очистка Domain csproj

Убрать из `CifraShop.Domain.csproj` пакеты, которые там не нужны:
- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `Microsoft.AspNetCore.Components.Authorization`
- `Microsoft.AspNetCore.Components.WebAssembly.Authentication`
- `Microsoft.AspNetCore.Components.WebAssembly.DevServer`
- `Solutaris.InfoWARE.ProtectedBrowserStorage`

Domain должен быть чистым слоем без зависимостей от ASP.NET.

#### 20. Применение Fluent API конфигураций

Раскомментировать и использовать существующие конфигурации:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfiguration(new UserConfiguration());
    modelBuilder.ApplyConfiguration(new ProductConfiguration());
    modelBuilder.ApplyConfiguration(new OrderConfiguration());
    modelBuilder.ApplyConfiguration(new OrderItemConfiguration());
}
```

Или, если конфиги не нужны (inline достаточно) — удалить папку `Configurations/`.

---

## Порты

| Сервис | Протокол | Порт | URL |
|--------|----------|------|-----|
| API | HTTPS | 5000 | `https://localhost:5000` |
| Client | HTTP | 5001 | `http://localhost:5001` |
| Docker (HTTPS) | HTTPS | 8085 | `https://localhost:8085` |
| Docker (HTTP) | HTTP | 8086 | `http://localhost:8086` |

---

## Лицензия

Проект разработан в рамках курса Cifra.
