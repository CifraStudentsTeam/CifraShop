using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

var rootDir = FindProjectRoot();
if (rootDir == null)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("  ✗ Не удалось найти корень проекта (CifraShop.slnx).");
    Console.ResetColor();
    Console.ReadKey(true);
    return;
}

var launcher = new ProjectLauncher(rootDir);
await launcher.RunAsync();

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

public class ProjectLauncher
{
    private readonly string _rootDir;
    private Process? _apiProcess;
    private Process? _clientProcess;
    private bool _dockerAvailable = true;
    private int _apiPort = 5000;
    private int _clientPort = 5001;

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
volumes:
  sqlserver-data:
""";

    private const int W = 62;

    public ProjectLauncher(string rootDir) => _rootDir = rootDir;

    public async Task RunAsync()
    {
        Console.Clear();
        PrintBanner();
        ReadPortsFromConfig();
        await RunPhaseAsync(1, "Проверка зависимостей", EnsurePrerequisitesAsync);
        await RunPhaseAsync(2, "Настройка конфигурации", EnsureConfigAsync);
        await RunPhaseAsync(3, "Запуск базы данных", StartDatabaseAsync);
        await RunPhaseAsync(4, "Применение миграций", EnsureMigrationsAsync);
        await RunPhaseAsync(5, "Запуск серверов", StartServicesAsync);
        await PrintLaunchResultAsync();
        await RunMenuAsync();
    }

    private void ReadPortsFromConfig()
    {
        try
        {
            var apiSettings = Path.Combine(_rootDir, "CifraShop.API", "Properties", "launchSettings.json");
            if (File.Exists(apiSettings))
            {
                var json = JsonDocument.Parse(File.ReadAllText(apiSettings));
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
                            {
                                _apiPort = port;
                                break;
                            }
                        }
                    }
                }
            }

            var clientSettings = Path.Combine(_rootDir, "CifraShop.Client", "Properties", "launchSettings.json");
            if (File.Exists(clientSettings))
            {
                var json = JsonDocument.Parse(File.ReadAllText(clientSettings));
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
                            {
                                _clientPort = port;
                                break;
                            }
                        }
                    }
                }
            }

            Log($"  Порты: API={_apiPort}, Клиент={_clientPort}");
        }
        catch { }
    }

    private void PrintBanner()
    {
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("┌" + new string('─', W) + "┐");
        Console.WriteLine("│" + Center("Запускай и управляй") + "│");
        Console.WriteLine("│" + Center("─ CifraShop ─") + "│");
        Console.WriteLine("└" + new string('─', W) + "┘");
        Console.ResetColor();
        Console.WriteLine();
        Log($"  📁 {Path.GetFileName(_rootDir)}  •  {DateTime.Now:HH:mm:ss dd.MM.yyyy}");
        Console.WriteLine();
    }

    private async Task RunPhaseAsync(int num, string title, Func<Task> action)
    {
        var phase = $"[{num}/5]";
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"  {phase} ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(title);
        Console.ResetColor();
        Console.WriteLine();
        await action();
    }

    // ─── Phase 1: Prerequisites ───────────────────────────────────

    private async Task EnsurePrerequisitesAsync()
    {
        var dotnet = await RunCmdAsync("dotnet", "--version");
        if (string.IsNullOrEmpty(dotnet))
        {
            Fail(".NET SDK не найден");
            Log("    Установите .NET 10: https://dotnet.microsoft.com/download");
            PressKey();
            Environment.Exit(1);
        }
        Ok($".NET SDK {dotnet.Trim()}");

        var docker = await RunCmdAsync("docker", "version --format '{{.Server.Version}}'");
        if (string.IsNullOrEmpty(docker) || string.IsNullOrWhiteSpace(docker))
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

        var ps = await RunCmdAsync("docker", "ps");
        if (ps == null)
        {
            Warn("Docker демон не отвечает");
            Log("    Ожидание запуска (до 60 сек)...");
            for (int i = 0; i < 60; i++)
            {
                await Task.Delay(1000);
                if (i % 10 == 0 && i > 0) await AnimateWait("    Идёт запуск Docker", i, 60);
                ps = await RunCmdAsync("docker", "ps");
                if (ps != null) { Console.WriteLine(); Ok("Docker демон запущен"); return; }
            }
            Console.WriteLine();
            Fail("Docker не запустился за 60 сек");
            PressKey();
            Environment.Exit(1);
        }

        var compose = await RunCmdAsync("docker", "compose version --short");
        if (!string.IsNullOrEmpty(compose))
            Ok($"Docker Compose {compose.Trim()}");
        else
            Warn("docker compose недоступен (попробуйте 'docker-compose')");
    }

    // ─── Phase 2: Config ──────────────────────────────────────────

    private async Task EnsureConfigAsync()
    {
        // docker-compose.yml
        var composePath = Path.Combine(_rootDir, "docker-compose.yml");
        if (!File.Exists(composePath))
        {
            await File.WriteAllTextAsync(composePath, DockerComposeContent);
            Created("docker-compose.yml создан");
        }
        else
            Ok("docker-compose.yml существует");

        // appsettings.json connection string
        var settingsPath = Path.Combine(_rootDir, "CifraShop.API", "appsettings.json");
        if (File.Exists(settingsPath))
        {
            var content = await File.ReadAllTextAsync(settingsPath);
            if (!content.Contains("DefaultConnection"))
            {
                var updated = content.Replace("{", """
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CifraShopDb;User Id=sa;Password=Your_strong_Password123;TrustServerCertificate=True;"
  },
""", StringComparison.Ordinal);
                await File.WriteAllTextAsync(settingsPath, updated);
                Created("Строка подключения добавлена в appsettings.json");
            }
            else
                Ok("Строка подключения найдена");
        }
        else
            Fail("appsettings.json не найден!");
    }

    // ─── Phase 3: Database ────────────────────────────────────────

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

        // Проверяем через docker compose ps (надёжнее чем парсить имена)
        var composePs = await RunCmdAsync("docker", "compose ps --format '{{.Status}}'");
        if (composePs != null && composePs.Contains("Up"))
        {
            Ok("SQL Server уже запущен");
            return;
        }

        var allContainers = await RunCmdAsync("docker", "compose ps -a --format '{{.Status}}'");
        if (allContainers != null && allContainers.Length > 2)
        {
            Log("    Запуск существующего контейнера...");
            await RunCmdAsync("docker", "compose up -d db");
        }
        else
        {
            Log("    Скачивание образа SQL Server...");
            try
            {
                await RunCmdAsync("docker", "compose pull db");
            }
            catch
            {
                Warn("Не удалось скачать образ. Проверьте подключение к интернету.");
                return;
            }
            Log("    Создание контейнера...");
            await RunCmdAsync("docker", "compose up -d db");
        }

        Log("    Ожидание готовности SQL Server...");
        for (int i = 0; i < 90; i++)
        {
            await Task.Delay(1000);
            if (await IsPortOpenAsync("localhost", 1433))
            {
                Ok("SQL Server готов");
                return;
            }
            if (i > 0 && i % 10 == 0) await AnimateWait("    Идёт инициализация", i, 90);
        }
        Console.WriteLine();
        Warn("SQL Server не ответил за 90 сек. Проверьте Docker.");
    }

    // ─── Phase 4: Migrations ──────────────────────────────────────

    private async Task EnsureMigrationsAsync()
    {
        var infraDir = Path.Combine(_rootDir, "CifraShop.Infrastructure");
        var apiDir = Path.Combine(_rootDir, "CifraShop.API");

        var efList = await RunCmdAsync("dotnet", "tool list -g");
        if (efList != null && !efList.Contains("dotnet-ef"))
        {
            Log("    Установка dotnet-ef...");
            await RunCmdAsync("dotnet", "tool install --global dotnet-ef --version 10.0.*");
        }

        // Повторяем попытку, т.к. SQL Server может быть ещё не готов к接受-accepting
        for (int attempt = 1; attempt <= 3; attempt++)
        {
            Log($"    Применение миграций (попытка {attempt}/3)...");
            var result = await RunCmdAsync("dotnet",
                $"ef database update --project \"{infraDir}\" --startup-project \"{apiDir}\"");

            if (result == null)
            {
                if (attempt < 3)
                {
                    Warn($"    Попытка {attempt} не удалась, повтор через 5 сек...");
                    await Task.Delay(5000);
                    continue;
                }
                Warn("Не удалось применить миграции после 3 попыток.");
                return;
            }

            if (result.Contains("Done"))
            {
                Ok("БД обновлена");
                return;
            }

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

            if (result.Contains("connection") || result.Contains("connect") || result.Contains("timeout"))
            {
                Warn($"    SQL Server ещё не готов (попытка {attempt}/3)...");
                if (attempt < 3) await Task.Delay(5000);
                continue;
            }

            // Если есть любая другая ошибка — не повторяем
            Warn("Не удалось применить миграции:");
            foreach (var line in result.Split('\n').Where(l => l.Contains("error") || l.Contains("Error")))
                Log($"    {line.Trim()}");
            return;
        }
    }

    // ─── Phase 5: Services ────────────────────────────────────────

    private async Task StartServicesAsync()
    {
        await KillPortAsync(5000);
        await KillPortAsync(5001);

        Log($"    Запуск API (порт {_apiPort})...");
        _apiProcess = StartDotnet("CifraShop.API");
        if (_apiProcess != null)
        {
            for (int i = 0; i < 40; i++)
            {
                await Task.Delay(500);
                if (await IsPortOpenAsync("localhost", _apiPort))
                {
                    Ok($"API → https://localhost:{_apiPort}");
                    break;
                }
                if (i == 39) Warn("API не запустился за 20 сек");
            }
        }

        Log($"    Запуск клиента (порт {_clientPort})...");
        _clientProcess = StartDotnet("CifraShop.Client");
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

    private async Task PrintLaunchResultAsync()
    {
        Console.WriteLine();
        var apiOk = _apiProcess is { HasExited: false };
        var clientOk = _clientProcess is { HasExited: false };

        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("┌" + new string('─', W) + "┐");
        Console.WriteLine("│" + Center("🚀  ВСЁ ГОТОВО К РАБОТЕ  🚀") + "│");
        Console.WriteLine("├" + new string('─', W) + "┤");
        Console.ResetColor();

        PrintStatusLine("API", apiOk, apiOk ? $"https://localhost:{_apiPort}" : "не запущен");
        PrintStatusLine("Клиент", clientOk, clientOk ? $"http://localhost:{_clientPort}/admin" : "не запущен");

        if (_dockerAvailable)
        {
            var dbPs = await RunCmdAsync("docker", "compose ps --format '{{.Status}}'");
            var dbOk = dbPs != null && dbPs.Contains("Up");
            PrintStatusLine("БД", dbOk, dbOk ? "SQL Server (Docker)" : "не запущен");
        }

        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("└" + new string('─', W) + "┘");
        Console.ResetColor();
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
    }

    // ─── Menu ─────────────────────────────────────────────────────

    private async Task RunMenuAsync()
    {
        while (true)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("  ╭─ ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("МЕНЮ УПРАВЛЕНИЯ");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(" " + new string('─', W - 20) + "╮");
            Console.ResetColor();

            MenuRow("1", "🌐  Открыть админ-панель", ConsoleColor.Cyan);
            MenuRow("2", "🏠  Открыть главную страницу", ConsoleColor.Cyan);
            Separator();
            MenuRow("3", "🔄  Перезапустить API", ConsoleColor.Yellow);
            MenuRow("4", "🔄  Перезапустить клиент", ConsoleColor.Yellow);
            MenuRow("5", "🔄  Перезапустить всё", ConsoleColor.Yellow);
            Separator();
            MenuRow("6", "⏹   Остановить всё", ConsoleColor.Red);
            Separator();
            MenuRow("9", "📊  Статус сервисов", ConsoleColor.Green);
            MenuRow("0", "🚪  Выход", ConsoleColor.DarkGray);

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("  ╰");
            Console.Write(new string('─', W - 3));
            Console.WriteLine("╯");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("  ▸ ");
            Console.ResetColor();

            var choice = (Console.ReadLine() ?? "").Trim();

            switch (choice)
            {
                case "1": OpenBrowser($"http://localhost:{_clientPort}/admin"); break;
                case "2": OpenBrowser($"http://localhost:{_clientPort}"); break;
                case "3":
                    StopProc(_apiProcess); _apiProcess = null;
                    await KillPortAsync(_apiPort);
                    Log($"  Перезапуск API (порт {_apiPort})...");
                    _apiProcess = StartDotnet("CifraShop.API");
                    if (_apiProcess != null && await WaitForPortAsync(_apiPort)) Ok("API перезапущен");
                    else Warn("API не запустился");
                    break;
                case "4":
                    StopProc(_clientProcess); _clientProcess = null;
                    await KillPortAsync(_clientPort);
                    Log($"  Перезапуск клиента (порт {_clientPort})...");
                    _clientProcess = StartDotnet("CifraShop.Client");
                    if (_clientProcess != null && await WaitForPortAsync(_clientPort)) Ok("Клиент перезапущен");
                    else Warn("Клиент не запустился");
                    break;
                case "5":
                    await StopAllAsync();
                    await StartServicesAsync();
                    await PrintLaunchResultAsync();
                    break;
                case "6":
                    await StopAllAsync();
                    Log("  Все сервисы остановлены.");
                    break;
                case "9": await ShowStatusAsync(); break;
                case "0":
                    await StopAllAsync();
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("\n  До свидания!\n");
                    Console.ResetColor();
                    return;
            }
        }
    }

    private async Task<bool> WaitForPortAsync(int port)
    {
        for (int i = 0; i < 40; i++)
        {
            await Task.Delay(500);
            if (await IsPortOpenAsync("localhost", port)) return true;
        }
        return false;
    }

    private async Task StopAllAsync()
    {
        StopProc(_apiProcess); _apiProcess = null;
        StopProc(_clientProcess); _clientProcess = null;
        await KillPortAsync(_apiPort);
        await KillPortAsync(_clientPort);
    }

    private async Task ShowStatusAsync()
    {
        var apiOk = _apiProcess is { HasExited: false };
        var clientOk = _clientProcess is { HasExited: false };

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("┌" + new string('─', W) + "┐");
        Console.WriteLine("│" + Center("СТАТУС СЕРВИСОВ") + "│");
        Console.WriteLine("├" + new string('─', W) + "┤");
        Console.ResetColor();

        StatusRow("API", apiOk, apiOk ? $"PID {_apiProcess!.Id}" : "остановлен");
        StatusRow("Клиент", clientOk, clientOk ? $"PID {_clientProcess!.Id}" : "остановлен");

        if (_dockerAvailable)
        {
            var dbPs = await RunCmdAsync("docker", "compose ps --format '{{.Status}}'");
            var dbOk = dbPs != null && dbPs.Contains("Up");
            StatusRow("БД", dbOk, dbOk ? "SQL Server" : "остановлен");
        }

        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("└" + new string('─', W) + "┘");
        Console.ResetColor();
    }

    // ─── Helpers ──────────────────────────────────────────────────

    private Process? StartDotnet(string project)
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
                }
            };
            p.Start();
            return p;
        }
        catch (Exception ex)
        {
            Fail($"Не удалось запустить dotnet для {project}: {ex.Message}");
            return null;
        }
    }

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

    private static async Task<bool> IsPortOpenAsync(string host, int port)
    {
        try { using var c = new TcpClient(); await c.ConnectAsync(host, port); return true; }
        catch { return false; }
    }

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

    // ─── UI Drawing ───────────────────────────────────────────────

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

    private static void PrintStep(string msg)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  → {msg}");
        Console.ResetColor();
    }

    private static void Ok(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("    ✓ ");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine(msg);
        Console.ResetColor();
    }

    private static void Created(string msg)
    {
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write("    ✦ ");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine(msg);
        Console.ResetColor();
    }

    private static void Warn(string msg)
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("    ⚠ ");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine(msg);
        Console.ResetColor();
    }

    private static void Fail(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("    ✗ ");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine(msg);
        Console.ResetColor();
    }

    private static void Log(string msg)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
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

    private static void UICard(string title, ConsoleColor color, string message)
    {
        Console.ForegroundColor = color;
        Console.WriteLine("┌" + new string('─', W) + "┐");
        Console.WriteLine("│" + Center(title) + "│");
        Console.WriteLine("├" + new string('─', W) + "┤");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Gray;
        foreach (var line in message.Split('\n'))
        {
            var lineLen = line.Length;
            var pad = Math.Max(0, W - 2 - lineLen);
            Console.WriteLine("│  " + line + new string(' ', pad) + "  │");
        }
        Console.ForegroundColor = color;
        Console.WriteLine("└" + new string('─', W) + "┘");
        Console.ResetColor();
    }

    private static string Center(string s)
    {
        var pad = W - s.Length;
        if (pad <= 0) return s;
        var left = pad / 2;
        var right = pad - left;
        return new string(' ', left) + s + new string(' ', right);
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
