using System.Diagnostics;
using System.Text;
using CifraShop.Launcher.Services;

namespace CifraShop.Launcher;

public class ProjectLauncher : IDisposable
{
    private readonly LauncherConfig _config;
    private readonly DependencyChecker _dependencyChecker;
    private readonly ConfigManager _configManager;
    private readonly DatabaseManager _databaseManager;
    private readonly MigrationRunner _migrationRunner;
    private readonly ProcessLauncher _processLauncher;
    private readonly ProcessMonitor _monitor;
    private int _disposed;

    private readonly StringBuilder _apiLogBuffer = new();
    private readonly object _apiLogLock = new();
    private readonly StringBuilder _clientLogBuffer = new();
    private readonly object _clientLogLock = new();

    public ProjectLauncher(LauncherConfig config)
    {
        _config = config;
        _dependencyChecker = new DependencyChecker();
        _configManager = new ConfigManager();
        _databaseManager = new DatabaseManager(config.RootDir);
        _migrationRunner = new MigrationRunner(config.RootDir);
        _processLauncher = new ProcessLauncher(config.RootDir);
        _monitor = new ProcessMonitor(config.RootDir, _processLauncher, _apiLogBuffer, _apiLogLock, _clientLogBuffer, _clientLogLock);
    }

    public async Task RunAsync()
    {
        UI.ConsoleUI.PrintBanner(_config.QuickMode, _config.SkipDocker, _config.RootDir);
        UI.ConsoleUI.Log($"  Порты: API={_config.ApiPort}, Клиент={_config.ClientPort}");

        var depsOk = await RunPhaseAsync(1, "Проверка зависимостей", () => _dependencyChecker.EnsureAsync(_config.SkipDocker));
        if (!depsOk) return;

        await RunPhaseAsync(2, "Настройка конфигурации", () => _configManager.EnsureAsync(_config.RootDir, _dependencyChecker.DockerAvailable));

        if (!(_config.QuickMode && await CommandRunner.IsPortOpenAsync("localhost", _config.DbPort)))
            await RunPhaseAsync(3, "Запуск базы данных", () => _databaseManager.StartAsync(_dependencyChecker.DockerAvailable, _config.DbPort));
        else
            UI.ConsoleUI.Ok("SQL Server уже доступен (--quick)");

        await RunPhaseAsync(4, "Применение миграций", () => _migrationRunner.ApplyAsync());

        Process? apiProcess = null;
        bool apiInDocker = false;
        await RunPhaseAsync(5, "Запуск API", async () =>
            (apiProcess, apiInDocker) = await _processLauncher.StartApiAsync(_config.ApiPort, _dependencyChecker.DockerAvailable, _apiLogBuffer, _apiLogLock));

        Process? clientProcess = null;
        await RunPhaseAsync(6, "Запуск клиента", async () =>
            clientProcess = await _processLauncher.StartClientAsync(_config.ApiPort, _config.ClientPort, _clientLogBuffer, _clientLogLock));

        _monitor.SetProcesses(apiProcess, clientProcess, apiInDocker, _config.ApiPort, _config.ClientPort);
        _monitor.Start();

        await UI.ConsoleUI.PrintLaunchResultAsync(_config.RootDir, _dependencyChecker.DockerAvailable, apiInDocker, apiProcess, _config.ApiPort, clientProcess, _config.ClientPort);
        await RunMenuAsync();
    }

    public async Task ResetAsync()
    {
        try { Console.Clear(); } catch (IOException) { }
        UI.ConsoleUI.PrintBanner(_config.QuickMode, _config.SkipDocker, _config.RootDir);
        UI.ConsoleUI.Log("  Сброс всех данных...");

        await SafeStopAllAsync();

        var dockerOk = await CommandRunner.RunAsync("docker", "version --format '{{.Server.Version}}'") != null;
        if (dockerOk)
        {
            UI.ConsoleUI.Log("  Остановка Docker контейнеров...");
            await CommandRunner.RunDockerComposeAsync(_config.RootDir, "down -v");
            UI.ConsoleUI.Log("  Очистка образов...");
            await CommandRunner.RunAsync("docker", "image prune -f");
            UI.ConsoleUI.Ok("Docker ресурсы очищены");
        }
        else
            UI.ConsoleUI.Warn("Docker недоступен, контейнеры не очищены");

        try { File.Delete(Path.Combine(_config.RootDir, ".last-docker-build")); } catch { }
        UI.ConsoleUI.Ok("Сброс завершён.");
    }

    public async Task ShowStatusAsync()
    {
        UI.ConsoleUI.PrintBanner(_config.QuickMode, _config.SkipDocker, _config.RootDir);
        Console.WriteLine();

        var apiOk = await CommandRunner.IsPortOpenAsync("localhost", _config.ApiPort);
        var clientOk = await CommandRunner.IsPortOpenAsync("localhost", _config.ClientPort);
        var dbOk = await CommandRunner.IsPortOpenAsync("localhost", _config.DbPort);

        UI.ConsoleUI.PrintStatusLinePublic("БД", dbOk, dbOk ? $"SQL Server (порт {_config.DbPort})" : "не запущен");
        UI.ConsoleUI.PrintStatusLinePublic("API", apiOk, apiOk ? $"http://localhost:{_config.ApiPort}" : "не запущен");
        UI.ConsoleUI.PrintStatusLinePublic("Клиент", clientOk, clientOk ? $"http://localhost:{_config.ClientPort}" : "не запущен");
    }

    private async Task RunPhaseAsync(int num, string title, Func<Task> action)
    {
        UI.ConsoleUI.PrintPhaseStart(num, 6, title);
        await action();
    }

    private async Task<bool> RunPhaseAsync(int num, string title, Func<Task<bool>> action)
    {
        UI.ConsoleUI.PrintPhaseStart(num, 6, title);
        return await action();
    }

    // ── Меню ─────────────────────────────────────────────────────

    private async Task RunMenuAsync()
    {
        while (true)
        {
            try
            {
                Console.WriteLine();
                var (api, client, apiInDocker, apiPort, clientPort) = _monitor.GetState();
                UI.ConsoleUI.DrawMenu(apiPort, (api is { HasExited: false }) || apiInDocker, clientPort, client is { HasExited: false });

                var input = Console.ReadLine();
                if (input == null) { await SafeStopAllAsync(); return; }
                var choice = input.Trim().ToLower();

                switch (choice)
                {
                    case "1": SafeCall(() => ProcessLauncher.OpenBrowser($"http://localhost:{clientPort}/admin")); break;
                    case "2": SafeCall(() => ProcessLauncher.OpenBrowser($"http://localhost:{clientPort}")); break;
                    case "3": await SafeCallAsync(RestartApiAsync); break;
                    case "4": await SafeCallAsync(RestartClientAsync); break;
                    case "5": await SafeCallAsync(RestartAllAsync); break;
                    case "6": await SafeCallAsync(async () => { await SafeStopAllAsync(); UI.ConsoleUI.Ok("Все сервисы остановлены."); }); break;
                    case "7": await SafeCallAsync(RebuildDockerApiAsync); break;
                    case "8": await SafeCallAsync(AskAndCleanupDockerAsync); break;
                    case "9": await SafeCallAsync(ShowLogsMenuAsync); break;
                    case "0": await SafeStopAllAsync(); Console.WriteLine("\n  До свидания!\n"); return;
                    default: if (!string.IsNullOrEmpty(choice)) UI.ConsoleUI.Fail("Используйте цифры 0-9."); break;
                }
            }
            catch (Exception ex) { UI.ConsoleUI.Fail($"Ошибка: {ex.Message}"); }
        }
    }

    private static void SafeCall(Action action) { try { action(); } catch (Exception ex) { UI.ConsoleUI.Fail($"Ошибка: {ex.Message}"); } }
    private static async Task SafeCallAsync(Func<Task> action) { try { await action(); } catch (Exception ex) { UI.ConsoleUI.Fail($"Ошибка: {ex.Message}"); } }

    // ── Остановка (идемпотентная) ────────────────────────────────

    private async Task SafeStopAllAsync()
    {
        _monitor.Stop();
        var (api, client, _, _, _) = _monitor.GetState();
        ProcessLauncher.StopProc(api);
        ProcessLauncher.StopProc(client);
        if (_dependencyChecker.DockerAvailable)
            await CommandRunner.RunDockerComposeAsync(_config.RootDir, "stop");
        await CommandRunner.KillPortAsync(_config.ApiPort);
        await CommandRunner.KillPortAsync(_config.ClientPort);
        _monitor.ResetRestartCounts();
    }

    // ── Рестарт API (идемпотентный) ─────────────────────────────

    private async Task RestartApiAsync()
    {
        _monitor.Stop();
        _monitor.ResetRestartCounts();
        var (oldApi, _, apiInDocker, _, _) = _monitor.GetState();

        if (apiInDocker)
        {
            await CommandRunner.RunDockerComposeAsync(_config.RootDir, "stop api");
            await CommandRunner.KillPortAsync(_config.ApiPort);
            UI.ConsoleUI.Log($"  Перезапуск API (Docker, порт {_config.ApiPort})...");
            await CommandRunner.RunDockerComposeAsync(_config.RootDir, "up -d api");
            if (await CommandRunner.WaitForPortAsync(_config.ApiPort)) UI.ConsoleUI.Ok("API перезапущен (Docker)");
            else UI.ConsoleUI.Warn("API не запустился");
            _monitor.UpdateApiProcess(null, true);
        }
        else
        {
            ProcessLauncher.StopProc(oldApi);
            await CommandRunner.KillPortAsync(_config.ApiPort);
            UI.ConsoleUI.Log($"  Перезапуск API (порт {_config.ApiPort})...");
            lock (_apiLogLock) { _apiLogBuffer.AppendLine("\n=== РЕСТАРТ ==="); }
            var apiProcess = _processLauncher.StartDotnetWithLogs("CifraShop.API", _apiLogBuffer, _apiLogLock);
            if (apiProcess != null && await CommandRunner.WaitForPortAsync(_config.ApiPort)) UI.ConsoleUI.Ok("API перезапущен");
            else UI.ConsoleUI.Warn("API не запустился");
            _monitor.UpdateApiProcess(apiProcess, false);
        }
        _monitor.Start();
    }

    private async Task RestartClientAsync()
    {
        _monitor.Stop();
        _monitor.ResetRestartCounts();
        var (oldApi, oldClient, apiInDocker, _, _) = _monitor.GetState();

        ProcessLauncher.StopProc(oldClient);
        await CommandRunner.KillPortAsync(_config.ClientPort);
        UI.ConsoleUI.Log($"  Перезапуск клиента (порт {_config.ClientPort})...");
        lock (_clientLogLock) { _clientLogBuffer.AppendLine("\n=== РЕСТАРТ ==="); }
        var clientProcess = _processLauncher.StartDotnetWithLogs("CifraShop.Client", _clientLogBuffer, _clientLogLock);
        if (clientProcess != null && await CommandRunner.WaitForPortAsync(_config.ClientPort)) UI.ConsoleUI.Ok("Клиент перезапущен");
        else UI.ConsoleUI.Warn("Клиент не запустился");

        _monitor.SetProcesses(oldApi, clientProcess, apiInDocker, _config.ApiPort, _config.ClientPort);
        _monitor.Start();
    }

    private async Task RestartAllAsync()
    {
        await SafeStopAllAsync();
        var (apiProcess, apiInDocker) = await _processLauncher.StartApiAsync(_config.ApiPort, _dependencyChecker.DockerAvailable, _apiLogBuffer, _apiLogLock);
        var clientProcess = await _processLauncher.StartClientAsync(_config.ApiPort, _config.ClientPort, _clientLogBuffer, _clientLogLock);
        _monitor.SetProcesses(apiProcess, clientProcess, apiInDocker, _config.ApiPort, _config.ClientPort);
        _monitor.Start();
        await UI.ConsoleUI.PrintLaunchResultAsync(_config.RootDir, _dependencyChecker.DockerAvailable, apiInDocker, apiProcess, _config.ApiPort, clientProcess, _config.ClientPort);
    }

    private async Task RebuildDockerApiAsync()
    {
        if (!_dependencyChecker.DockerAvailable) { UI.ConsoleUI.Warn("Docker недоступен"); return; }

        _monitor.Stop();
        _monitor.ResetRestartCounts();
        UI.ConsoleUI.Log("  Пересборка Docker образа API...");
        await CommandRunner.RunDockerComposeAsync(_config.RootDir, "stop api");
        await CommandRunner.KillPortAsync(_config.ApiPort);
        var composePath = Path.Combine(_config.RootDir, "docker-compose.yml");
        var (_, buildExitCode) = await CommandRunner.RunWithOutputAsync("docker", $"compose -f \"{composePath}\" build api");
        if (buildExitCode == 0)
        {
            UI.ConsoleUI.Ok("Образ пересобран");
            ConfigManager.UpdateDockerBuildTimestamp(_config.RootDir);
            await CommandRunner.RunDockerComposeAsync(_config.RootDir, "up -d api");
            if (await CommandRunner.WaitForPortAsync(_config.ApiPort)) UI.ConsoleUI.Ok("API перезапущен (Docker)");
            else UI.ConsoleUI.Warn("API не запустился");
            _monitor.UpdateApiProcess(null, true);
        }
        else
        {
            UI.ConsoleUI.Warn("Не удалось собрать образ");
            var (_, _, apiInDocker, _, _) = _monitor.GetState();
            _monitor.UpdateApiProcess(null, apiInDocker);
        }
        _monitor.Start();
    }

    private async Task AskAndCleanupDockerAsync()
    {
        if (!_dependencyChecker.DockerAvailable) { UI.ConsoleUI.Warn("Docker недоступен"); return; }
        Console.Write("    Остановить и очистить Docker-образы? [y/N] ");
        var confirm = (Console.ReadLine() ?? "").Trim().ToLower();
        if (confirm is "y" or "yes")
        {
            await CommandRunner.RunDockerComposeAsync(_config.RootDir, "down");
            await CommandRunner.RunAsync("docker", "image prune -f");
            UI.ConsoleUI.Ok("Docker ресурсы очищены");
            await CommandRunner.RunDockerComposeAsync(_config.RootDir, "up -d");
        }
    }

    private async Task ShowLogsMenuAsync()
    {
        UI.ConsoleUI.ShowLogsMenu();
        var sub = (Console.ReadLine() ?? "").Trim();
        var (api, client, apiInDocker, apiPort, clientPort) = _monitor.GetState();
        switch (sub)
        {
            case "1": UI.ConsoleUI.ShowFullLog(_apiLogBuffer, _apiLogLock, "API"); break;
            case "2": UI.ConsoleUI.ShowFullLog(_clientLogBuffer, _clientLogLock, "КЛИЕНТ"); break;
            case "3":
                await UI.ConsoleUI.PrintServiceStatusesAsync(_config.RootDir, _dependencyChecker.DockerAvailable, apiInDocker, api, apiPort, client, clientPort);
                break;
        }
    }

    // ── Dispose ──────────────────────────────────────────────────

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
        _monitor.Dispose();
        var (api, client, _, _, _) = _monitor.GetState();
        ProcessLauncher.StopProc(api);
        ProcessLauncher.StopProc(client);
        try
        {
            if (_dependencyChecker.DockerAvailable)
            {
                using var p = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "docker",
                        Arguments = $"compose -f \"{Path.Combine(_config.RootDir, "docker-compose.yml")}\" stop",
                        UseShellExecute = false, CreateNoWindow = true
                    }
                };
                p.Start();
                p.WaitForExit(5000);
            }
        }
        catch { }
        _processLauncher?.Dispose();
    }
}
