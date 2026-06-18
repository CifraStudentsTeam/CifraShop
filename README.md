<h1 align="center">🛒 CifraShop</h1>

<p align="center">
  <b>Интернет-магазин</b>, построенный по архитектуре <b>Clean Architecture</b> на стеке <b>.NET 10</b>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 10">
  <img src="https://img.shields.io/badge/Blazor-WASM-512BD4?style=for-the-badge&logo=blazor&logoColor=white" alt="Blazor WASM">
  <img src="https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white" alt="SQL Server">
  <img src="https://img.shields.io/badge/EF_Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt="EF Core">
  <img src="https://img.shields.io/badge/Bootstrap-5.3-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white" alt="Bootstrap 5.3">
  <img src="https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white" alt="Docker">
</p>

---

## 📋 Содержание

> Кликните на раздел, чтобы перейти к нему

- [Что это за проект](#что-это-за-проект)
- [Технологический стек](#технологический-стек)
- [Как запустить проект](#как-запустить-проект)
- [Структура решения](#структура-решения)
- [Подробное описание каждого проекта](#подробное-описание-каждого-проекта)
- [API эндпоинты](#api-эндпоинты)
- [Админ-панель](#админ-панель)
- [Компоненты клиента](#компоненты-клиента)
- [Архитектура и принципы](#архитектура-и-принципы)
- [Известные проблемы](#известные-проблемы)
- [Дорожная карта](#дорожная-карта)
- [Порты и сети](#порты-и-сети)
- [Конвенции проекта](#конвенции-проекта)

---

## 🎯 Что это за проект

**CifraShop** — это учебный интернет-магазин, разработанный в рамках курса Cifra. Проект демонстрирует полный цикл разработки веб-приложения: от архитектуры бэкенда до пользовательского интерфейса.

### Что умеет

- 🛍️ **Каталог товаров** — просмотр, поиск, фильтрация
- 🛒 **Оформление заказов** — корзина, подтверждение, статусы
- 👥 **Управление пользователями** — роли (Студент / Админ)
- 📊 **Админ-панель** — полное управление магазином из одного места
- 📝 **История действий** — лог всех операций администратора

### Для кого

- Для тех, кто изучает .NET и хочет увидеть **реальный проект** с чистой архитектурой
- Для преподавателей — как пример Clean Architecture на .NET 10
- Для команды — как база для дальнейшего развития

---

## 🔧 Технологический стек

### Бэкенд (серверная часть)

| Технология | Зачем используется | Где в проекте |
|-----------|-------------------|---------------|
| **.NET 10** | Рантайм — среда выполнения C# кода | Все проекты |
| **ASP.NET Core Web API** | Создание REST API — сервер отвечает на HTTP-запросы клиента | `CifraShop.API` |
| **Entity Framework Core 10** | ORM — объектно-реляционное отображение. Позволяет работать с БД через C# объекты, а не SQL-запросы | `CifraShop.Infrastructure` |
| **SQL Server** | Реляционная база данных — хранит товары, заказы, пользователей | Подключение через EF Core |
| **Minimal Hosting** | Современный способ запуска API без `Startup.cs` — всё в `Program.cs` | `CifraShop.API/Program.cs` |

### Фронтенд (клиентская часть)

| Технология | Зачем используется | Где в проекте |
|-----------|-------------------|---------------|
| **Blazor WebAssembly** | SPA-фреймворк — весь интерфейс работает в браузере без перезагрузки страницы | `CifraShop.Client` |
| **Bootstrap 5.3.8** | CSS-фреймворк — стилизация кнопок, таблиц, форм, карточек | `wwwroot/bootstrap/` |
| **Bootstrap Icons** | Иконки — иконки корзины, поиска, редактирования и т.д. | CDN в `index.html` |

### Инфраструктура

| Технология | Зачем используется |
|-----------|-------------------|
| **Docker** | Контейнеризация — упаковка приложения в контейнер для запуска на любом сервере |
| **User Secrets** | Хранение секретов (строка подключения к БД) — не попадают в Git |

---

## 🚀 Как запустить проект

### Шаг 0: Что нужно установить

Перед запуском убедитесь, что установлено:

1. **[.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)** — компилятор и рантайм для C#
2. **SQL Server** — локальная установка или через Docker
3. **Visual Studio 2022** (рекомендуется) или **VS Code** с расширением C#

### Шаг 1: Клонировать репозиторий

```bash
git clone https://github.com/CifraStudentsTeam/CifraShop.git
cd CifraShop
```

### Шаг 2: Настроить базу данных

Строка подключения к SQL Server должна быть настроена. Есть два способа:

**Способ A: User Secrets (рекомендуется)**

```bash
cd CifraShop.API
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=.;Database=CifraShopDb;Trusted_Connection=True;TrustServerCertificate=True"
```

**Способ B: appsettings.json**

Добавьте в `CifraShop.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=CifraShopDb;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

### Шаг 3: Создать и применить миграцию

Миграция — это способ создать структуру таблиц в базе данных из C# кода.

```bash
# Из корня решения
dotnet ef migrations add InitialCreate --project CifraShop.Infrastructure --startup-project CifraShop.API
dotnet ef database update --project CifraShop.Infrastructure --startup-project CifraShop.API
```

**Что делает каждая команда:**
- `migrations add` — анализирует сущности (User, Product, Order...) и генерирует SQL-код для создания таблиц
- `database update` — применяет этот SQL-код к базе данных, создавая таблицы

### Шаг 4: Запустить API

```bash
dotnet run --project CifraShop.API
```

API запустится на `https://localhost:5000`. Проверьте, работает ли: откройте в браузере `https://localhost:5000/openapi/v1.json` — должен отобразиться JSON с описанием API.

### Шаг 5: Запустить клиент

```bash
dotnet run --project CifraShop.Client
```

Клиент запустится на `http://localhost:5001`. Откройте этот адрес в браузере.

### Шаг 6: Открыть админ-панель

Перейдите по адресу `http://localhost:5001/admin` — увидите панель управления товарами, заказами и пользователями.

### Через Docker

```bash
docker build -t cifrashop -f CifraShop.API/Dockerfile .
docker run -p 8085:8085 -p 8086:8086 cifrashop
```

---

## 📁 Структура решения

```
CifraShop/
│
├── CifraShop.slnx               Файл решения (XML-формат .NET 10)
│
├── CifraShop.API/               Точка входа — ASP.NET Core Web API
│   ├── Program.cs               DI, CORS, middleware, маршруты
│   ├── Controllers/             Контроллеры (обработчики HTTP-запросов)
│   │   ├── ProductController.cs
│   │   ├── OrderController.cs
│   │   ├── OrderItemController.cs
│   │   ├── UserController.cs
│   │   └── AdminActionController.cs
│   ├── appsettings.json         Конфигурация (строка подключения и т.д.)
│   └── Dockerfile               Инструкция для сборки Docker-образа
│
├── CifraShop.Client/            Blazor WebAssembly SPA (фронтенд)
│   ├── Program.cs               Регистрация HttpClient
│   ├── App.razor                Маршрутизатор
│   ├── Models/
│   │   └── ApiModels.cs         DTO для клиента (ProductDto, OrderDto, ...)
│   ├── Pages/
│   │   ├── Home.razor           Главная страница
│   │   ├── Admin.razor          Админ-панель
│   │   └── NotFound.razor       Страница 404
│   ├── Components/              Переиспользуемые UI-компоненты
│   │   ├── MetricCard.razor     Карточка метрики
│   │   ├── SearchBar.razor      Поиск + фильтр
│   │   ├── Pagination.razor     Пагинация
│   │   ├── Spinner.razor        Индикатор загрузки
│   │   └── EmptyState.razor     «Ничего не найдено»
│   └── wwwroot/
│       ├── index.html           Точка монтирования Blazor
│       └── bootstrap/           Bootstrap 5.3.8 (CSS, JS, SCSS)
│
├── CifraShop.Application/       Бизнес-логика (интерфейсы и сервисы)
│   ├── Services/
│   │   ├── Interfaces/          IUserService, IProductService, IOrderService, ...
│   │   └── Implementations/     UserService, ProductService, OrderService, ...
│   └── Class1.cs                Placeholder (удалить)
│
├── CifraShop.Domain/            Ядро (сущности и перечисления)
│   ├── Entities/
│   │   ├── User.cs              Пользователь
│   │   ├── Product.cs           Товар
│   │   ├── Order.cs             Заказ
│   │   ├── OrderItem.cs         Позиция заказа
│   │   └── AdminAction.cs       Действие администратора
│   └── Enums/
│       ├── UserRole.cs          Student, Admin
│       ├── StatusProduct.cs     InStock, OutOfStock, OnSaleSoon
│       └── StatusOrder.cs       Pending, AwaitingPayment, PaidFor, ManufacturedBy, Completed
│
├── CifraShop.Infrastructure/    Инфраструктура (EF Core, репозитории)
│   ├── Data/
│   │   ├── ApplicationDbContext.cs   DbContext (контекст БД)
│   │   ├── Configurations/           Fluent API конфигурации (пока не применяются)
│   │   └── Repositories/
│   │       ├── Interfaces/           Интерфейсы репозиториев
│   │       └── Implementations/      EF Core реализации
│   └── CifraShop.Infrastructure.csproj
│
├── CifraShop.Contracts/         Контракты (DTO, маппинг)
│   ├── Requests/                Входящие DTO (что клиент отправляет)
│   ├── Responses/               Исходящие DTO (что API возвращает)
│   └── Mappings/                Extension-методы для конвертации
│
└── CifraShop.Tests/             Тесты (заготовка, пока пусто)
```

### Поток зависимостей

```
┌───────────────────────────────────────────────────────┐
│                    Поток данных                        │
│                                                       │
│   Client ──(HTTP)──> API ──> Application ──> Domain   │
│                                  │                    │
│                            Infrastructure              │
│                                  │                    │
│                               БД (SQL Server)         │
└───────────────────────────────────────────────────────┘
```

**Правило:** каждый слой зависит только от нижележащих. Domain — самый низкий (ни от чего не зависит). API и Client — самые верхние.

---

## 📦 Подробное описание каждого проекта

### CifraShop.Domain — Ядро

> **Зачем нужен:** Это самая важная часть проекта. Здесь описаны все данные, с которыми работает приложение (товары, заказы, пользователи) и бизнес-правила (какие статусы бывают, какие роли существуют).

> **Почему отдельно:** Domain не зависит ни от одного другого проекта. Это значит, что его можно использовать где угодно — в другом приложении, в тестах, в мобильном клиенте.

#### Сущности

**User (Пользователь)**

```csharp
public class User
{
    public int Id { get; set; }
    public string Email { get; set; }           // Email пользователя
    public string Password { get; set; }        // Пароль (открыто — нужно хешировать!)
    public short? Balance { get; set; }         // Баланс (может быть null)
    public UserRole Role { get; set; }          // Роль: Student или Admin
    public List<Order> Orders { get; set; }     // Связь: у пользователя много заказов
}
```

**Product (Товар)**

```csharp
public class Product : INotifyPropertyChanged  // Реализует INotifyPropertyChanged для Blazor
{
    public int Id { get; set; }
    public string Name { get; set; }            // Название (макс. 30 символов)
    public string Description { get; set; }     // Описание (макс. 100 символов)
    public short Price { get; set; }            // Цена в рублях (макс. 32 767)
    public short Quantity { get; set; }         // Количество на складе
    public StatusProduct Status { get; set; }   // Статус: InStock / OutOfStock / OnSaleSoon
    public string? ThePathToTheImage { get; set; } // Путь к изображению (пока не используется)
}
```

**Order (Заказ)**

```csharp
public class Order
{
    public int Id { get; set; }
    public StatusOrder Status { get; set; }     // Статус заказа (5 вариантов)
    public short Sum { get; set; }              // Сумма заказа
    public DateTime DateOfPurchase { get; set; } // Дата покупки
    public string CustomerLogin { get; set; }   // Email покупателя
    public int CustomerId { get; set; }         // ID покупателя (связь с User)
    public ICollection<OrderItem> OrderItems { get; set; } // Состав заказа
}
```

**OrderItem (Позиция заказа)**

```csharp
public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }            // ID заказа
    public int ProductId { get; set; }          // ID товара
    public short Quantity { get; set; }         // Количество в заказе
    public short Price { get; set; }            // Цена на момент заказа
}
```

**AdminAction (Действие администратора)**

```csharp
public class AdminAction
{
    public int Id { get; set; }
    public string ActionType { get; set; }     // Тип: "Создание", "Изменение", "Удаление"
    public string Details { get; set; }         // Описание: "Товар «Ноутбук»"
    public DateTime CreatedAt { get; set; }     // Время действия
}
```

#### Перечисления (Enums)

| Enum | Значения | Описание |
|------|----------|----------|
| `UserRole` | `Student`, `Admin` | Роль пользователя |
| `StatusProduct` | `InStock` (0), `OutOfStock` (1), `OnSaleSoon` (2) | Статус товара |
| `StatusOrder` | `Pending` (0), `AwaitingPayment` (1), `PaidFor` (2), `ManufacturedBy` (3), `Completed` (4) | Статус заказа |

---

### CifraShop.Infrastructure — Инфраструктура

> **Зачем нужен:** Это «мост» между C# кодом и базой данных. Здесь описано, как данные из сущностей (User, Product...) сохраняются в таблицы SQL Server и обратно.

> **Почему отдельно:** Если завтра вы решите заменить SQL Server на PostgreSQL или MongoDB, нужно будет изменить только этот проект — остальной код не пострадает.

#### ApplicationContext (контекст БД)

```csharp
public class ApplicationContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<AdminAction> AdminActions => Set<AdminAction>();
}
```

Это главный класс EF Core. Он описывает, какие таблицы есть в базе и как они связаны.

#### Repository pattern (паттерн репозиторий)

Каждая сущность имеет:
1. **Интерфейс** — описывает, какие операции возможны (получить все, получить по ID, добавить, обновить, удалить)
2. **Реализацию** — конкретный код с EF Core

```csharp
// Интерфейс — абстракция
public interface IProductRepository
{
    Task<List<Product>> UploadingProductData();    // Получить все товары
    Task<Product> GetProductsById(int id);          // Получить товар по ID
    Task AddPoduct(Product productToAdd);            // Добавить товар
    Task UpdateProduct(Product productToUpdate);    // Обновить товар
    Task DeleteProduct(Product productToDelete);    // Удалить товар
}

// Реализация — конкретный код с EF Core
public class ProductRepositoryEfCore : IProductRepository
{
    private readonly ApplicationContext _context;

    public async Task<List<Product>> UploadingProductData()
        => await _context.Products.ToListAsync();
    // ... остальные методы
}
```

#### Связи между таблицами

```
┌──────────┐     1:N     ┌──────────┐     1:N     ┌────────────┐
│   User   │ ─────────── │  Order   │ ─────────── │ OrderItem  │
│          │             │          │             │            │
│ Id       │             │ Id       │             │ Id         │
│ Email    │             │ Status   │             │ OrderId    │
│ Password │             │ Sum      │             │ ProductId  │
│ Role     │             │ Date     │             │ Quantity   │
└──────────┘             │ CustId   │             │ Price      │
                         └──────────┘             └────────────┘
                                                      │
                                                      │ N:1
                                                      ▼
                                                 ┌──────────┐
                                                 │ Product  │
                                                 │          │
                                                 │ Id       │
                                                 │ Name     │
                                                 │ Price    │
                                                 │ Status   │
                                                 └──────────┘
```

- **User → Order:** один пользователь может иметь много заказов (связь через `CustomerId`)
- **Order → OrderItem:** один заказ содержит много позиций (каскадное удаление — при удалении заказа удаляются и его позиции)
- **Product → OrderItem:** один товар может быть в разных заказах (RESTRICT — нельзя удалить товар, если он есть в заказе)

---

### CifraShop.Application — Бизнес-логика

> **Зачем нужен:** Это «переводчик» между контроллерами (которые обрабатывают HTTP-запросы) и репозиториями (которые работают с БД). Здесь происходит настоящая бизнес-логика.

> **Пример:** Когда вы создаёте товар, сервис автоматически ставит статус `OnSaleSoon` («Скоро в продаже»). Это бизнес-правило — оно не в контроллере (тот только принимает запрос) и не в репозитории (тот только пишет в БД).

```csharp
public class ProductService : IProductService
{
    private readonly IProductRepository _repository;

    public async Task<Product> CreateProductData(string name, string description, short price, short quantity)
    {
        var product = new Product
        {
            Name = name,
            Description = description,
            Price = price,
            Quantity = quantity,
            Status = StatusProduct.OnSaleSoon  // <-- бизнес-правило
        };

        await _repository.AddPoduct(product);
        return product;
    }
}
```

---

### CifraShop.Contracts — Контракты

> **Зачем нужен:** Определяет, какие данные передаются между API и клиентом. Клиент не знает про сущности (User, Product) — он работает с DTO (UserDto, ProductDto).

> **Почему не напрямую с сущностями:** Если изменить сущность (добавить поле Password), это не сломает клиент. DTO — это «маска» для сущности.

#### Запросы (Requests) — что клиент отправляет

| DTO | Поля | Зачем |
|-----|------|-------|
| `CreateProductRequest` | Name, Description, Price, Quantity | Создание товара |
| `UppdateProductRequest` | Name?, Description?, Price?, Quantity?, Status? | Обновление товара (все поля опциональны) |
| `CreateOrderRequest` | CustomerLogin, Items[] | Создание заказа |
| `UpdateOrderRequest` | Status?, Sum? | Обновление заказа |
| `CreateUserRequest` | Email, Password | Создание пользователя |
| `UpdateUserRequest` | Email?, Password?, Balance? | Обновление пользователя |
| `CreateAdminActionRequest` | ActionType, Details | Запись действия администратора |

#### Ответы (Responses) — что API возвращает

| DTO | Поля | Зачем |
|-----|------|-------|
| `ProductResponce` | Id, Name, Description, Price, Quantity, Status | Данные товара |
| `OrderResponse` | Id, Status, Sum, DateOfPurchase, CustomerLogin, Items[] | Данные заказа |
| `UserResponse` | Id, Email, Password, Balance, Role | Данные пользователя |
| `AdminActionResponse` | Id, ActionType, Details, CreatedAt | Действие администратора |

#### Маппинг — конвертация сущностей в DTO

```csharp
// Extension-метод: Product -> ProductResponce
public static ProductResponce ToResponse(this Product product)
{
    return new ProductResponce
    {
        Id = product.Id,
        Name = product.Name,
        Description = product.Description,
        Price = product.Price,
        Quantity = product.Quantity,
        Status = product.Status,
    };
}
```

---

### CifraShop.API — Веб-API

> **Зачем нужен:** Это точка входа в приложение. Он принимает HTTP-запросы от клиента (браузера) и возвращает JSON-ответы.

> **Как работает:** Клиент отправляет, например, `GET api/Product/all`. Контроллер `ProductController` получает этот запрос, вызывает `IProductService.GetAllProducts()`, тот обращается к `IProductRepository`, тот читает данные из БД, и результат возвращается клиенту в формате JSON.

```csharp
// Program.cs — регистрация зависимостей
builder.Services.AddScoped<IProductRepository, ProductRepositoryEfCore>();
builder.Services.AddScoped<IProductService, ProductService>();
// ... остальные зависимости

// CORS — разрешаем клиенту обращаться к API
builder.Services.AddCors(options =>
{
    options.AddPolicy("ClientCORS", policy =>
    {
        policy.WithOrigins("https://localhost:5001", "http://localhost:5001")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

---

### CifraShop.Client — Blazor WebAssembly

> **Зачем нужен:** Это интерфейс, который видит пользователь. Одностраничное приложение (SPA) — вся страница загружается один раз, а потом обновляется без перезагрузки.

> **Как работает:** Blazor компилирует C# код в WebAssembly (байткод), который выполняется прямо в браузере. Клиент общается с API через HTTP-запросы.

```
Пользователь нажимает кнопку
        ↓
Blazor вызывает C# метод
        ↓
C# метод отправляет HTTP-запрос к API
        ↓
API возвращает JSON
        ↓
Blazor обновляет интерфейс (без перезагрузки страницы)
```

---

## 🌐 API эндпоинты

### Товары

| Метод | Маршрут | Описание | Тело запроса |
|-------|---------|----------|--------------|
| `GET` | `api/Product/all` | Получить все товары | — |
| `GET` | `api/Product/by-id?id=X` | Получить товар по ID | — |
| `GET` | `api/Product/by-name?name=X` | Поиск по имени | — |
| `GET` | `api/Product/by-price?price=X` | Фильтр по цене | — |
| `GET` | `api/Product/by-quantity?quantity=X` | Фильтр по количеству | — |
| `GET` | `api/Product/by-status?statusProduct=X` | Фильтр по статусу | — |
| `POST` | `api/Product/create-product` | Создать товар | `{ Name, Description, Price, Quantity }` |
| `PUT` | `api/Product/update-product?id=X` | Обновить товар | `{ Name?, Description?, Price?, Quantity?, Status? }` |
| `DELETE` | `api/Product/delete-product?id=X` | Удалить товар | — |

### Заказы

| Метод | Маршрут | Описание | Тело запроса |
|-------|---------|----------|--------------|
| `GET` | `api/Order/all` | Все заказы (с позициями) | — |
| `GET` | `api/Order/by-id?id=X` | Заказ по ID | — |
| `GET` | `api/Order/by-login?login=X` | Заказы по логину клиента | — |
| `GET` | `api/Order/by-status?status=X` | Фильтр по статусу | — |
| `GET` | `api/Order/by-sum?sum=X` | Фильтр по сумме | — |
| `POST` | `api/Order/create-order` | Создать заказ | `{ CustomerLogin, Items[] }` |
| `PUT` | `api/Order/update-order?id=X` | Обновить статус/сумму | `{ Status?, Sum? }` |
| `DELETE` | `api/Order/delete-order?id=X` | Удалить заказ + позиции | — |

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

## 📊 Админ-панель

Страница `/admin` — полноценная панель управления магазином.

### Метрики

Три карточки вверху страницы показывают общую статистику:

| Карточка | Что показывает | Цвет иконки |
|----------|---------------|-------------|
| Товаров | Количество товаров в базе | Синий |
| Заказы | Количество заказов | Жёлтый |
| Пользователи | Количество пользователей | Зелёный |

Данные обновляются автоматически каждые 30 секунд.

### Управление товарами

| Возможность | Как работает |
|-------------|-------------|
| **Поиск** | Вводите текст — товары фильтруются мгновенно при каждом нажатии клавиши. Ищет по названию и описанию |
| **Фильтр** | Выпадающий список: Все / В наличии / Нет в наличии / Скоро |
| **Сортировка** | Клик по заголовку столбца — сортировка по возрастанию (▲), повторный клик — по убыванию (▼) |
| **Пагинация** | По 8 товаров на страницу. Навигация: кнопки «Вперёд/Назад» + номера страниц |
| **Создание** | Кнопка «Добавить» → модальное окно с полями: название, описание, цена, количество |
| **Редактирование** | Кнопка «Карандаш» → модальное окно с заполненными данными |
| **Количество** | Поле ввода прямо в таблице — меняете число, оно сохраняется автоматически |
| **Статус** | Клик по бейджу — переключает «В наличии» ↔ «Нет в наличии» |
| **Массовые действия** | Выберите несколько товаров чекбоксами → появляется панель: Показать / Скрыть / Удалить |
| **Удаление** | Кнопка «Корзина» → модальное окно подтверждения «Удалить «Ноутбук»?» |

### Управление заказами

| Возможность | Как работает |
|-------------|-------------|
| **Поиск** | По номеру заказа и email покупателя |
| **Фильтр** | По статусу: Ожидание / Ожидает оплаты / Оплачен / В производстве / Выполнен |
| **Сортировка** | По номеру, покупателю, сумме, статусу |
| **Статус** | Выпадающий список прямо в таблице — меняете статус, он сохраняется мгновенно |
| **Детали** | Клик по строке заказа → раскрывается состав: список товаров с количеством, ценой и итогом |
| **Дата** | Колонка «Дата» показывает дату покупки (dd.MM.yy) |
| **Массовые действия** | Чекбоксы + выпадающий список для смены статуса выбранных заказов |

### Управление пользователями

| Возможность | Как работает |
|-------------|-------------|
| **Поиск** | По email |
| **Фильтр** | По роли: Все / Студенты / Админы |
| **Сортировка** | По email, балансу, роли |
| **Удаление** | С подтверждением через модальное окно |

### История действий

Сворачиваемая панель внизу страницы. Показывает все операции администраторов:

| Поле | Описание |
|------|----------|
| **Время** | Таймстамп (HH:mm:ss) |
| **Тип** | Цветной бейдж: Создание (зелёный), Изменение (синий), Удаление (красный) |
| **Описание** | Что было сделано: «Товар «Ноутбук»», «Заказ #5: статус = Оплачен» |

**Важно:** История хранится на бэкенде (в таблице `AdminActions`). Это значит:
- Все администраторы видят одну историю (не только свои действия)
- История сохраняется после перезагрузки страницы
- Последние 50 действий загружаются при открытии страницы

### Уведомления

Внизу шапки появляется всплывающее сообщение:
- 🟢 Зелёное — успешная операция («Товар создан»)
- 🔴 Красное — ошибка («Ошибка: сервер недоступен»)
- Автоскрытие через 4 секунды
- Кнопка «X» для ручного закрытия

---

## 🧩 Компоненты клиента

| Компонент | Файл | Назначение |
|-----------|------|-----------|
| `MetricCard` | `Components/MetricCard.razor` | Карточка метрики: название, число, иконка, цвет фона |
| `SearchBar` | `Components/SearchBar.razor` | Поле поиска + выпадающий фильтр. Принимает ChildContent для опций |
| `Pagination` | `Components/Pagination.razor` | Навигация по страницам: Prev / номера / Next |
| `Spinner` | `Components/Spinner.razor` | Индикатор загрузки (спиннер + текст «Загрузка...») |
| `EmptyState` | `Components/EmptyState.razor` | Заглушка «Ничего не найдено» |

---

## 🏗 Архитектура и принципы

### Clean Architecture

Проект следует паттерну **Clean Architecture** (Чистая архитектура) — каждый слой имеет свою ответственность и зависит только от нижележащих:

```
┌─────────────────────────────────────────────────────────────┐
│                  Presentation Layer                          │
│            (API Controllers + Blazor Client)                 │
│                                                             │
│   Отвечает за: HTTP-запросы, рендеринг UI, маршрутизацию    │
├─────────────────────────────────────────────────────────────┤
│                  Application Layer                           │
│              (Services + Interfaces + DTO)                   │
│                                                             │
│   Отвечает за: бизнес-логику, валидацию, оркестрацию        │
├─────────────────────────────────────────────────────────────┤
│                    Domain Layer                              │
│            (Entities + Enums + Business Rules)               │
│                                                             │
│   Отвечает за: данные и правила (самый стабильный слой)     │
├─────────────────────────────────────────────────────────────┤
│                 Infrastructure Layer                         │
│          (EF Core + Repositories + External APIs)            │
│                                                             │
│   Отвечает за: доступ к БД, файловой системе, внешним API   │
└─────────────────────────────────────────────────────────────┘
```

**Зачем это нужно:** Если завтра вы решите заменить SQL Server на MongoDB, нужно изменить только Infrastructure. Если решите заменить Blazor на React — только Presentation. Domain и Application останутся без изменений.

### Dependency Injection (Внедрение зависимостей)

Все зависимости регистрируются в `API/Program.cs`:

```csharp
// Репозитории (работа с БД)
builder.Services.AddScoped<IProductRepository, ProductRepositoryEfCore>();
builder.Services.AddScoped<IOrderRepository, OrderRepositoryEfCore>();
builder.Services.AddScoped<IUserRepository, UserRepositoryEfCore>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepositoryEfCore>();
builder.Services.AddScoped<IAdminActionRepository, AdminActionRepositoryEfCore>();

// Сервисы (бизнес-логика)
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IOrderItemService, OrderItemService>();
builder.Services.AddScoped<IAdminActionService, AdminActionService>();
```

**Scoped** — означает, что для каждого HTTP-запроса создаётся новый экземпляр. Это безопасно для работы с БД.

---

## ⚠️ Известные проблемы

### 🔴 Критические (ломают работу)

| # | Проблема | Где | Что происходит |
|---|----------|-----|----------------|
| 1 | **Нет `AddDbContext` в DI** | `API/Program.cs` | API не может подключиться к БД — все репозитории падают |
| 2 | **`OrderItemService` не сохраняет Quantity** | `OrderItemService.cs:41` | Все позиции заказов создаются с количеством 0 |
| 3 | **`OrderController` путает количество** | `OrderController.cs:114` | В заказ записывается остаток со склада вместо заказанного |
| 4 | **`UserResponse` отдаёт пароль** | `UserResponse.cs:12` | Пароль виден в JSON-ответе API |

### 🟡 Архитектурные (нужно исправить)

| # | Проблема | Почему это плохо |
|---|----------|-----------------|
| 5 | Application зависит от Infrastructure | Нарушение Clean Architecture — нельзя заменить EF Core |
| 6 | Контроллеры inject репозитории напрямую | Обход слоя сервисов — бизнес-логика в контроллерах |
| 7 | Fluent API конфигурации не применяются | Мёртвый код — конфиги существуют, но не используются |
| 8 | Пароли в открытом виде | Угроза безопасности — при утечке БД пароли доступны |
| 9 | Нет аутентификации | Любой может удалить товары, посмотреть пароли |

### 🔵 Мелочи (приятно бы исправить)

| # | Проблема | Где |
|---|----------|-----|
| 10 | Опечатка `AddPoduct` (пропущена 'r') | IProductRepository, реализации |
| 11 | Опечатка `UppdateProductRequest` (двойная 'p') | Contracts/Requests/Products/ |
| 12 | Опечатка `ProductResponce` / `UpdateOrderItemResponce` | Contracts/Responses/ |
| 13 | Файл ≠ имя класса (`ApplicationDbContext.cs` vs `ApplicationContext`) | Infrastructure/Data/ |
| 14 | Domain csproj содержит лишние NuGet-пакеты | Domain.csproj |
| 15 | `Class1.cs` placeholder | Application/ |

---

## 🗺 Дорожная карта

### Приоритет 1 — Критические исправления

> Без этих пунктов приложение не работает корректно. Начать с них.

<details>
<summary><b>1. Регистрация ApplicationContext в DI</b></summary>

В `API/Program.cs` нет `AddDbContext<ApplicationContext>(...)`. Без него ни один репозиторий не может быть создан.

**Что добавить в Program.cs:**

```csharp
builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));
```

**Зачем:** EF Core DbContext — это «проводник» между C# кодом и БД. Без его регистрации DI-контейнер не знает, как создать репозитории.

</details>

<details>
<summary><b>2. Исправить OrderItemService.CreateOrderItem</b></summary>

Параметр `quantity` принимается, но не присваивается позиции заказа.

**Исправление:**

```csharp
var orderItem = new OrderItem
{
    OrderId = order.Id,
    ProductId = product.Id,
    Price = product.Price,
    Quantity = quantity  // <-- добавить
};
```

**Зачем:** Без этого все позиции заказов создаются с количеством 0 — заказы бессмысленны.

</details>

<details>
<summary><b>3. Исправить OrderController.Create</b></summary>

Контроллер использует `product.Quantity` (остаток на складе) вместо `itemReq.Quantity` (сколько заказал клиент).

**Исправление:**

```csharp
Quantity = itemReq.Quantity  // заменить product.Quantity
```

**Зачем:** Клиент заказал 5 штук, а в заказ записывается 100 (весь склад).

</details>

<details>
<summary><b>4. Убрать пароль из UserResponse</b></summary>

Удалить поле `Password` из `UserResponse.cs`.

**Зачем:** Пароль в JSON-ответе — прямая угроза безопасности. Даже если клиент его не показывает, его видит любой, кто перехватит трафик.

</details>

---

### Приоритет 2 — Безопасность

> Без этих пунктов приложение уязвимо.

<details>
<summary><b>5. Хеширование паролей</b></summary>

**Как сейчас:** Пароли хранятся открыто в БД.

**Как должно быть:** Использовать ASP.NET Identity с bcrypt:

```csharp
await _userManager.CreateAsync(user, password); // пароль хешируется автоматически
```

**Зачем:** Если БД взломают, хешированные пароли невозможно восстановить. Без хеширования — один взлом = все аккаунты.

</details>

<details>
<summary><b>6. JWT аутентификация + авторизация</b></summary>

**Как сейчас:** Любой может выполнить любой запрос к API.

**Как должно быть:**

```csharp
[Authorize(Roles = "Admin")]
[HttpPost("create-product")]
public async Task<IActionResult> CreateProduct(...) { ... }
```

**Зачем:** Без аутентификации любой человек в интернете может удалить товары, украсть пароли, изменить заказы.

</details>

---

### Приоритет 3 — Архитектура

> Делает код поддерживаемым и расширяемым.

<details>
<summary><b>7. Исправить Clean Architecture</b></summary>

Перенести интерфейсы репозиториев из Infrastructure в Domain. Application не должен ссылаться на Infrastructure.

</details>

<details>
<summary><b>8. Убрать репозитории из контроллеров</b></summary>

`OrderController` и `OrderItemController` инжектят репозитории напрямую — обходят слой сервисов.

</details>

<details>
<summary><b>9. Валидация запросов</b></summary>

FluentValidation или DataAnnotations. Сейчас можно создать товар с пустым именем.

</details>

<details>
<summary><b>10. Глобальная обработка ошибок</b></summary>

Middleware для перехвата исключений. Сейчас stack traces утекают клиенту.

</details>

<details>
<summary><b>11. Логирование</b></summary>

`ILogger<T>` в сервисах. Без логов невозможно отлаживать в продакшене.

</details>

---

### Приоритет 4 — Улучшения клиента

> Делает админ-панель удобнее и функциональнее.

<details>
<summary><b>12. Реальное время (SignalR)</b></summary>

Заменить 30-секундный опрос на push-уведомления. Требует JS на клиенте.

</details>

<details>
<summary><b>13. Расширить типы Price/Quantity</b></summary>

Заменить `short` (макс. 32 767) на `int` (макс. 2 млрд) везде в Domain и DTO.

</details>

<details>
<summary><b>14. Тесты</b></summary>

单元 тесты сервисов (с моками), интеграционные тесты контроллеров (InMemory DB), тесты валидации.

</details>

<details>
<summary><b>15. Исправить опечатки</b></summary>

`AddPoduct` → `AddProduct`, `UppdateProductRequest` → `UpdateProductRequest`, `ProductResponce` → `ProductResponse`.

</details>

<details>
<summary><b>16. Очистить Domain csproj</b></summary>

Убрать JWT, Components, DevServer пакеты — Domain должен быть чистым.

</details>

<details>
<summary><b>17. Применить или удалить Fluent API конфигурации</b></summary>

Сейчас конфиги в `Configurations/` не используются. Либо применить, либо удалить.

</details>

---

## 🔌 Порты и сети

| Сервис | Протокол | Порт | URL | Когда использовать |
|--------|----------|------|-----|-------------------|
| API | HTTPS | 5000 | `https://localhost:5000` | Разработка (основной) |
| Client | HTTP | 5001 | `http://localhost:5001` | Разработка (фронтенд) |
| Docker (HTTPS) | HTTPS | 8085 | `https://localhost:8085` | Docker-контейнер |
| Docker (HTTP) | HTTP | 8086 | `http://localhost:8086` | Docker-контейнер |

**Почему Client на HTTP:** В .NET 10 WasmAppHost dev server HTTPS вызывает краш (`InvalidOperationException`). CORS политика разрешает оба варианта.

---

## 📐 Конвенции проекта

### Язык

- Все ответы, пояснения и комментарии в коде — на **русском языке**
- Имена классов, методов, переменных — на **английском** (стандарт C#)

### Стиль кода

- Nullable reference types включены во всех проектах
- Implicit usings включены
- `short` для Price/Quantity (макс. 32 767)
- Bootstrap CSS для стилей (никаких кастомных CSS)
- `rem` вместо `px` в inline-стилях

### Архитектурные правила

- Domain не зависит ни от одного проекта
- Client общается с API только через HTTP
- Все DTO в `Contracts` (не в Domain)
- Repository pattern для работы с БД
- Сервисы — посредники между контроллерами и репозиториями

---

## 📄 Лицензия

Проект разработан в рамках курса **Cifra**.
