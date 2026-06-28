namespace CifraShop.Launcher.Services;

/// <summary>
/// Фаза 3: запуск SQL Server через Docker Compose.
/// </summary>
internal class DatabaseManager
{
    private readonly string _rootDir;

    public DatabaseManager(string rootDir)
    {
        _rootDir = rootDir;
    }

    /// <summary>
    /// Запускает SQL Server. Универсально: проверяет статус, определяет нужно ли скачивать образ.
    /// </summary>
    public async Task StartAsync(bool dockerAvailable, int dbPort)
    {
        if (!dockerAvailable)
        {
            if (await CommandRunner.IsPortOpenAsync("localhost", dbPort))
                UI.ConsoleUI.Ok($"SQL Server доступен на localhost:{dbPort}");
            else
                UI.ConsoleUI.Warn("SQL Server не обнаружен. Убедитесь, что он запущен.");
            return;
        }

        var running = await CommandRunner.RunDockerComposeAsync(_rootDir, "ps db --format '{{.Status}}'");
        if (running != null && running.Contains("Up"))
        {
            UI.ConsoleUI.Ok("SQL Server уже запущен");
            return;
        }

        var allStatus = await CommandRunner.RunDockerComposeAsync(_rootDir, "ps -a db --format '{{.Status}}'");
        if (!string.IsNullOrWhiteSpace(allStatus) && allStatus.Trim().Length > 0)
        {
            UI.ConsoleUI.Log("    Запуск существующего контейнера...");
            var startResult = await CommandRunner.RunDockerComposeAsync(_rootDir, "up -d db");
            if (startResult == null) { UI.ConsoleUI.Warn("Не удалось запустить контейнер SQL Server"); return; }
        }
        else
        {
            UI.ConsoleUI.Log("    Скачивание образа SQL Server (может занять несколько минут)...");
            var (pullOutput, pullExit) = await CommandRunner.RunStreamingAsync("docker", $"compose -f \"{Path.Combine(_rootDir, "docker-compose.yml")}\" pull db",
                line =>
                {
                    if (line.Contains("Downloading") || line.Contains("Extracting") || line.Contains("Pull complete") || line.Contains("already exists"))
                    {
                        var clean = line.Length > 60 ? line[..57] + "..." : line;
                        lock (Console.Out)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            Console.Write($"\r    {clean,-58}");
                            Console.ResetColor();
                        }
                    }
                }, timeoutMs: 600_000);
            Console.WriteLine();

            if (pullExit != 0)
            {
                var lastLines = pullOutput?.Split('\n', StringSplitOptions.RemoveEmptyEntries).TakeLast(3);
                UI.ConsoleUI.Warn("Не удалось скачать образ SQL Server:");
                if (lastLines != null)
                    foreach (var line in lastLines)
                        if (!string.IsNullOrWhiteSpace(line))
                            UI.ConsoleUI.Log($"    {line.Trim()}");
                return;
            }
            UI.ConsoleUI.Ok("Образ SQL Server скачан");

            UI.ConsoleUI.Log("    Создание контейнера...");
            var createResult = await CommandRunner.RunDockerComposeAsync(_rootDir, "up -d db");
            if (createResult == null) { UI.ConsoleUI.Warn("Не удалось создать контейнер SQL Server"); return; }
        }

        await WaitUntilReady(180, dbPort);
    }

    /// <summary>
    /// Ожидание готовности SQL Server (проверка порта).
    /// </summary>
    public async Task<bool> WaitUntilReady(int timeoutSec, int dbPort)
    {
        UI.ConsoleUI.Log("    Ожидание готовности SQL Server...");
        for (int i = 0; i < timeoutSec; i++)
        {
            await Task.Delay(1000);

            if (await CommandRunner.IsPortOpenAsync("localhost", dbPort))
            {
                Console.Write("\r" + new string(' ', 60) + "\r");
                UI.ConsoleUI.Ok("SQL Server готов");
                return true;
            }

            var statusCheck = await CommandRunner.RunDockerComposeAsync(_rootDir, "ps db --format '{{.Status}}'");
            if (i == 30 && (statusCheck == null || !statusCheck.Contains("Up")))
            {
                Console.WriteLine();
                UI.ConsoleUI.Warn("SQL Server не запущен. Повторный запуск контейнера...");
                await CommandRunner.RunDockerComposeAsync(_rootDir, "up -d db");
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"\r    Ожидание SQL Server... {i + 1}с/{timeoutSec}с");
            Console.ResetColor();
        }
        Console.WriteLine();

        var dbLogs = await CommandRunner.RunDockerComposeAsync(_rootDir, "logs db --tail 10");
        if (!string.IsNullOrWhiteSpace(dbLogs))
        {
            UI.ConsoleUI.Warn("SQL Server не ответил за 180 сек. Последние логи:");
            foreach (var line in dbLogs.Split('\n').TakeLast(5))
                if (!string.IsNullOrWhiteSpace(line))
                    UI.ConsoleUI.Log($"    {line.Trim()}");
        }
        else
            UI.ConsoleUI.Warn("SQL Server не ответил за 180 сек. Проверьте Docker.");

        return false;
    }
}
