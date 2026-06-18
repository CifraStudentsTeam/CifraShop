<h1 align="center">CifraShop</h1>

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

## Содержание

- [Что это за проект](#что-это-за-проект)
- [Технологический стек](#технологический-стек)
- [Как запустить](#как-запустить)
- [Структура решения](#структура-решения)
- [Описание проектов](#описание-проектов)
- [API эндпоинты](#api-эндпоинты)
- [Админ-панель](#админ-панель)
- [Архитектура](#архитектура)
- [Порты](#порты)
- [Конвенции](#конвенции)

---

## Что это за проект

**CifraShop** — учебный интернет-магазин, разработанный в рамках курса Cifra. Демонстрирует полный цикл разработки веб-приложения: от архитектуры бэкенда до пользовательского интерфейса.

### Возможности

- **Каталог товаров** — просмотр, поиск, фильтрация, сортировка, пагинация
- **Управление заказами** — создание, смена статуса, просмотр состава, фильтр по дате
- **Управление пользователями** — создание, редактирование баланса, удаление
- **Админ-панель** — полное управление магазином из одного места
- **История действий** — лог всех операций администратора (хранится в БД)
- **Batch-операции** — массовая смена статуса, массовое удаление (один запрос)
- **Debounce поиск** — поиск с задержкой 300мс для оптимизации запросов

---

## Технологический стек

| Технология | Зачем |
|-----------|-------|
| **.NET 10** | Рантайм |
| **ASP.NET Core Web API** | REST API |
| **Entity Framework Core 10** | ORM для работы с БД |
| **SQL Server** | Реляционная база данных |
| **Blazor WebAssembly** | SPA-клиент в браузере |
| **Bootstrap 5.3.8** | CSS-фреймворк |
| **Bootstrap Icons** | Иконки (CDN) |
| **Docker** | Контейнеризация |

---

## Как запустить

### Установка

1. **[.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)**
2. **SQL Server** (локальный или Docker)

### Настройка БД

```bash
cd CifraShop.API
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=.;Database=CifraShopDb;Trusted_Connection=True;TrustServerCertificate=True"
```

### Миграция и запуск

```bash
# Создать миграцию
dotnet ef migrations add InitialCreate --project CifraShop.Infrastructure --startup-project CifraShop.API
dotnet ef database update --project CifraShop.Infrastructure --startup-project CifraShop.API

# Запустить API (порт 5000)
dotnet run --project CifraShop.API

# Запустить клиент (порт 5001)
dotnet run --project CifraShop.Client
```

Откройте `http://localhost:5001/admin` — админ-панель.

### Docker

```bash
docker build -t cifrashop -f CifraShop.API/Dockerfile .
docker run -p 8085:8085 -p 8086:8086 cifrashop
```

---

## Структура решения

```
CifraShop/
├── CifraShop.slnx                    XML-формат решения (.NET 10)
│
├── CifraShop.Domain/                 Ядро (сущности, перечисления)
│   ├── Entities/                     User, Product, Order, OrderItem, AdminAction
│   └── Enums/                        UserRole, StatusProduct, StatusOrder
│
├── CifraShop.Infrastructure/         Инфраструктура (EF Core, репозитории)
│   ├── Data/
│   │   ├── ApplicationContext.cs     DbContext
│   │   ├── Configurations/           Fluent API конфигурации
│   │   └── Repositories/             Интерфейсы + EF Core реализации
│
├── CifraShop.Application/            Бизнес-логика (интерфейсы + сервисы)
│   └── Services/                     IUserService, IProductService, ...
│
├── CifraShop.Contracts/              DTO (Request/Response) + маппинг
│   ├── Requests/                     Входящие DTO
│   ├── Responses/                    Исходящие DTO
│   └── Mappings/                     Extension-методы конвертации
│
├── CifraShop.API/                    Точка входа — ASP.NET Core Web API
│   ├── Program.cs                    DI, CORS, middleware
│   ├── Controllers/                  Контроллеры
│   └── Middleware/                   ExceptionHandlerMiddleware
│
├── CifraShop.Client/                 Blazor WebAssembly (фронтенд)
│   ├── Pages/Admin.razor             Админ-панель
│   ├── Pages/Home.razor              Главная страница
│   ├── Components/                   Переиспользуемые компоненты
│   └── Models/ApiModels.cs           DTO клиента
│
└── CifraShop.Tests/                  Тесты (заготовка)
```

### Поток данных

```
Client ──(HTTP)──> API ──> Application ──> Domain
                           │
                     Infrastructure
                           │
                        БД (SQL Server)
```

---

## Описание проектов

### Domain — Ядро

Не зависит ни от одного проекта. Описывает данные и бизнес-правила.

**Сущности:**

| Сущность | Описание |
|----------|----------|
| `User` | Пользователь (Email, Password, Balance, Role) |
| `Product` | Товар (Name, Description, Price, Quantity, Status). Реализует `INotifyPropertyChanged` |
| `Order` | Заказ (Status, Sum, DateOfPurchase, CustomerLogin) |
| `OrderItem` | Позиция заказа (OrderId, ProductId, Quantity, Price) |
| `AdminAction` | Действие администратора (ActionType, Details, CreatedAt) |

**Перечисления:**

| Enum | Значения |
|------|----------|
| `UserRole` | Student, Admin |
| `StatusProduct` | InStock (0), OutOfStock (1), OnSaleSoon (2) |
| `StatusOrder` | Pending (0), AwaitingPayment (1), PaidFor (2), ManufacturedBy (3), Completed (4) |

### Infrastructure — Инфраструктура

«Мост» между C# кодом и БД. EF Core конфигурации применяются через `ApplyConfiguration`.

**Repository pattern:** каждая сущность имеет интерфейс + EF Core реализацию.

```csharp
public interface IProductRepository
{
    Task<List<Product>> GetAll();
    Task<(List<Product> Items, int TotalCount)> GetAllPaged(int page, int pageSize);
    Task<Product> GetProductById(int id);
    Task AddProduct(Product productToAdd);
    Task UpdateProduct(Product productToUpdate);
    Task DeleteProduct(Product productToDelete);
    Task DeleteRange(List<int> ids);
}
```

### Application — Бизнес-логика

Сервисы — посредники между контроллерами и репозиториями.

```csharp
public class ProductService : IProductService
{
    public async Task<Product> CreateProduct(string name, string description, short price, short quantity)
    {
        var product = new Product
        {
            Name = name, Description = description,
            Price = price, Quantity = quantity,
            Status = StatusProduct.OnSaleSoon  // бизнес-правило
        };
        await _repository.AddProduct(product);
        return product;
    }
}
```

### Contracts — DTO

Клиент не знает про сущности — работает с DTO. Маппинг через extension-методы.

| Request DTO | Response DTO |
|-------------|-------------|
| `CreateProductRequest` | `ProductResponse` |
| `UpdateProductRequest` | `OrderResponse` |
| `CreateOrderRequest` | `OrderItemResponse` |
| `UpdateOrderRequest` | `UserResponse` |
| `CreateUserRequest` | `AdminActionResponse` |
| `UpdateUserRequest` | `PagedResponse<T>` |
| `BatchUpdateOrderStatusRequest` | |
| `BatchDeleteProductsRequest` | |

### API — Точка входа

- Minimal hosting (нет `Startup.cs`)
- `ExceptionHandlerMiddleware` — единая обработка ошибок
- CORS политика `ClientCORS`

### Client — Blazor WebAssembly

- DTO создаются локально в `Models/ApiModels.cs`
- Компоненты: MetricCard, SearchBar, Pagination, Spinner, EmptyState
- HttpClient.BaseAddress: `https://localhost:5000/`

---

## API эндпоинты

### Товары

| Метод | Маршрут | Описание |
|-------|---------|----------|
| `GET` | `api/Product/all` | Все товары |
| `GET` | `api/Product/paged?page=0&pageSize=8` | Товары с пагинацией |
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
| `GET` | `api/Order/paged?page=0&pageSize=8` | Заказы с пагинацией |
| `GET` | `api/Order/by-id?id=X` | По ID |
| `GET` | `api/Order/by-login?login=X` | По логину |
| `GET` | `api/Order/by-status?status=X` | По статусу |
| `GET` | `api/Order/by-sum?sum=X` | По сумме |
| `GET` | `api/Order/by-date?from=&to=` | По диапазону дат |
| `POST` | `api/Order/create-order` | Создать |
| `PUT` | `api/Order/update-order?id=X` | Обновить |
| `POST` | `api/Order/batch-update-status` | Массовая смена статуса |
| `DELETE` | `api/Order/delete-order?id=X` | Удалить |

### Позиции заказов

| Метод | Маршрут | Описание |
|-------|---------|----------|
| `GET` | `api/OrderItem/by-id?id=X` | По ID |
| `GET` | `api/OrderItem/by-orderid?id=X` | По ID заказа |
| `POST` | `api/OrderItem/create-orderitem` | Создать |
| `PUT` | `api/OrderItem/update-orderitem?id=X` | Обновить |
| `DELETE` | `api/OrderItem/delete-orderItem?id=X` | Удалить |

### Пользователи

| Метод | Маршрут | Описание |
|-------|---------|----------|
| `GET` | `api/User/all` | Все пользователи |
| `GET` | `api/User/paged?page=0&pageSize=8` | С пагинацией |
| `GET` | `api/User/all-admins` | Только админы |
| `GET` | `api/User/all-students` | Только студенты |
| `GET` | `api/User/by-id?id=X` | По ID |
| `GET` | `api/User/by-email?email=X` | По email |
| `POST` | `api/User/create-user` | Создать пользователя |
| `POST` | `api/User/create-admin` | Создать админа |
| `POST` | `api/User/create-student` | Создать студента |
| `PUT` | `api/User?id=X` | Обновить |
| `DELETE` | `api/User?id=X` | Удалить |

### История действий

| Метод | Маршрут | Описание |
|-------|---------|----------|
| `GET` | `api/AdminAction/last?count=50` | Последние N действий |
| `POST` | `api/AdminAction` | Записать действие |

---

## Админ-панель

Страница `/admin` — полноценная панель управления.

### Метрики

Три карточки: Товаров, Заказы, Пользователи. Обновляются автоматически каждые 30 секунд.

### Товары

- **Поиск** — debounce 300мс, по названию и описанию
- **Фильтр** — по статусу (В наличии / Нет / Скоро)
- **Сортировка** — по клику на заголовок (▲/▼)
- **Пагинация** — серверная, по 8 товаров
- **Создание** — модалка (название, описание, цена, количество)
- **Редактирование** — модалка с заполненными данными
- **Количество** — инлайн-редактирование в таблице
- **Статус** — клик по бейджу: цикл В наличии → Нет → Скоро
- **Массовые действия** — чекбоксы: Показать / Скрыть / Удалить (batch-запрос)
- **Индикатор загрузки** — кнопка блокируется при сохранении

### Заказы

- **Поиск** — debounce 300мс, по номеру и email
- **Фильтр** — по статусу
- **Фильтр по дате** — два date-picker (from/to)
- **Сортировка** — по номеру, покупателю, сумме, статусу
- **Пагинация** — серверная, по 8 заказов
- **Статус** — выпадающий список в таблице
- **Детали** — клик по строке раскрывает состав заказа
- **Массовые действия** — чекбоксы + batch-смена статуса
- **Создание** — модалка (покупатель + выбор товаров)

### Пользователи

- **Поиск** — debounce 300мс, по email
- **Фильтр** — по роли (Студенты / Админы)
- **Сортировка** — по email, балансу, роли
- **Пагинация** — серверная, по 8 пользователей
- **Создание** — модалка (email, пароль, роль)
- **Баланс** — клик по балансу → модалка редактирования
- **Удаление** — с подтверждением

### История действий

Сворачиваемая панель. Показывает последние 50 операций: время, тип (создание/изменение/удаление), описание. Хранится в БД.

### Уведомления

Всплывающие сообщения: зелёное (успех), красное (ошибка). Автоскрытие 4 сек.

---

## Архитектура

### Clean Architecture

```
┌──────────────────────────────────────────┐
│  Presentation (API Controllers + Client) │
├──────────────────────────────────────────┤
│  Application (Services + DTO)            │
├──────────────────────────────────────────┤
│  Domain (Entities + Enums)               │
├──────────────────────────────────────────┤
│  Infrastructure (EF Core + Repos)        │
└──────────────────────────────────────────┘
```

### Dependency Injection

Все зависимости регистрируются в `API/Program.cs` как Scoped.

### ExceptionHandlerMiddleware

Единая точка обработки необработанных исключений. Логирует через `ILogger`, возвращает JSON с HTTP-кодом.

| Исключение | HTTP-код |
|-----------|----------|
| `ArgumentException` | 400 |
| `KeyNotFoundException` | 404 |
| `UnauthorizedAccessException` | 401 |
| Остальные | 500 |

---

## Порты

| Сервис | Протокол | Порт |
|--------|----------|------|
| API | HTTPS | 5000 |
| Client | HTTP | 5001 |
| Docker (HTTPS) | HTTPS | 8085 |
| Docker (HTTP) | HTTP | 8086 |

---

## Конвенции

- **Язык:** имена классов/методов на английском, комментарии и UI на русском
- **Nullable reference types:** включены во всех проектах
- **Implicit usings:** включены
- **Типы:** `short` для Price/Quantity, `int` для ID
- **Стили:** Bootstrap CSS, `rem` вместо `px` в inline-стилях
- **Коммиты:** описание на русском языке

---

## Дорожная карта

### Сделано

- [x] CRUD для товаров, заказов, пользователей
- [x] Админ-панель с поиском, фильтрами, сортировкой, пагинацией
- [x] Batch-операции (массовое удаление, смена статуса)
- [x] Серверная пагинация
- [x] Debounce поиска
- [x] История действий (в БД)
- [x] ExceptionHandlerMiddleware
- [x] DateTime.UtcNow
- [x] Исправлены баги (Quantity, Sum mapping, пароль в ответе)
- [x] Очищена архитектура (убраны лишние пакеты, исправлены имена)

### Осталось

- [ ] Аутентификация (JWT)
- [ ] Хеширование паролей
- [ ] Валидация DTO
- [ ] Unit-тесты
- [ ] Seed-данные
- [ ] Страница каталога для студентов
- [ ] Корзина и оформление заказа
- [ ] Логирование (ILogger)

---

Проект разработан в рамках курса **Cifra**.
