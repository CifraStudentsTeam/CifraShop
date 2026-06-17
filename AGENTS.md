# AGENTS.md

## Project overview

CifraShop is a .NET 10 shop application using Clean Architecture with a Blazor WebAssembly client and ASP.NET Core Web API backend. UI text is in Russian.

## Solution structure

```
CifraShop.slnx          # XML-based solution file (not classic .sln)
CifraShop.API/           # ASP.NET Core Web API — host/entrypoint
CifraShop.Client/        # Blazor WebAssembly SPA (port 5001)
CifraShop.Application/   # Service interfaces + implementations
CifraShop.Domain/        # Entities (User, Product, Order, OrderItem) + Enums
CifraShop.Infrastructure/# EF Core DbContext, repository pattern, SQL Server
CifraShop.Contracts/     # Request/Response DTOs (currently empty)
CifraShop.Tests/         # MSTest (currently empty)
```

## Key commands

```bash
# Build entire solution
dotnet build CifraShop.slnx

# Run API (port 5000)
dotnet run --project CifraShop.API

# Run Client (port 5001)
dotnet run --project CifraShop.Client

# Run tests
dotnet test CifraShop.Tests/CifraShop.Tests.csproj

# EF Core migrations (from Infrastructure project)
dotnet ef migrations add <MigrationName> --project CifraShop.Infrastructure --startup-project CifraShop.API
dotnet ef database update --project CifraShop.Infrastructure --startup-project CifraShop.API
```

## Architecture notes

- **Dependency flow**: API → Application → Domain; Infrastructure → Domain; Client → Application (via ApiService HTTP calls)
- API exposes controllers; Client calls API over HTTP (no shared project reference between Client and API)
- Repository pattern: interfaces in `Infrastructure/Data/Repositories/Interfaces/`, implementations in `Implementations/` using EF Core
- Service layer: interfaces in `Application/Services/Interfaces/`, implementations in `Implementations/`
- `ApplicationDbContext` (not `ApplicationDbContext`) is the EF Core context class name (in `Infrastructure/Data/`)
- Product entity implements `INotifyPropertyChanged` — this is intentional for Blazor data binding

## Ports and networking

- API: `https://localhost:5000` (dev)
- Client: `http://localhost:5001` (HTTP — HTTPS fails in .NET 10 WasmAppHost dev server)
- Docker: ports 8085 (HTTPS) / 8086 (HTTP)
- CORS policy `ClientCORS` allows `https://localhost:5001` and `http://localhost:5001`
- Client's `HttpClient.BaseAddress` is hardcoded to `https://localhost:5000/` in `Program.cs`

## Database

- SQL Server via `Microsoft.EntityFrameworkCore.SqlServer`
- User Secrets enabled for API project (`UserSecretsId: 643a6a93-...`)
- Identity columns start at seed 0, increment 1
- Relationship config: OrderItem → Order (OrderId), OrderItem → Product (ProductId)

## Conventions

- Nullable reference types enabled across all projects
- Implicit usings enabled
- Repository methods use verb-noun naming: `UploadingProductData`, `GetProductsById`, `ChangeProductName`
- Domain entities use `uint` for price/quantity fields
- Enums: `UserRole` (Student, Admin), `StatusOrder` (AwaitingPayment, PaidFor, ManufacturedBy, Completed), `StatusProduct` (InStock, OutOfStock, OnSaleSoon)
- **Язык**: Все ответы, пояснения и комментарии в коде — на русском языке

## Gotchas

- No README exists — this is the primary reference for repo structure
- `Class1.cs` placeholder files exist in Application and Infrastructure projects
- Contracts project is empty (no DTOs yet)
- Tests project is empty (no test classes yet)
- The solution uses `.slnx` format (XML-based) — older tooling may not recognize it
- `ApplicationDbContext` class name does not match the conventional `AppDbContext` pattern
- API `Program.cs` uses minimal hosting (no `Startup.cs`)
- .NET 10 WasmAppHost dev server throws `InvalidOperationException: Failed to determine web server's IP address or port` with HTTPS in launchSettings — use HTTP for Client
