using System.Diagnostics;

namespace CifraShop.Launcher.Services;

/// <summary>
/// Фаза 1: проверка зависимостей (.NET SDK, Docker).
/// </summary>
internal class DependencyChecker
{
    private bool _dockerAvailable = true;

    public bool DockerAvailable => _dockerAvailable;

    /// <summary>
    /// Проверяет наличие .NET SDK и Docker. Возвращает false если нужно завершить работу.
    /// </summary>
    public async Task<bool> EnsureAsync(bool skipDocker)
    {
        // ── .NET SDK ──────────────────────────────────────────────
        var dotnet = await CommandRunner.RunAsync("dotnet", "--version");
        if (string.IsNullOrEmpty(dotnet))
        {
            UI.ConsoleUI.Fail(".NET SDK не найден");
            UI.ConsoleUI.Log("    Установите .NET 10: https://dotnet.microsoft.com/download");
            UI.ConsoleUI.PressKey();
            return false;
        }
        UI.ConsoleUI.Ok($".NET SDK {dotnet.Trim()}");

        if (skipDocker)
        {
            _dockerAvailable = false;
            return true;
        }

        // ── Docker ────────────────────────────────────────────────
        var docker = await CommandRunner.RunAsync("docker", "version --format '{{.Server.Version}}'");
        if (string.IsNullOrWhiteSpace(docker))
        {
            UI.ConsoleUI.Warn("Docker не найден или не запущен");
            UI.ConsoleUI.Log("    → Установите Docker Desktop: https://docker.com/products/docker-desktop");
            UI.ConsoleUI.Log("    → Или используйте локальный SQL Server на порту 1433");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("    Продолжить без Docker? [y/N] ");
            Console.ResetColor();
            var ans = (Console.ReadLine() ?? "").Trim().ToLower();
            if (ans != "y" && ans != "yes") { UI.ConsoleUI.PressKey(); return false; }
            _dockerAvailable = false;
            return true;
        }
        UI.ConsoleUI.Ok($"Docker {docker.Trim()}");

        // ── Проверка демона Docker ────────────────────────────────
        var ps = await CommandRunner.RunAsync("docker", "ps");
        if (ps == null)
        {
            UI.ConsoleUI.Warn("Docker демон не отвечает. Попытка запуска Docker Desktop...");
            await TryStartDockerDesktop();
            for (int i = 0; i < 60; i++)
            {
                await Task.Delay(1000);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"\r    Ожидание Docker демона... {i + 1}с/60с");
                Console.ResetColor();
                ps = await CommandRunner.RunAsync("docker", "ps");
                if (ps != null)
                {
                    Console.Write("\r" + new string(' ', 50) + "\r");
                    UI.ConsoleUI.Ok("Docker демон запущен");
                    return true;
                }
            }
            Console.WriteLine();
            UI.ConsoleUI.Fail("Docker не запустился за 60 сек");
            UI.ConsoleUI.PressKey();
            return false;
        }

        // ── Docker Compose ────────────────────────────────────────
        var compose = await CommandRunner.RunAsync("docker", "compose version --short");
        if (!string.IsNullOrEmpty(compose))
            UI.ConsoleUI.Ok($"Docker Compose {compose.Trim()}");
        else
            UI.ConsoleUI.Warn("docker compose недоступен (попробуйте 'docker-compose')");

        return true;
    }

    private static async Task TryStartDockerDesktop()
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                var regPath = Microsoft.Win32.Registry.GetValue(
                    @"HKEY_LOCAL_MACHINE\SOFTWARE\Docker Inc.\Docker", "BinPath", "") as string;
                if (!string.IsNullOrEmpty(regPath))
                {
                    var exe = Path.Combine(regPath, "Docker Desktop.exe");
                    if (File.Exists(exe))
                    {
                        Process.Start(new ProcessStartInfo(exe) { UseShellExecute = true });
                        return;
                    }
                }

                var defaultPaths = new[]
                {
                    @"C:\Program Files\Docker\Docker\Docker Desktop.exe",
                    @"C:\Program Files (x86)\Docker\Docker\Docker Desktop.exe"
                };
                foreach (var p in defaultPaths)
                {
                    if (File.Exists(p))
                    {
                        Process.Start(new ProcessStartInfo(p) { UseShellExecute = true });
                        return;
                    }
                }

                await CommandRunner.RunAsync("cmd", "/c start Docker Desktop");
            }
            else if (OperatingSystem.IsMacOS())
            {
                await CommandRunner.RunAsync("open", "-a Docker");
            }
        }
        catch { }
    }
}
