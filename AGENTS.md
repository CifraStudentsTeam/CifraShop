# AGENTS.md

## Project overview

CifraShop is a .NET 10 shop application using Clean Architecture with a Blazor WebAssembly client and ASP.NET Core Web API backend. UI text is in Russian.

## Solution structure

```
CifraShop.slnx          # XML-based solution file (not classic .sln)
CifraShop.API/           # ASP.NET Core Web API — host/entrypoint
CifraShop.Client/        # Blazor WebAssembly SPA (port 5001)
CifraShop.Application/   # Service interfaces + implementations
CifraShop.Domain/        # Entities (User, Product, Order, OrderItem, AdminAction, NotificationSettings) + Enums
CifraShop.Infrastructure/# EF Core DbContext, repository pattern, SQL Server
CifraShop.Contracts/     # Request/Response DTOs + Mappings (extension methods)
CifraShop.Tests/         # MSTest (currently empty — no test classes)
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

- **Dependency flow**: API → Application → Domain + Contracts; Infrastructure → Domain; Client → Application (via HTTP)
- API exposes controllers; Client calls API over HTTP (no shared project reference between Client and API)
- Repository pattern: interfaces in `Infrastructure/Data/Repositories/Interfaces/`, implementations in `Implementations/` using EF Core
- Service layer: interfaces in `Application/Services/Interfaces/`, implementations in `Implementations/`
- DbContext class is named `ApplicationContext` (file is `ApplicationDbContext.cs` — naming mismatch is intentional/historical)
- Product entity implements `INotifyPropertyChanged` — this is intentional for Blazor data binding
- Contracts project has Request DTOs, Response DTOs, and Mapping extension methods (not empty)

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
- `Infrastructure/Data/SeedData/` exists but is currently empty

## Conventions

- Nullable reference types enabled across all projects
- Implicit usings enabled
- Domain entities use `short` for price/quantity fields (not uint)
- Enums: `UserRole` (Student, Admin), `StatusOrder` (Pending, AwaitingPayment, PaidFor, ManufacturedBy, Completed), `StatusProduct` (InStock, OutOfStock, OnSaleSoon)
- **Language**: All UI text, comments, and commit messages are in Russian
- Bootstrap 5.3.8 via libman (not npm)
- Client DTOs live in `Models/ApiModels.cs`, not in the Contracts project

## Gotchas

- The solution uses `.slnx` format (XML-based) — older tooling may not recognize it
- `ApplicationContext` class name does not follow the conventional `AppDbContext` pattern
- .NET 10 WasmAppHost dev server throws `InvalidOperationException` with HTTPS in launchSettings — use HTTP for Client
- API `Program.cs` uses minimal hosting (no `Startup.cs`)
- `Application/Validators/` and `Infrastructure/Auth/` directories exist but are currently empty
- Client `Services/` directory exists but is currently empty
