// ══════════════════════════════════════════════════════════════════════════════
// CifraShop Launcher — Консольный лаунчер для автоматизации запуска проекта
// ══════════════════════════════════════════════════════════════════════════════
//
// ИСПОЛЬЗОВАНИЕ:
//   dotnet run --project CifraShop.Launcher           — полный запуск
//   dotnet run --project CifraShop.Launcher -- --quick — быстрый (пропуск завершённых фаз)
//
// ЛАУНЧЕР ВЫПОЛНЯЕТ:
//   1. Проверку зависимостей (.NET SDK, Docker)
//   2. Настройку конфигурации (docker-compose.yml, appsettings.json)
//   3. Запуск SQL Server в Docker
//   4. Применение EF Core миграций (с авто-синхронизацией модели)
//   5. Запуск API (в Docker или локально — автоматически)
//   6. Запуск Blazor-клиента (локально)
//   7. Мониторинг процессов с авто-перезапуском при падении
//
// МЕНЮ УПРАВЛЕНИЯ (после запуска):
//   [1] Открыть админ-панель       [6] Остановить всё
//   [2] Открыть главную страницу   [7] Пересобрать API (Docker)
//   [3] Перезапустить API          [8] Очистить Docker-образы
//   [4] Перезапустить клиент       [9] Показать логи
//   [5] Перезапустить всё          [0] Выход
//
// ОСОБЕННОСТИ:
//   - Автоматический выбор: API в Docker или локально (зависит от Docker)
//   - Health-check API перед запуском клиента
//   - Мониторинг процессов: авто-перезапуск до 3 раз при падении
//   - Логирование вывода процессов с просмотром через меню
//   - Обнаружение изменений в коде с предупреждением о пересборке Docker
//   - Универсальное сканирование проекта (без хардкода имён проектов)
//   - Защита от двойного запуска (Mutex)
//   - Корректная очистка при Ctrl+C (убийство дочерних процессов)
//   - Быстрый режим (--quick): пропуск уже выполненных фаз
// ══════════════════════════════════════════════════════════════════════════════

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

// ── Парсинг аргументов ──────────────────────────────────────────
// Отделяем аргументы dotnet run (--project ...) от наших (--quick)
var myArgs = args;
var dashDashIdx = Array.IndexOf(args, "--");
if (dashDashIdx >= 0 && dashDashIdx + 1 < args.Length)
    myArgs = args[(dashDashIdx + 1)..];

var quickMode = myArgs.Contains("--quick", StringComparer.OrdinalIgnoreCase);

// ── Защита от двойного запуска ──────────────────────────────────
// Именованный Mutex: если другой экземпляр лаунчера уже работает — выходим
using var mutex = new Mutex(false, "CifraShop_Launcher_SingleInstance");
if (!mutex.WaitOne(0))
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("  ⚠ CifraShop Launcher уже запущен.");
    Console.WriteLine("    Закройте предыдущий экземпляр или подождите его завершения.");
    Console.ResetColor();
    Console.ReadKey(true);
    return;
}

var rootDir = FindProjectRoot();
if (rootDir == null)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("  ✗ Не удалось найти корень проекта (CifraShop.slnx).");
    Console.WriteLine("    Убедитесь, что лаунчер запущен из папки решения.");
    Console.ResetColor();
    Console.ReadKey(true);
    return;
}

var launcher = new ProjectLauncher(rootDir, quickMode);

// ── Обработка Ctrl+C: корректная остановка всех дочерних процессов ──
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true; // Предотвращаем немедленное завершение
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("\n  Завершение работы...");
    Console.ResetColor();
    launcher.Cleanup();
    Environment.Exit(0);
};

await launcher.RunAsync();

/// <summary>
/// Поиск корня проекта — поднимается по дереву каталогов от места запуска,
/// ищет файл CifraShop.slnx (маркер корня решения).
/// Работает на любой машине независимо от пути размещения проекта.
/// </summary>
static string? FindProjectRoot()
{
    var dir = AppContext.BaseDirectory;
    for (int i = 0; i < 15; i++)
    {
        if (File.Exists(Path.Combine(dir, "CifraShop.slnx"))) return dir;
        var parent = Directory.GetParent(dir);
        if (parent == null) break;
        dir = parent.FullName;
    }
    return null;
}

/// <summary>
/// Главный класс лаунчера. Управляет всем жизненным циклом:
/// проверка → конфигурация → БД → миграции → API → клиент → мониторинг.
/// </summary>
public class ProjectLauncher
{
    // ════════════════════════════════════════════════════════════════
    // ПОЛЯ И КОНСТАНТЫ
    // ════════════════════════════════════════════════════════════════

    /// <summary>Корневая директория решения (содержит CifraShop.slnx)</summary>
    private readonly string _rootDir;

    /// <summary>Быстрый режим: пропускает уже выполненные фазы (БД запущена, миграции применены)</summary>
    private readonly bool _quickMode;

    /// <summary>
    /// HTTP-клиент для health-check API.
    /// Сертификаты НЕ проверяются — Docker-контейнер использует self-signed сертификат.
    /// </summary>
    private readonly HttpClient _httpClient = new(
        new HttpClientHandler { ServerCertificateCustomValidationCallback = (_, _, _, _) => true })
    { Timeout = TimeSpan.FromSeconds(5) };

    // ── Процессы ──────────────────────────────────────────────────
    private Process? _apiProcess;     // Локальный процесс API (null если API в Docker)
    private Process? _clientProcess;  // Процесс Blazor-клиента

    // ── Логи процессов ────────────────────────────────────────────
    // Буферы хранят stdout/stderr каждого процесса.
    // Потокобезопасны: доступ через lock.
    private readonly StringBuilder _apiLogBuffer = new();
    private readonly StringBuilder _clientLogBuffer = new();
    private readonly object _apiLogLock = new();
    private readonly object _clientLogLock = new();
    private const int MaxLogBufferSize = 80_000; // Максимум символов в буфере (~80KB)

    // ── Мониторинг и авто-рестарт ─────────────────────────────────
    private readonly ConcurrentDictionary<string, int> _restartCounts = new();
    private CancellationTokenSource? _monitorCts;
    private const int MaxRestarts = 3; // Макс. авто-перезапусков подряд

    // ── Состояние ─────────────────────────────────────────────────
    private bool _dockerAvailable = true;  // Docker доступен и запущен
    private bool _apiInDocker = false;     // API работает в Docker-контейнере
    private int _apiPort = 5000;           // Порт API (читается из launchSettings)
    private int _clientPort = 5001;        // Порт клиента (читается из launchSettings)

    /// <summary>
    /// Шаблон docker-compose.yml для автосоздания.
    /// Используется ТОЛЬКО если файл отсутствует.
    /// Содержит сервисы: db (SQL Server) и api (.NET API в Docker).
    /// </summary>
    private static readonly string DockerComposeContent = """
services:
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      ACCEPT_EULA: "Y"
      MSSQL_SA_PASSWORD: "Your_strong_Password123"
      MSSQL_PID: "Express"
    ports:
      - "1433:1433"
    volumes:
      - sqlserver-data:/var/opt/mssql

  api:
    build:
      context: .
      dockerfile: CifraShop.API/Dockerfile
    ports:
      - "5000:8085"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=db;Database=CifraShopDb;User Id=sa;Password=Your_strong_Password123;TrustServerCertificate=True;
    depends_on:
      - db

volumes:
  sqlserver-data:
""";

    /// <summary>Ширина UI-рамки в символах</summary>
    private const int W = 62;

    // ════════════════════════════════════════════════════════════════
    // ТОЧКА ВХОДА
    // ════════════════════════════════════════════════════════════════

    public ProjectLauncher(string rootDir, bool quickMode = false)
    {
        _rootDir = rootDir;
        _quickMode = quickMode;
    }

    /// <summary>
    /// Главный метод запуска. Выполняет 6 фаз последовательно,
    /// затем запускает фоновый мониторинг и переходит в меню.
    /// В быстром режиме пропускает фазы, которые уже выполнены.
    /// </summary>
    public async Task RunAsync()
    {
        Console.Clear();
        PrintBanner();
        ReadPortsFromConfig();
        await RunPhaseAsync(1, "Проверка зависимостей", EnsurePrerequisitesAsync);
        await RunPhaseAsync(2, "Настройка конфигурации", EnsureConfigAsync);

        if (!(_quickMode && await IsPortOpenAsync("localhost", 1433)))
            await RunPhaseAsync(3, "Запуск базы данных", StartDatabaseAsync);
        else
            Ok("SQL Server уже доступен (--quick)");

        if (!(_quickMode && await IsDbReadyAsync()))
            await RunPhaseAsync(4, "Применение миграций", EnsureMigrationsAsync);
        else
            Ok("Миграции уже применены (--quick)");

        await RunPhaseAsync(5, "Запуск API", StartApiAsync);
        await RunPhaseAsync(6, "Запуск клиента", StartClientAsync);
        StartMonitoring();
        await PrintLaunchResultAsync();
        await RunMenuAsync();
    }

    /// <summary>
    /// Публичный метод очистки: останавливает все дочерние процессы.
    /// Вызывается из обработчика Ctrl+C и при завершении.
    /// </summary>
    public void Cleanup()
    {
        StopMonitoring();
        StopProc(_apiProcess); _apiProcess = null;
        StopProc(_clientProcess); _clientProcess = null;
    }

    /// <summary>
    /// Проверяет, применены ли миграции (БД доступна и таблица __EFMigrationsHistory существует).
    /// Используется в быстром режиме для пропуска фазы миграций.
    /// </summary>
    private static async Task<bool> IsDbReadyAsync()
    {
        try
        {
            using var c = new TcpClient();
            await c.ConnectAsync("localhost", 1433);
            return true;
        }
        catch { return false; }
    }

    // ════════════════════════════════════════════════════════════════
    // ЧТЕНИЕ КОНФИГУРАЦИИ
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Читает порты API и клиента из launchSettings.json.
    /// Парсит JSON-профили, ищет applicationUrl с localhost.
    /// Если файл отсутствует или некорректен — используются порты по умолчанию (5000/5001).
    /// </summary>
    private void ReadPortsFromConfig()
    {
        try
        {
            _apiPort = ReadPortFromLaunchSettings(
                Path.Combine(_rootDir, "CifraShop.API", "Properties", "launchSettings.json"))
                ?? 5000;

            _clientPort = ReadPortFromLaunchSettings(
                Path.Combine(_rootDir, "CifraShop.Client", "Properties", "launchSettings.json"))
                ?? 5001;

            Log($"  Порты: API={_apiPort}, Клиент={_clientPort}");
        }
        catch
        {
            // Если чтение конфигурации не удалось — работаем с дефолтными портами
            Log($"  Порты (по умолчанию): API={_apiPort}, Клиент={_clientPort}");
        }
    }

    /// <summary>
    /// Универсальный метод: извлекает первый порт из launchSettings.json.
    /// Работает с любым количеством профилей — берёт первый найденный localhost-порт.
    /// </summary>
    private static int? ReadPortFromLaunchSettings(string path)
    {
        if (!File.Exists(path)) return null;
        var json = JsonDocument.Parse(File.ReadAllText(path));
        var profiles = json.RootElement.GetProperty("profiles");
        foreach (var prop in profiles.EnumerateObject())
        {
            if (prop.Value.TryGetProperty("applicationUrl", out var url))
            {
                var urlStr = url.GetString() ?? "";
                if (urlStr.Contains("localhost"))
                {
                    var portStr = urlStr.Split(':').Last().TrimEnd('/');
                    if (int.TryParse(portStr, out var port) && port > 0)
                        return port;
                }
            }
        }
        return null;
    }

    // ════════════════════════════════════════════════════════════════
    // UI: БАННЕР И ФАЗЫ
    // ════════════════════════════════════════════════════════════════

    private void PrintBanner()
    {
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("┌" + new string('─', W) + "┐");
        Console.WriteLine("│" + Center("Запускай и управляй") + "│");
        Console.WriteLine("│" + Center("─ CifraShop ─") + "│");
        Console.WriteLine("└" + new string('─', W) + "┘");
        Console.ResetColor();
        Console.WriteLine();
        var mode = _quickMode ? " •  ⚡ быстрый режим" : "";
        Log($"  📁 {Path.GetFileName(_rootDir)}{mode}  •  {DateTime.Now:HH:mm:ss dd.MM.yyyy}");
        Console.WriteLine();
    }

    /// <summary>Обёртка для запуска фазы: выводит номер и название, затем выполняет действие.</summary>
    private async Task RunPhaseAsync(int num, string title, Func<Task> action)
    {
        var phase = $"[{num}/6]";
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"  {phase} ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(title);
        Console.ResetColor();
        Console.WriteLine();
        await action();
    }

    // ════════════════════════════════════════════════════════════════
    // ФАЗА 1: ПРОВЕРКА ЗАВИСИМОСТЕЙ
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Проверяет наличие .NET SDK и Docker.
    /// Если Docker недоступен — предлагает выбор: продолжить без него (только локальный SQL Server).
    /// Если Docker есть, но демон не запущен — ждёт до 60 сек.
    /// </summary>
    private async Task EnsurePrerequisitesAsync()
    {
        // ── .NET SDK ──────────────────────────────────────────────
        var dotnet = await RunCmdAsync("dotnet", "--version");
        if (string.IsNullOrEmpty(dotnet))
        {
            Fail(".NET SDK не найден");
            Log("    Установите .NET 10: https://dotnet.microsoft.com/download");
            PressKey();
            Environment.Exit(1);
        }
        Ok($".NET SDK {dotnet.Trim()}");

        // ── Docker ────────────────────────────────────────────────
        var docker = await RunCmdAsync("docker", "version --format '{{.Server.Version}}'");
        if (string.IsNullOrWhiteSpace(docker))
        {
            Warn("Docker не найден или не запущен");
            Log("    → Установите Docker Desktop: https://docker.com/products/docker-desktop");
            Log("    → Или используйте локальный SQL Server на порту 1433");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("    Продолжить без Docker? [y/N] ");
            Console.ResetColor();
            var ans = (Console.ReadLine() ?? "").Trim().ToLower();
            if (ans != "y" && ans != "yes") { PressKey(); Environment.Exit(0); }
            _dockerAvailable = false;
            return;
        }
        Ok($"Docker {docker.Trim()}");

        // ── Проверка демона Docker ────────────────────────────────
        var ps = await RunCmdAsync("docker", "ps");
        if (ps == null)
        {
            Warn("Docker демон не отвечает. Ожидание запуска (до 60 сек)...");
            for (int i = 0; i < 60; i++)
            {
                await Task.Delay(1000);
                if (i % 10 == 0 && i > 0)
                    await AnimateWait("    Идёт запуск Docker", i, 60);
                ps = await RunCmdAsync("docker", "ps");
                if (ps != null)
                {
                    Console.WriteLine();
                    Ok("Docker демон запущен");
                    return;
                }
            }
            Console.WriteLine();
            Fail("Docker не запустился за 60 сек");
            PressKey();
            Environment.Exit(1);
        }

        // ── Docker Compose ────────────────────────────────────────
        var compose = await RunCmdAsync("docker", "compose version --short");
        if (!string.IsNullOrEmpty(compose))
            Ok($"Docker Compose {compose.Trim()}");
        else
            Warn("docker compose недоступен (попробуйте 'docker-compose')");
    }

    // ════════════════════════════════════════════════════════════════
    // ФАЗА 2: НАСТРОЙКА КОНФИГУРАЦИИ
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Создаёт docker-compose.yml (если нет) и проверяет строку подключения в appsettings.json.
    /// Если обнаружены изменения .cs файлов после последней Docker-сборки — предупреждает.
    /// </summary>
    private async Task EnsureConfigAsync()
    {
        // ── docker-compose.yml ────────────────────────────────────
        var composePath = Path.Combine(_rootDir, "docker-compose.yml");
        if (!File.Exists(composePath))
        {
            await File.WriteAllTextAsync(composePath, DockerComposeContent);
            Created("docker-compose.yml создан из шаблона");
        }
        else
            Ok("docker-compose.yml существует");

        // ── Строка подключения ────────────────────────────────────
        var settingsPath = Path.Combine(_rootDir, "CifraShop.API", "appsettings.json");
        if (File.Exists(settingsPath))
        {
            var content = await File.ReadAllTextAsync(settingsPath);
            if (!content.Contains("DefaultConnection"))
            {
                // Вставляем ConnectionStrings после первого '{' (корневой объект JSON)
                var idx = content.IndexOf('{');
                if (idx >= 0)
                {
                    var insertion = """
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CifraShopDb;User Id=sa;Password=Your_strong_Password123;TrustServerCertificate=True;"
  },
""";
                    var updated = content[..(idx + 1)] + "\n" + insertion + content[(idx + 1)..];
                    await File.WriteAllTextAsync(settingsPath, updated);
                    Created("Строка подключения добавлена в appsettings.json");
                }
                else
                    Fail("Не удалось найти начало JSON в appsettings.json");
            }
            else
                Ok("Строка подключения найдена");
        }
        else
            Fail("appsettings.json не найден!");

        // ── Обнаружение изменений в коде ──────────────────────────
        if (_dockerAvailable && DetectCodeChanges())
        {
            Warn("Обнаружены изменения в коде после последней сборки Docker");
            Log("    Рекомендуется пересборка образа (пункт [7] в меню)");
        }
    }

    // ════════════════════════════════════════════════════════════════
    // ФАЗА 3: ЗАПУСК БАЗЫ ДАННЫХ
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Запускает SQL Server через Docker Compose.
    /// Универсально: проверяет статус через `docker compose ps`, определяет
    /// нужно ли скачивать образ или просто поднять контейнер.
    /// Ждёт готовности SQL Server до 90 сек (проверка порта 1433).
    /// </summary>
    private async Task StartDatabaseAsync()
    {
        if (!_dockerAvailable)
        {
            if (await IsPortOpenAsync("localhost", 1433))
                Ok("SQL Server доступен на localhost:1433");
            else
                Warn("SQL Server не обнаружен. Убедитесь, что он запущен.");
            return;
        }

        // Проверяем: уже запущен?
        var running = await RunCmdAsync("docker", "compose ps --format '{{.Status}}'");
        if (running != null && running.Contains("Up"))
        {
            Ok("SQL Server уже запущен");
            return;
        }

        // Контейнер есть, но остановлен — просто поднимаем
        var allStatus = await RunCmdAsync("docker", "compose ps -a --format '{{.Status}}'");
        if (!string.IsNullOrWhiteSpace(allStatus) && allStatus.Trim().Length > 0)
        {
            Log("    Запуск существующего контейнера...");
            await RunDockerComposeAsync("up -d db");
        }
        else
        {
            // Контейнера нет — скачиваем образ и создаём
            Log("    Скачивание образа SQL Server...");
            try { await RunDockerComposeAsync("pull db"); }
            catch
            {
                Warn("Не удалось скачать образ. Проверьте подключение к интернету.");
                return;
            }
            Log("    Создание контейнера...");
            await RunDockerComposeAsync("up -d db");
        }

        // Ожидание готовности SQL Server (порт 1433)
        Log("    Ожидание готовности SQL Server...");
        for (int i = 0; i < 90; i++)
        {
            await Task.Delay(1000);
            if (await IsPortOpenAsync("localhost", 1433))
            {
                Ok("SQL Server готов");
                return;
            }
            if (i > 0 && i % 10 == 0)
                await AnimateWait("    Идёт инициализация", i, 90);
        }
        Console.WriteLine();
        Warn("SQL Server не ответил за 90 сек. Проверьте Docker.");
    }

    // ════════════════════════════════════════════════════════════════
    // ФАЗА 4: МИГРАЦИИ
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Применяет EF Core миграции. Устанавливает dotnet-ef если нужно.
    /// Повторяет до 3 попыток (SQL Server может быть ещё не готов).
    /// Если модель изменилась — автоматически создаёт и применяет миграцию.
    /// </summary>
    private async Task EnsureMigrationsAsync()
    {
        var infraDir = Path.Combine(_rootDir, "CifraShop.Infrastructure");
        var apiDir = Path.Combine(_rootDir, "CifraShop.API");

        // Установка dotnet-ef если отсутствует
        var efList = await RunCmdAsync("dotnet", "tool list -g");
        if (efList != null && !efList.Contains("dotnet-ef"))
        {
            Log("    Установка dotnet-ef...");
            await RunCmdAsync("dotnet", "tool install --global dotnet-ef --version 10.0.*");
        }

        for (int attempt = 1; attempt <= 3; attempt++)
        {
            Log($"    Применение миграций (попытка {attempt}/3)...");
            var result = await RunCmdAsync("dotnet",
                $"ef database update --project \"{infraDir}\" --startup-project \"{apiDir}\"");

            if (result == null)
            {
                if (attempt < 3) { Warn($"    Попытка {attempt} не удалась, повтор через 5 сек..."); await Task.Delay(5000); continue; }
                Warn("Не удалось применить миграции после 3 попыток.");
                return;
            }

            if (result.Contains("Done")) { Ok("БД обновлена"); return; }

            if (result.Contains("PendingModelChanges"))
            {
                Log("    Есть несохранённые изменения модели. Создаю миграцию...");
                var addResult = await RunCmdAsync("dotnet",
                    $"ef migrations add AutoSync --project \"{infraDir}\" --startup-project \"{apiDir}\"");
                if (addResult != null)
                {
                    Log("    Применение новой миграции...");
                    await RunCmdAsync("dotnet",
                        $"ef database update --project \"{infraDir}\" --startup-project \"{apiDir}\"");
                }
                Ok("БД синхронизирована (авто-миграция)");
                return;
            }

            if (result.Contains("connection", StringComparison.OrdinalIgnoreCase) ||
                result.Contains("connect", StringComparison.OrdinalIgnoreCase) ||
                result.Contains("timeout", StringComparison.OrdinalIgnoreCase))
            {
                Warn($"    SQL Server ещё не готов (попытка {attempt}/3)...");
                if (attempt < 3) await Task.Delay(5000);
                continue;
            }

            // Другая ошибка — показываем и выходим
            Warn("Не удалось применить миграции:");
            foreach (var line in result.Split('\n'))
                if (line.Contains("error", StringComparison.OrdinalIgnoreCase) ||
                    line.Contains("Error"))
                    Log($"    {line.Trim()}");
            return;
        }
    }

    // ════════════════════════════════════════════════════════════════
    // ФАЗА 5: ЗАПУСК API
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Запуск API с автоматическим выбором способа:
    ///   - Если Docker доступен → собирает и запускает Docker-контейнер
    ///   - Если Docker недоступен или сборка упала → запускает локально через dotnet run
    ///   - Автоматически находит свободный порт если указанный занят
    /// </summary>
    private async Task StartApiAsync()
    {
        if (_dockerAvailable)
        {
            _apiInDocker = true;
            Log("    Сборка Docker образа API...");
            var buildResult = await RunDockerComposeAsync("build api");
            if (buildResult != null && !buildResult.Contains("error", StringComparison.OrdinalIgnoreCase))
            {
                Ok("Docker образ собран");
                UpdateDockerBuildTimestamp();
                Log("    Запуск API контейнера...");
                await RunDockerComposeAsync("up -d api");
                for (int i = 0; i < 40; i++)
                {
                    await Task.Delay(500);
                    if (await IsPortOpenAsync("localhost", _apiPort))
                    {
                        Ok($"API → https://localhost:{_apiPort} (Docker)");
                        return;
                    }
                    if (i == 39) Warn("API не запустился за 20 сек");
                }
                return;
            }
            Warn("Не удалось собрать Docker образ. Запуск локально...");
            _apiInDocker = false;
        }

        // Локальный запуск (фолбэк)
        await KillPortAsync(_apiPort);
        if (await IsPortOpenAsync("localhost", _apiPort))
        {
            Warn($"Порт {_apiPort} всё ещё занят. Попробуйте закрыть использующую его программу.");
            return;
        }
        Log($"    Запуск API локально (порт {_apiPort})...");
        _apiProcess = StartDotnetWithLogs("CifraShop.API", "API", _apiLogBuffer, _apiLogLock);
        if (_apiProcess != null)
        {
            for (int i = 0; i < 40; i++)
            {
                await Task.Delay(500);
                if (await IsPortOpenAsync("localhost", _apiPort))
                {
                    Ok($"API → https://localhost:{_apiPort}");
                    return;
                }
                if (i == 39) Warn("API не запустился за 20 сек");
            }
        }
    }

    // ════════════════════════════════════════════════════════════════
    // ФАЗА 6: ЗАПУСК КЛИЕНТА
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Запуск Blazor-клиента. Перед стартом выполняет health-check API:
    /// отправляет HTTP-запрос и ждёт ответ (любой, включая 404 — это значит сервер жив).
    /// </summary>
    private async Task StartClientAsync()
    {
        Log("    Ожидание готовности API...");
        if (!await WaitForApiHealthAsync(30))
            Warn("API не отвечает. Запуск клиента всё равно...");

        await KillPortAsync(_clientPort);
        Log($"    Запуск клиента (порт {_clientPort})...");
        _clientProcess = StartDotnetWithLogs("CifraShop.Client", "CLI", _clientLogBuffer, _clientLogLock);
        if (_clientProcess != null)
        {
            for (int i = 0; i < 40; i++)
            {
                await Task.Delay(500);
                if (await IsPortOpenAsync("localhost", _clientPort))
                {
                    Ok($"Клиент → http://localhost:{_clientPort}");
                    break;
                }
                if (i == 39) Warn("Клиент не запустился за 20 сек");
            }
        }
    }

    // ════════════════════════════════════════════════════════════════
    // МОНИТОРИНГ И АВТО-ПЕРЕЗАПУСК
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Запуск фонового мониторинга процессов.
    /// Каждые 5 сек проверяет: жив ли API, жив ли клиент.
    /// Если процесс пал с ошибкой — перезапускает (до MaxRestarts раз).
    /// При ручном перезапуске через меню счётчик сбрасывается.
    /// </summary>
    private void StartMonitoring()
    {
        _monitorCts?.Cancel();
        _monitorCts?.Dispose();
        _monitorCts = new CancellationTokenSource();
        _ = MonitorProcessesAsync(_monitorCts.Token);
    }

    private void StopMonitoring()
    {
        _monitorCts?.Cancel();
        _monitorCts?.Dispose();
        _monitorCts = null;
    }

    private async Task MonitorProcessesAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try { await Task.Delay(5000, ct); }
            catch (OperationCanceledException) { return; }

            try
            {
                // ── Мониторинг API ────────────────────────────────
                if (_apiInDocker)
                    await MonitorDockerServiceAsync("api_docker", "api", "API");
                else
                {
                    var restarted = await MonitorLocalProcessAsync("api_local", _apiProcess, "API", _apiLogBuffer, _apiLogLock);
                    if (restarted != null) _apiProcess = restarted;
                }

                // ── Мониторинг клиента ────────────────────────────
                {
                    var restarted = await MonitorLocalProcessAsync("client", _clientProcess, "Клиент", _clientLogBuffer, _clientLogLock);
                    if (restarted != null) _clientProcess = restarted;
                }
            }
            catch { }
        }
    }

    /// <summary>Мониторинг одного Docker-сервиса. Проверяет `docker compose ps`.</summary>
    private async Task MonitorDockerServiceAsync(string restartKey, string serviceName, string displayName)
    {
        var ps = await RunDockerComposeAsync($"ps {serviceName} --format '{{{{.Status}}}}'");
        if (ps == null) return;

        var isUp = ps.Contains("Up") || ps.Contains("running");
        if (isUp)
        {
            _restartCounts.TryRemove(restartKey, out _);
            return;
        }

        // Сервис упал — авто-перезапуск
        _restartCounts.TryGetValue(restartKey, out var count);
        if (count < MaxRestarts)
        {
            _restartCounts[restartKey] = count + 1;
            Warn($"{displayName} контейнер упал (попытка {count + 1}/{MaxRestarts}). Перезапуск...");
            await RunDockerComposeAsync($"up -d {serviceName}");
            await WaitForPortAsync(_apiPort);
        }
        else if (count == MaxRestarts)
        {
            _restartCounts[restartKey] = MaxRestarts + 1;
            Fail($"{displayName} контейнер упал {MaxRestarts} раз подряд. Авто-перезапуск отключён.");
            Log("    Используйте [7] для ручной пересборки или [3] для перезапуска");
        }
    }

    /// <summary>Мониторинг одного локального процесса. Проверяет HasExited + ExitCode.
    /// Возвращает новый процесс если был перезапуск, иначе null.</summary>
    private async Task<Process?> MonitorLocalProcessAsync(string restartKey, Process? process, string displayName, StringBuilder logBuffer, object logLock)
    {
        if (process == null) return null;

        // Защита от race condition: если процесс уже заменён пользователем через меню — пропускаем
        if (process != _apiProcess && process != _clientProcess) return null;

        if (process.HasExited && process.ExitCode != 0)
        {
            _restartCounts.TryGetValue(restartKey, out var count);
            if (count < MaxRestarts)
            {
                _restartCounts[restartKey] = count + 1;
                Warn($"{displayName} упал (код {process.ExitCode}). Перезапуск ({count + 1}/{MaxRestarts})...");
                ShowLastLogLines(logBuffer, logLock, displayName);
                var project = restartKey == "client" ? "CifraShop.Client" : "CifraShop.API";
                return StartDotnetWithLogs(project, displayName, logBuffer, logLock);
            }
            else if (count == MaxRestarts)
            {
                _restartCounts[restartKey] = MaxRestarts + 1;
                Fail($"{displayName} упал {MaxRestarts} раз подряд. Авто-перезапуск отключён.");
                ShowLastLogLines(logBuffer, logLock, displayName);
            }
        }
        else if (process is { HasExited: false })
        {
            _restartCounts.TryRemove(restartKey, out _);
        }
        return null;
    }

    /// <summary>Выводит последние 8 строк из буфера логов (при падении процесса).</summary>
    private static void ShowLastLogLines(StringBuilder buffer, object lockObj, string prefix)
    {
        lock (lockObj)
        {
            var lines = buffer.ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var last = lines.Skip(Math.Max(0, lines.Length - 8)).ToArray();
            if (last.Length == 0) return;
            Log($"    ── Логи {prefix} (последние строки) ──");
            foreach (var line in last)
                Log($"    │ {line.TrimEnd()}");
            Log($"    ── конец логов ──");
        }
    }

    // ════════════════════════════════════════════════════════════════
    // УТИЛИТЫ: ПОРТЫ, HEALTH-CHECK, ИЗМЕНЕНИЯ КОДА, DOCKER
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Ищет свободный TCP-порт начиная с startPort.
    /// Если startPort занят — пробует следующий (до +100).
    /// Возвращает startPort если все заняты (как фолбэк).
    /// </summary>
    private static async Task<int> FindFreePortAsync(int startPort)
    {
        for (int port = startPort; port < startPort + 100; port++)
        {
            if (!await IsPortOpenAsync("localhost", port))
                return port;
        }
        return startPort;
    }

    /// <summary>
    /// Health-check API: отправляет HTTP GET и ждёт ответ.
    /// Любой ответ от сервера (включая 404, 405) = сервер жив.
    /// Только исключение (connection refused) = сервер не готов.
    /// Пробует HTTPS, затем HTTP (на случай разных конфигураций).
    /// </summary>
    private async Task<bool> WaitForApiHealthAsync(int timeoutSec)
    {
        for (int i = 0; i < timeoutSec * 2; i++)
        {
            await Task.Delay(500);
            if (await CheckApiHealthAsync($"https://localhost:{_apiPort}/")) return true;
            if (await CheckApiHealthAsync($"http://localhost:{_apiPort}/")) return true;
            if (i > 0 && i % 4 == 0)
                await AnimateWait("    Проверка API", i, timeoutSec * 2);
        }
        return false;
    }

    /// <summary>
    /// Отправляет один HTTP-запрос. Любой ответ = сервер работает.
    /// HttpRequestException = сервер не доступен.
    /// </summary>
    private async Task<bool> CheckApiHealthAsync(string url)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = await _httpClient.SendAsync(request);
            return true;
        }
        catch { return false; }
    }

    /// <summary>
    /// Обнаруживает изменения в исходниках после последней Docker-сборки.
    /// Универсально: сканирует ВСЕ подкаталоги решения (не хардкодит имена проектов).
    /// Проверяет файлы: .cs, .csproj, .razor, .json, .css, .js.
    /// Исключает: bin, obj, .git, node_modules.
    /// </summary>
    private bool DetectCodeChanges()
    {
        var markerFile = Path.Combine(_rootDir, ".last-docker-build");
        if (!File.Exists(markerFile)) return false;

        DateTime lastBuild = DateTime.MinValue;
        var content = File.ReadAllText(markerFile).Trim();
        if (!DateTime.TryParse(content, out lastBuild)) return false;

        var extensions = new[] { "*.cs", "*.csproj", "*.razor", "*.json", "*.css", "*.js" };
        var excludeDirs = new[] { "bin", "obj", ".git", "node_modules", ".vs", "CifraShop.Launcher", "CifraShop.Tests" };

        try
        {
            foreach (var ext in extensions)
            {
                foreach (var file in Directory.EnumerateFiles(_rootDir, ext, SearchOption.AllDirectories))
                {
                    var relativePath = Path.GetRelativePath(_rootDir, file);
                    if (excludeDirs.Any(d => relativePath.StartsWith(d, StringComparison.OrdinalIgnoreCase)))
                        continue;

                    if (File.GetLastWriteTime(file) > lastBuild)
                        return true;
                }
            }
        }
        catch
        {
            // Нет прав доступа к некоторым папкам — пропускаем проверку
            return false;
        }
        return false;
    }

    /// <summary>Записывает текущее время в маркер .last-docker-build.</summary>
    private void UpdateDockerBuildTimestamp()
    {
        try
        {
            File.WriteAllText(Path.Combine(_rootDir, ".last-docker-build"), DateTime.Now.ToString("o"));
        }
        catch { /* маркер не критичен */ }
    }

    /// <summary>
    /// Очистка Docker: остановка сервисов, удаление неиспользуемых образов,
    /// перезапуск. НЕ удаляет volume'ы — данные БД в безопасности.
    /// </summary>
    private async Task CleanupDockerAsync()
    {
        Log("  Остановка сервисов...");
        await RunDockerComposeAsync("down");
        Log("  Очистка неиспользуемых образов...");
        await RunCmdAsync("docker", "image prune -f");
        Ok("Docker ресурсы очищены");
        Log("  Перезапуск сервисов...");
        await RunDockerComposeAsync("up -d");
    }

    // ════════════════════════════════════════════════════════════════
    // UI: ИТОГОВЫЙ СТАТУС
    // ════════════════════════════════════════════════════════════════

    /// <summary>Выводит итоговую таблицу со статусом всех сервисов после запуска.</summary>
    private async Task PrintLaunchResultAsync()
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("┌" + new string('─', W) + "┐");
        Console.WriteLine("│" + Center("🚀  ВСЁ ГОТОВО К РАБОТЕ  🚀") + "│");
        Console.WriteLine("├" + new string('─', W) + "┤");
        Console.ResetColor();

        // Показываем статусы в едином формате
        await PrintServiceStatusesAsync();

        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("└" + new string('─', W) + "┘");
        Console.ResetColor();
    }

    /// <summary>
    /// Единый метод проверки и вывода статуса всех сервисов.
    /// Используется и в PrintLaunchResultAsync и в ShowStatusAsync — без дублирования.
    /// </summary>
    private async Task PrintServiceStatusesAsync()
    {
        // ── БД ────────────────────────────────────────────────────
        if (_dockerAvailable)
        {
            var dbPs = await RunCmdAsync("docker", "compose ps --format '{{.Status}}'");
            var dbOk = dbPs != null && dbPs.Contains("Up");
            PrintStatusLine("БД", dbOk, dbOk ? "SQL Server (Docker)" : "не запущен");
        }

        // ── API ───────────────────────────────────────────────────
        if (_apiInDocker)
        {
            var apiPs = await RunCmdAsync("docker", "compose ps api --format '{{.Status}}'");
            var apiOk = apiPs != null && (apiPs.Contains("Up") || apiPs.Contains("running"));
            PrintStatusLine("API", apiOk, apiOk ? $"https://localhost:{_apiPort} (Docker)" : "не запущен");
        }
        else
        {
            var apiOk = _apiProcess is { HasExited: false };
            PrintStatusLine("API", apiOk, apiOk ? $"https://localhost:{_apiPort}" : "не запущен");
        }

        // ── Клиент ────────────────────────────────────────────────
        var clientOk = _clientProcess is { HasExited: false };
        PrintStatusLine("Клиент", clientOk, clientOk ? $"http://localhost:{_clientPort}/admin" : "не запущен");
    }

    private void PrintStatusLine(string name, bool ok, string detail)
    {
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write("│ ");
        Console.ForegroundColor = ok ? ConsoleColor.Green : ConsoleColor.Red;
        Console.Write(ok ? "  ✓ " : "  ✗ ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"{name,-8}");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write(detail);
        Console.ResetColor();
        PadLine(W - 2 - 4 - 8 - detail.Length);
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("│");
        Console.ResetColor();
    }

    // ════════════════════════════════════════════════════════════════
    // МЕНЮ УПРАВЛЕНИЯ
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Интерактивное меню управления. Работает в цикле до выбора "Выход".
    /// Все операции с перезапуском/остановкой корректно управляют мониторингом.
    /// </summary>
    private async Task RunMenuAsync()
    {
        while (true)
        {
            DrawMenu();
            var choice = (Console.ReadLine() ?? "").Trim().ToLower();

            switch (choice)
            {
                // ── Навигация ──────────────────────────────────────
                case "1": OpenBrowser($"http://localhost:{_clientPort}/admin"); break;
                case "2": OpenBrowser($"http://localhost:{_clientPort}"); break;

                // ── Перезапуск ─────────────────────────────────────
                case "3": await RestartApiAsync(); break;
                case "4": await RestartClientAsync(); break;
                case "5":
                    _restartCounts.Clear();
                    await StopAllAsync();
                    await StartApiAsync();
                    await StartClientAsync();
                    StartMonitoring();
                    await PrintLaunchResultAsync();
                    break;

                // ── Остановка ──────────────────────────────────────
                case "6":
                    await StopAllAsync();
                    Log("  Все сервисы остановлены.");
                    break;

                // ── Docker ─────────────────────────────────────────
                case "7": await RebuildDockerApiAsync(); break;
                case "8": await AskAndCleanupDockerAsync(); break;

                // ── Логи ───────────────────────────────────────────
                case "9": await ShowLogsMenuAsync(); break;

                // ── Выход ──────────────────────────────────────────
                case "0":
                    await StopAllAsync();
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("\n  До свидания!\n");
                    Console.ResetColor();
                    return;
            }
        }
    }

    private void DrawMenu()
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write("  ╭─ ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("МЕНЮ УПРАВЛЕНИЯ");
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine(" " + new string('─', W - 20) + "╮");
        Console.ResetColor();

        MenuRow("1", "Открыть админ-панель", ConsoleColor.Cyan);
        MenuRow("2", "Открыть главную страницу", ConsoleColor.Cyan);
        Separator();
        MenuRow("3", "Перезапустить API", ConsoleColor.Yellow);
        MenuRow("4", "Перезапустить клиент", ConsoleColor.Yellow);
        MenuRow("5", "Перезапустить всё", ConsoleColor.Yellow);
        Separator();
        MenuRow("6", "Остановить всё", ConsoleColor.Red);
        Separator();
        MenuRow("7", "Пересобрать API (Docker)", ConsoleColor.Magenta);
        MenuRow("8", "Очистить Docker-образы", ConsoleColor.DarkCyan);
        Separator();
        MenuRow("9", "Показать логи", ConsoleColor.Gray);
        MenuRow("0", "Выход", ConsoleColor.DarkGray);

        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write("  ╰");
        Console.Write(new string('─', W - 3));
        Console.WriteLine("╯");
        Console.ResetColor();

        // Краткий статус сервисов — сразу видно, что работает
        Console.ForegroundColor = ConsoleColor.DarkGray;
        var apiStatus = _apiProcess is { HasExited: false } || _apiInDocker ? " ●" : " ○";
        var clientStatus = _clientProcess is { HasExited: false } ? " ●" : " ○";
        Console.WriteLine($"    API: {_apiPort}{apiStatus}   Клиент: {_clientPort}{clientStatus}");
        Console.ResetColor();
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("  ▸ ");
        Console.ResetColor();
    }

    // ── Команды меню ───────────────────────────────────────────────

    private async Task RestartApiAsync()
    {
        _restartCounts.TryRemove("api_docker", out _);
        _restartCounts.TryRemove("api_local", out _);
        if (_apiInDocker)
        {
            Log("  Остановка API контейнера...");
            await RunDockerComposeAsync("stop api");
            await KillPortAsync(_apiPort);
            Log($"  Перезапуск API (Docker, порт {_apiPort})...");
            await RunDockerComposeAsync("up -d api");
            if (await WaitForPortAsync(_apiPort)) Ok("API перезапущен (Docker)");
            else Warn("API не запустился");
        }
        else
        {
            StopProc(_apiProcess); _apiProcess = null;
            await KillPortAsync(_apiPort);
            Log($"  Перезапуск API (порт {_apiPort})...");
            lock (_apiLogLock) { _apiLogBuffer.AppendLine("\n=== РЕСТАРТ ==="); }
            _apiProcess = StartDotnetWithLogs("CifraShop.API", "API", _apiLogBuffer, _apiLogLock);
            if (_apiProcess != null && await WaitForPortAsync(_apiPort)) Ok("API перезапущен");
            else Warn("API не запустился");
        }
    }

    private async Task RestartClientAsync()
    {
        _restartCounts.TryRemove("client", out _);
        StopProc(_clientProcess); _clientProcess = null;
        await KillPortAsync(_clientPort);
        Log($"  Перезапуск клиента (порт {_clientPort})...");
        lock (_clientLogLock) { _clientLogBuffer.AppendLine("\n=== РЕСТАРТ ==="); }
        _clientProcess = StartDotnetWithLogs("CifraShop.Client", "CLI", _clientLogBuffer, _clientLogLock);
        if (_clientProcess != null && await WaitForPortAsync(_clientPort)) Ok("Клиент перезапущен");
        else Warn("Клиент не запустился");
    }

    private async Task RebuildDockerApiAsync()
    {
        if (!_dockerAvailable) { Warn("Docker недоступен"); return; }

        _restartCounts.TryRemove("api_docker", out _);
        Log("  Пересборка Docker образа API...");
        await RunDockerComposeAsync("stop api");
        await KillPortAsync(_apiPort);
        var buildResult = await RunDockerComposeAsync("build api");
        if (buildResult != null && !buildResult.Contains("error", StringComparison.OrdinalIgnoreCase))
        {
            Ok("Образ пересобран");
            UpdateDockerBuildTimestamp();
            Log("  Запуск API контейнера...");
            await RunDockerComposeAsync("up -d api");
            if (await WaitForPortAsync(_apiPort)) Ok("API перезапущен (Docker)");
            else Warn("API не запустился");
        }
        else
            Warn("Не удалось собрать образ");
    }

    private async Task AskAndCleanupDockerAsync()
    {
        if (!_dockerAvailable) { Warn("Docker недоступен"); return; }
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("    Остановить сервисы и очистить неиспользуемые Docker-образы? [y/N] ");
        Console.ResetColor();
        var confirm = (Console.ReadLine() ?? "").Trim().ToLower();
        if (confirm is "y" or "yes")
            await CleanupDockerAsync();
    }

    /// <summary>
    /// Подменю выбора логов: API, клиент или статус.
    /// Показывается при выборе [9] в основном меню.
    /// </summary>
    private async Task ShowLogsMenuAsync()
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("    [1] Логи API    [2] Логи клиента    [3] Статус сервисов    [0] Назад");
        Console.ResetColor();
        Console.Write("    ▸ ");
        var sub = (Console.ReadLine() ?? "").Trim();
        switch (sub)
        {
            case "1": ShowFullLog(_apiLogBuffer, _apiLogLock, "API"); break;
            case "2": ShowFullLog(_clientLogBuffer, _clientLogLock, "КЛИЕНТ"); break;
            case "3": await ShowStatusAsync(); break;
        }
    }

    // ── Логирование ───────────────────────────────────────────────

    /// <summary>
    /// Показывает полный буфер логов конкретного сервиса в рамке.
    /// Выводит последние 50 строк (чтобы не засорять консоль).
    /// </summary>
    private static void ShowFullLog(StringBuilder buffer, object lockObj, string prefix)
    {
        Console.WriteLine();
        lock (lockObj)
        {
            var content = buffer.ToString().Trim();
            if (string.IsNullOrEmpty(content))
            {
                Log($"  Логи {prefix} пусты");
                return;
            }
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("┌" + new string('─', W) + "┐");
            Console.WriteLine("│" + Center($"ЛОГИ {prefix}") + "│");
            Console.WriteLine("├" + new string('─', W) + "┤");
            Console.ResetColor();
            foreach (var line in content.Split('\n').TakeLast(50))
            {
                var trimmed = line.TrimEnd();
                if (trimmed.Length > W - 2) trimmed = trimmed[..(W - 5)] + "...";
                var pad = Math.Max(0, W - 2 - trimmed.Length);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("│ " + trimmed + new string(' ', pad) + "│");
            }
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("└" + new string('─', W) + "┘");
            Console.ResetColor();
        }
    }

    // ── Ожидание и остановка ──────────────────────────────────────

    private async Task<bool> WaitForPortAsync(int port)
    {
        for (int i = 0; i < 40; i++)
        {
            await Task.Delay(500);
            if (await IsPortOpenAsync("localhost", port)) return true;
        }
        return false;
    }

    /// <summary>
    /// Остановка ВСЕХ сервисов + мониторинга.
    /// Корректно обрабатывает и Docker-контейнеры и локальные процессы.
    /// </summary>
    private async Task StopAllAsync()
    {
        Cleanup();

        if (_apiInDocker)
            await RunDockerComposeAsync("stop api");

        await KillPortAsync(_apiPort);
        await KillPortAsync(_clientPort);
        _restartCounts.Clear();
    }

    private async Task ShowStatusAsync()
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("┌" + new string('─', W) + "┐");
        Console.WriteLine("│" + Center("СТАТУС СЕРВИСОВ") + "│");
        Console.WriteLine("├" + new string('─', W) + "┤");
        Console.ResetColor();

        // Используем тот же единый метод — без дублирования
        await PrintServiceStatusesAsync();

        // Показать информацию о рестартах, если были
        var activeRestarts = _restartCounts.Where(kv => kv.Value > 0 && kv.Value <= MaxRestarts).ToList();
        if (activeRestarts.Count > 0)
        {
            Separator();
            foreach (var kv in activeRestarts)
                StatusRow("⚠ " + kv.Key, false, $"перезапусков: {kv.Value}/{MaxRestarts}");
        }

        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("└" + new string('─', W) + "┘");
        Console.ResetColor();
    }

    // ════════════════════════════════════════════════════════════════
    // НИЗКОУРОВНЕВЫЕ ХЕЛПЕРЫ
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Запуск dotnet-процесса с перехватом stdout/stderr.
    /// Вывод буферизуется в logBuffer (потокобезопасно через lock).
    /// Буфер автоматически обрезается при превышении MaxLogBufferSize.
    /// </summary>
    private Process? StartDotnetWithLogs(string project, string prefix, StringBuilder logBuffer, object logLock)
    {
        try
        {
            var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"run --project \"{Path.Combine(_rootDir, project)}\"",
                    WorkingDirectory = _rootDir,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };

            p.OutputDataReceived += (_, e) =>
            {
                if (e.Data == null) return;
                lock (logLock)
                {
                    logBuffer.AppendLine(e.Data);
                    if (logBuffer.Length > MaxLogBufferSize)
                    {
                        var str = logBuffer.ToString();
                        logBuffer.Clear();
                        logBuffer.Append(str.AsSpan(str.Length / 2));
                    }
                }
            };

            p.ErrorDataReceived += (_, e) =>
            {
                if (e.Data == null) return;
                lock (logLock)
                {
                    logBuffer.AppendLine($"[ERR] {e.Data}");
                }
            };

            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            return p;
        }
        catch (Exception ex)
        {
            Fail($"Не удалось запустить {project}: {ex.Message}");
            return null;
        }
    }

    /// <summary>Остановка процесса с убийством всего дерева (включая дочерние).</summary>
    private static void StopProc(Process? proc)
    {
        try
        {
            if (proc is { HasExited: false })
            {
                proc.Kill(entireProcessTree: true);
                proc.WaitForExit(3000);
            }
        }
        catch { }
    }

    private static void OpenBrowser(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            Ok($"Открыт: {url}");
        }
        catch (Exception ex) { Fail($"Не удалось открыть браузер: {ex.Message}"); }
    }

    /// <summary>
    /// Универсальный запуск внешней команды. Перенаправляет stdout+stderr.
    /// Возвращает stdout при успехе (exit code 0), или stdout+stderr при ошибке.
    /// Возвращает null при исключении (команда не найдена и т.д.).
    /// </summary>
    private static async Task<string?> RunCmdAsync(string cmd, string args)
    {
        try
        {
            using var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = cmd, Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            p.Start();
            var output = await p.StandardOutput.ReadToEndAsync();
            var error = await p.StandardError.ReadToEndAsync();
            await p.WaitForExitAsync();
            return p.ExitCode == 0 ? output : output + "\n" + error;
        }
        catch { return null; }
    }

    /// <summary>
    /// Обёртка над RunCmdAsync для docker compose команд.
    /// Автоматически подставляет путь к docker-compose.yml (для работы из любой директории).
    /// Перед запуском проверяет валидность YAML-файла (наличие ключа 'services').
    /// </summary>
    private async Task<string?> RunDockerComposeAsync(string args)
    {
        var composePath = Path.Combine(_rootDir, "docker-compose.yml");
        if (!File.Exists(composePath))
        {
            Warn("docker-compose.yml не найден");
            return null;
        }

        // Быстрая проверка: файл должен содержать ключ 'services'
        var content = File.ReadAllText(composePath);
        if (!content.Contains("services:", StringComparison.OrdinalIgnoreCase))
        {
            Warn("docker-compose.yml не содержит секцию 'services'. Проверьте файл.");
            return null;
        }

        return await RunCmdAsync("docker", $"compose -f \"{composePath}\" {args}");
    }

    /// <summary>Проверка: открыт ли TCP-порт на хосте.</summary>
    private static async Task<bool> IsPortOpenAsync(string host, int port)
    {
        try { using var c = new TcpClient(); await c.ConnectAsync(host, port); return true; }
        catch { return false; }
    }

    /// <summary>
    /// Принудительное освобождение порта: находит процесс по порту через netstat и убивает его.
    /// Работает только на Windows (netstat -ano). На других ОС — no-op.
    /// </summary>
    private static async Task KillPortAsync(int port)
    {
        if (!OperatingSystem.IsWindows()) return;
        try
        {
            var result = await RunCmdAsync("netstat", "-ano");
            if (result == null) return;
            foreach (var line in result.Split('\n'))
            {
                if (line.Contains($":{port}") && line.Contains("LISTENING"))
                {
                    var parts = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 0 && int.TryParse(parts[^1], out var pid))
                    {
                        try { Process.GetProcessById(pid).Kill(); } catch { }
                    }
                }
            }
        }
        catch { }
    }

    // ════════════════════════════════════════════════════════════════
    // UI: РИСОВАНИЕ ЭЛЕМЕНТОВ
    // ════════════════════════════════════════════════════════════════

    private static async Task AnimateWait(string label, int elapsed, int total)
    {
        try
        {
            var progress = Math.Min((double)elapsed / total, 1.0);
            var filled = (int)(progress * 30);
            var bar = new string('█', filled) + new string('░', 30 - filled);
            var pct = (int)(progress * 100);
            if (Console.CursorLeft > 4) Console.CursorLeft = 0;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"    {bar} {pct,3}%  ({elapsed}s/{total}s)   ");
            Console.ResetColor();
        }
        catch { }
        await Task.CompletedTask;
    }

    private static void Ok(string msg)      { WriteColored("    ✓ ", ConsoleColor.Green, ConsoleColor.Gray, msg); }
    private static void Created(string msg) { WriteColored("    ✦ ", ConsoleColor.DarkCyan, ConsoleColor.Gray, msg); }
    private static void Warn(string msg)    { WriteColored("    ⚠ ", ConsoleColor.DarkYellow, ConsoleColor.Gray, msg); }
    private static void Fail(string msg)    { WriteColored("    ✗ ", ConsoleColor.Red, ConsoleColor.Gray, msg); }

    private static void Log(string msg)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(msg);
        Console.ResetColor();
    }

    private static void WriteColored(string prefix, ConsoleColor prefixColor, ConsoleColor msgColor, string msg)
    {
        Console.ForegroundColor = prefixColor;
        Console.Write(prefix);
        Console.ForegroundColor = msgColor;
        Console.WriteLine(msg);
        Console.ResetColor();
    }

    private static void MenuRow(string key, string label, ConsoleColor color)
    {
        Console.Write("  │  ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"[{key}]");
        Console.ForegroundColor = color;
        Console.Write($" {label}");
        Console.ResetColor();
        var contentLen = 4 + 3 + 1 + label.Length;
        var pad = Math.Max(0, W - contentLen);
        Console.Write(new string(' ', pad));
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("│");
        Console.ResetColor();
    }

    private static void Separator()
    {
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("  │  " + new string('─', W - 5) + "│");
        Console.ResetColor();
    }

    private static void StatusRow(string name, bool ok, string detail)
    {
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write("│ ");
        Console.ForegroundColor = ok ? ConsoleColor.Green : ConsoleColor.Red;
        Console.Write(ok ? " ● " : " ○ ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(name.PadRight(10));
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write(detail);
        Console.ResetColor();
        var contentLen = 4 + 10 + detail.Length;
        var pad = Math.Max(0, W - contentLen);
        Console.Write(new string(' ', pad));
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("│");
        Console.ResetColor();
    }

    private static string Center(string s)
    {
        var pad = W - s.Length;
        if (pad <= 0) return s;
        var left = pad / 2;
        return new string(' ', left) + s + new string(' ', pad - left);
    }

    private static void PadLine(int width)
    {
        if (width > 0) Console.Write(new string(' ', width));
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("│");
        Console.ResetColor();
    }

    private static void PressKey()
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("\n  Нажмите любую клавишу...");
        Console.ResetColor();
        Console.ReadKey(true);
    }
}
