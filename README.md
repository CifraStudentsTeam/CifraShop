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
  <a href="#features">Features</a> &bull;
  <a href="#quick-start">Quick Start</a> &bull;
  <a href="#project-structure">Structure</a> &bull;
  <a href="#api-endpoints">API</a> &bull;
  <a href="#admin-panel">Admin Panel</a> &bull;
  <a href="#architecture">Architecture</a> &bull;
  <a href="#launcher">Launcher</a>
</p>

---

## Features

<table>
  <tr>
    <td><b>Real-time Updates</b></td>
    <td>SignalR — all changes (products, orders, users) appear instantly across all connected admin panels without page refresh. Auto-reconnect with fallback polling every 5 min</td>
  </tr>
  <tr>
    <td><b>Products</b></td>
    <td>Full CRUD, photo management, inline quantity editing, status toggle, batch operations (status change, delete). Server-side search with 300ms debounce</td>
  </tr>
  <tr>
    <td><b>Orders</b></td>
    <td>Creation with email autocomplete, status management, expandable details with product images, date filtering, batch status change</td>
  </tr>
  <tr>
    <td><b>Users</b></td>
    <td>CRUD with role assignment (Student/Admin), inline balance editing</td>
  </tr>
  <tr>
    <td><b>Notifications</b></td>
    <td>Per-branch notification settings: email, Telegram Bot, low-stock thresholds. Admin-to-branch binding</td>
  </tr>
  <tr>
    <td><b>Action History</b></td>
    <td>Full audit log with filtering by type and branch. Auto-timestamped</td>
  </tr>
  <tr>
    <td><b>Search &amp; Filtering</b></td>
    <td>Highlight matching text, server-side pagination, URL-persisted filters (shareable links)</td>
  </tr>
  <tr>
    <td><b>Launcher</b></td>
    <td>One-command startup: Docker, DB, migrations, API, client. Interactive menu with monitoring and auto-restart</td>
  </tr>
</table>

---

## Quick Start

### Prerequisites

1. **[.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)**
2. **Docker Desktop** (for SQL Server)

### Option A: Launcher (recommended)

```bash
dotnet run --project CifraShop.Launcher
```

The launcher automates everything:
- Checks .NET SDK and Docker
- Creates `docker-compose.yml` and connection string
- Starts SQL Server in Docker
- Applies EF Core migrations
- Starts API and Client
- Provides an interactive management menu

Quick mode (skip already-completed phases):
```bash
dotnet run --project CifraShop.Launcher -- --quick
```

### Option B: Manual

```bash
# Start SQL Server
docker compose up -d db

# Apply migrations
dotnet ef database update --project CifraShop.Infrastructure --startup-project CifraShop.API

# Start API (port 5000)
dotnet run --project CifraShop.API --urls http://localhost:5000

# Start Client (port 5001)
dotnet run --project CifraShop.Client
```

Open **http://localhost:5001/admin** — the admin panel.

---

## Project Structure

```
CifraShop/
├── CifraShop.slnx                     .NET 10 solution (XML format)
│
├── CifraShop.Domain/                  Core — entities, enums, repository interfaces
│   ├── Entities/                      User, Product, Order, OrderItem, AdminAction, etc.
│   ├── Enums/                         UserRole, StatusProduct, StatusOrder
│   └── Repositories/                  IProductRepository, IOrderRepository, ...
│
├── CifraShop.Infrastructure/          EF Core, SQL Server, repository implementations
│   └── Data/
│       ├── ApplicationDbContext.cs    ApplicationContext (DbContext)
│       ├── Configurations/            Fluent API configurations
│       └── Repositories/Implementations/
│
├── CifraShop.Application/             Business logic — services
│   └── Services/
│       ├── Interfaces/                IOrderService, IProductService, ...
│       └── Implementations/           OrderService, ProductService, ...
│
├── CifraShop.Contracts/               DTOs (Request/Response) + mappings
│   ├── Requests/                      CreateProductRequest, BatchUpdateRequest, ...
│   ├── Responses/                     ProductResponse, PagedResponse<T>, ...
│   └── Mappings/                      Extension methods: ToResponse()
│
├── CifraShop.API/                     ASP.NET Core Web API — entry point
│   ├── Program.cs                     DI, CORS, SignalR, middleware
│   ├── Controllers/                   7 controllers + ExceptionHandlerMiddleware
│   ├── Hubs/                          AdminHub (SignalR)
│   └── Dockerfile                     Multi-stage Docker build
│
├── CifraShop.Client/                  Blazor WebAssembly — SPA client
│   ├── Pages/
│   │   ├── Admin.razor                Admin panel (orchestrator)
│   │   └── Components/                5 child components:
│   │       ├── AdminProductsPanel     Products CRUD + image management
│   │       ├── AdminOrdersPanel       Orders CRUD + status management
│   │       ├── AdminUsersPanel        Users CRUD + balance editing
│   │       ├── AdminNotificationsPanel Branch notification settings
│   │       └── AdminHistoryPanel      Action audit log
│   ├── Services/SignalRService.cs     SignalR client with auto-reconnect
│   ├── Models/ApiModels.cs            Client-side DTOs
│   └── wwwroot/                       Bootstrap 5.3.8 + Icons (local)
│
├── CifraShop.Launcher/                Console launcher — startup automation
│   └── Program.cs                     6 phases + menu + monitoring
│
├── CifraShop.Tests/                   MSTest (skeleton)
└── README.md
```

### Data Flow

```
┌──────────────┐   HTTP/WS   ┌──────────────┐
│   Client     │ <──────────> │   API        │
│   (Blazor)   │  SignalR     │   Controllers│
└──────────────┘              └──────┬───────┘
                                     │
                              ┌──────▼───────┐
                              │  Application  │
                              │   Services    │
                              └──────┬───────┘
                                     │
                       ┌─────────────┼─────────────┐
                ┌──────▼──────┐           ┌───────▼──────┐
                │   Domain    │           │Infrastructure│
                │  Entities   │           │   EF Core    │
                └─────────────┘           └──────┬───────┘
                                                 │
                                          ┌──────▼───────┐
                                          │  SQL Server   │
                                          └──────────────┘
```

---

## API Endpoints

### Products

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `api/Product/all` | All products |
| `GET` | `api/Product/paged?page=0&pageSize=8` | Paginated with search &amp; filter |
| `POST` | `api/Product/create-product` | Create |
| `PUT` | `api/Product/update-product?id=X` | Update |
| `DELETE` | `api/Product/delete-product?id=X` | Delete |
| `POST` | `api/Product/batch-delete` | Batch delete |
| `POST` | `api/Product/batch-update-status` | Batch status change |

### Orders

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `api/Order/paged?page=0&pageSize=8` | Paginated with filters |
| `POST` | `api/Order/create-order` | Create (CustomerEmail, Items) |
| `PUT` | `api/Order/update-order?id=X` | Update status |
| `POST` | `api/Order/batch-update-status` | Batch status change |

### Users

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `api/User/paged?page=0&pageSize=8` | Paginated |
| `POST` | `api/User/create-admin` | Create admin |
| `POST` | `api/User/create-student` | Create student |
| `PUT` | `api/User?id=X` | Update |
| `DELETE` | `api/User?id=X` | Delete |

### Notifications, Images, History

See full API documentation in the source controllers.

---

## Admin Panel

### Real-time Status

| Badge | Meaning |
|-------|---------|
| <span style="color:green">● Online</span> | SignalR connected — all changes appear instantly |
| <span style="color:red">● Offline</span> | SignalR disconnected — fallback polling every 5 min. Refresh page to reconnect |

### Features by Section

**Products:**
- Search with highlight, status filter, server-side pagination
- Inline quantity editing, status toggle (click badge)
- Photo upload with drag-and-drop style
- Batch operations: status change, delete (checkbox selection)
- Low stock warning (highlighted rows)

**Orders:**
- Email autocomplete, date range filter
- Expandable rows with product details and images
- Status dropdown in table, batch status change

**Users:**
- Role filter (Students/Admins)
- Balance editing via modal

**Notifications:**
- Per-branch: email, Telegram, low-stock threshold
- Admin-to-branch binding

**History:**
- Collapsible timeline
- Filter by action type and branch

### URL Filters

All search/filter state is persisted in URL query parameters (`ps`, `pf`, `os`, `of`, `us`, `uf`, `odf`, `odt`). Share links with specific filters applied.

---

## Architecture

### Clean Architecture

```
┌─────────────────────────────────────────────────────┐
│  Presentation                                        │
│  ├── API Controllers + SignalR Hub                   │
│  └── Blazor WASM Client (5 components)              │
├─────────────────────────────────────────────────────┤
│  Application                                         │
│  ├── Service Interfaces                              │
│  └── Service Implementations                         │
├─────────────────────────────────────────────────────┤
│  Contracts                                           │
│  ├── Request/Response DTOs                           │
│  └── Mappings (extension methods)                    │
├─────────────────────────────────────────────────────┤
│  Domain (no dependencies)                            │
│  ├── Entities + Enums                                │
│  └── Repository Interfaces                           │
├─────────────────────────────────────────────────────┤
│  Infrastructure                                      │
│  ├── EF Core + Fluent API                            │
│  └── Repository Implementations                      │
└─────────────────────────────────────────────────────┘
```

### SignalR Integration

| Component | Role |
|-----------|------|
| `AdminHub` | Empty hub — server pushes, clients listen |
| Controllers | `IHubContext<AdminHub>` → `SendAsync("Notify", entity, action)` after mutations |
| `SignalRService` | Client connection with auto-reconnect (0s, 2s, 5s, 10s) |
| `Admin.razor` | `HandleSignalRNotify` → reloads relevant data section |

Events: `product` (created/updated/deleted), `order` (created/updated), `user` (created/updated/deleted), `notification` (updated).

### Exception Handling

| Exception | HTTP Code |
|-----------|-----------|
| `ArgumentException` | 400 Bad Request |
| `KeyNotFoundException` | 404 Not Found |
| `InvalidOperationException` | 409 Conflict |
| Other | 500 Internal Server Error |

---

## Launcher

### 6 Startup Phases

| Phase | Description |
|-------|-------------|
| 1 | Check .NET SDK and Docker (auto-start Docker Desktop) |
| 2 | Create `docker-compose.yml` and connection string |
| 3 | Start SQL Server in Docker |
| 4 | Apply EF Core migrations |
| 5 | Start API |
| 6 | Start Blazor Client |

### Interactive Menu

```
╔══════════════════════════════════════════════════╗
║  MANAGEMENT MENU                                 ║
║  [1] Open admin panel                            ║
║  [2] Open main page                              ║
║  ──────────────────────────────────────────────── ║
║  [3] Restart API                                 ║
║  [4] Restart client                              ║
║  [5] Restart all                                 ║
║  ──────────────────────────────────────────────── ║
║  [6] Stop all                                    ║
║  ──────────────────────────────────────────────── ║
║  [7] Rebuild API (Docker)                        ║
║  [8] Clean Docker images                         ║
║  ──────────────────────────────────────────────── ║
║  [9] Show logs                                   ║
║  [0] Exit                                        ║
╚══════════════════════════════════════════════════╝
```

### CLI Flags

| Flag | Description |
|------|-------------|
| `--quick` | Skip completed phases |
| `--skip-docker` | Force local mode |
| `--port-api N` | Override API port |
| `--port-client N` | Override client port |
| `--reset` | Full reset: stop containers, clean images |

### Monitoring

- Background process check every 5 seconds
- Auto-restart on crash (up to 3 times)
- Log buffer with restart markers

---

## Ports

| Service | Protocol | Port | URL |
|---------|----------|------|-----|
| API | HTTP | 5000 | `http://localhost:5000` |
| Client | HTTP | 5001 | `http://localhost:5001` |
| SQL Server | TCP | 1433 | `localhost:1433` |

---

## Roadmap

### Done

- [x] CRUD for products, orders, users
- [x] Admin panel with search, filters, server pagination
- [x] Batch operations (status change, delete)
- [x] **SignalR real-time updates**
- [x] Photo upload and management
- [x] Action history with filtering
- [x] URL-persisted filters
- [x] Console launcher with auto-restart
- [x] Docker integration
- [x] Exception handler middleware
- [x] 30+ bugs fixed

### Planned

- [ ] Authentication (JWT)
- [ ] Password hashing
- [ ] DTO validation (FluentValidation)
- [ ] Unit tests (MSTest)
- [ ] Student-facing catalog page
- [ ] Shopping cart
- [ ] Real notifications (email/Telegram)
- [ ] Server-side sorting

---

<p align="center">
  Built for the <b>Cifra</b> course project
</p>
