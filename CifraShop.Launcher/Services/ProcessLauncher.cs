using System.Diagnostics;
using System.Net.Http;
using System.Text;

namespace CifraShop.Launcher.Services;

internal class ProcessLauncher : IDisposable
{
    private readonly string _rootDir;
    private readonly HttpClient _httpClient = new(
        new HttpClientHandler { ServerCertificateCustomValidationCallback = (_, _, _, _) => true })
    { Timeout = TimeSpan.FromSeconds(5) };

    public ProcessLauncher(string rootDir) => _rootDir = rootDir;

    public async Task<(Process? process, bool isDocker)> StartApiAsync(int apiPort, bool dockerAvailable, StringBuilder logBuffer, object logLock, CancellationToken ct = default)
    {
        if (dockerAvailable)
        {
            UI.ConsoleUI.Log("    Сборка Docker образа API...");
            var composePath = Path.Combine(_rootDir, "docker-compose.yml");
            var (buildOutput, buildExitCode) = await CommandRunner.RunStreamingAsync("docker", $"compose -f \"{composePath}\" build api",
                line =>
                {
                    if (line.Contains("=>") || line.Contains("Building") || line.Contains("error"))
                    {
                        var clean = line.Length > 60 ? line[..57] + "..." : line;
                        lock (Console.Out)
                        {
                            Console.ForegroundColor = line.Contains("error") ? ConsoleColor.Red : ConsoleColor.DarkGray;
                            Console.Write($"\r    {clean,-58}");
                            Console.ResetColor();
                        }
                    }
                }, timeoutMs: 300_000, ct: ct);
            Console.WriteLine();

            if (buildExitCode == 0)
            {
                UI.ConsoleUI.Ok("Docker образ собран");
                ConfigManager.UpdateDockerBuildTimestamp(_rootDir);
                UI.ConsoleUI.Log("    Запуск API контейнера...");
                await CommandRunner.RunDockerComposeAsync(_rootDir, "up -d api", ct);
                for (int i = 0; i < 40; i++)
                {
                    if (ct.IsCancellationRequested) break;
                    await Task.Delay(500, ct);
                    if (await CommandRunner.IsPortOpenAsync("localhost", apiPort))
                    {
                        UI.ConsoleUI.Ok($"API → http://localhost:{apiPort} (Docker)");
                        return (null, true);
                    }
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write($"\r    Ожидание API... {i * 500}мс");
                    Console.ResetColor();
                }
                Console.WriteLine();
                UI.ConsoleUI.Warn("API не запустился за 20 сек");
                return (null, false);
            }

            var lastLines = buildOutput?.Split('\n', StringSplitOptions.RemoveEmptyEntries).TakeLast(5);
            UI.ConsoleUI.Warn("Не удалось собрать Docker образ:");
            if (lastLines != null)
                foreach (var line in lastLines)
                    if (!string.IsNullOrWhiteSpace(line))
                        UI.ConsoleUI.Log($"    {line.Trim()}");
        }

        await CommandRunner.KillPortAsync(apiPort, ct);
        if (await CommandRunner.IsPortOpenAsync("localhost", apiPort))
        {
            UI.ConsoleUI.Warn($"Порт {apiPort} всё ещё занят.");
            return (null, false);
        }
        UI.ConsoleUI.Log($"    Запуск API локально (порт {apiPort})...");
        var process = StartDotnetWithLogs("CifraShop.API", logBuffer, logLock);
        if (process != null)
        {
            for (int i = 0; i < 40; i++)
            {
                if (ct.IsCancellationRequested) break;
                await Task.Delay(500, ct);
                if (await CommandRunner.IsPortOpenAsync("localhost", apiPort))
                {
                    UI.ConsoleUI.Ok($"API → https://localhost:{apiPort}");
                    return (process, false);
                }
                if (i == 39) UI.ConsoleUI.Warn("API не запустился за 20 сек");
            }
            StopProc(process);
        }
        return (null, false);
    }

    public async Task<Process?> StartClientAsync(int apiPort, int clientPort, StringBuilder logBuffer, object logLock, CancellationToken ct = default)
    {
        UI.ConsoleUI.Log("    Ожидание готовности API...");
        if (!await WaitForApiHealthAsync(apiPort, 60, ct))
            UI.ConsoleUI.Warn("API не отвечает. Запуск клиента всё равно...");

        await CommandRunner.KillPortAsync(clientPort, ct);
        UI.ConsoleUI.Log($"    Запуск клиента (порт {clientPort})...");
        var process = StartDotnetWithLogs("CifraShop.Client", logBuffer, logLock);
        if (process != null)
        {
            for (int i = 0; i < 40; i++)
            {
                if (ct.IsCancellationRequested) break;
                await Task.Delay(500, ct);
                if (await CommandRunner.IsPortOpenAsync("localhost", clientPort))
                {
                    UI.ConsoleUI.Ok($"Клиент → http://localhost:{clientPort}");
                    return process;
                }
                if (i == 39) UI.ConsoleUI.Warn("Клиент не запустился за 20 сек");
            }
            StopProc(process);
        }
        return null;
    }

    public async Task<bool> WaitForApiHealthAsync(int apiPort, int timeoutSec, CancellationToken ct = default)
    {
        var deadline = DateTime.UtcNow.AddSeconds(timeoutSec);
        while (DateTime.UtcNow < deadline)
        {
            if (ct.IsCancellationRequested) return false;
            if (await CheckApiHealthAsync($"https://localhost:{apiPort}/")) return true;
            if (await CheckApiHealthAsync($"http://localhost:{apiPort}/")) return true;
            var remaining = (int)(deadline - DateTime.UtcNow).TotalSeconds;
            if (remaining <= 0) break;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"\r    Проверка API... {timeoutSec - remaining}с/{timeoutSec}с");
            Console.ResetColor();
            await Task.Delay(Math.Min(500, remaining * 1000), ct);
        }
        Console.Write("\r" + new string(' ', 50) + "\r");
        return false;
    }

    private async Task<bool> CheckApiHealthAsync(string url)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    internal Process? StartDotnetWithLogs(string project, StringBuilder logBuffer, object logLock)
    {
        Process? p = null;
        try
        {
            p = new Process
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

            var handler = CreateLogHandler(logBuffer, logLock);
            p.OutputDataReceived += handler;
            p.ErrorDataReceived += handler;

            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            return p;
        }
        catch (Exception ex)
        {
            try { p?.Dispose(); } catch { }
            UI.ConsoleUI.Fail($"Не удалось запустить {project}: {ex.Message}");
            return null;
        }
    }

    private static DataReceivedEventHandler CreateLogHandler(StringBuilder logBuffer, object logLock)
    {
        return (_, e) =>
        {
            if (e.Data == null) return;
            lock (logLock)
            {
                logBuffer.AppendLine(e.Data);
                if (logBuffer.Length > 80_000)
                {
                    var str = logBuffer.ToString();
                    var cutPoint = str.IndexOf('\n', str.Length / 2);
                    if (cutPoint > 0) cutPoint++;
                    logBuffer.Clear();
                    logBuffer.Append(str.AsSpan(cutPoint > 0 ? cutPoint : str.Length / 2));
                }
            }
        };
    }

    internal static void StopProc(Process? proc)
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
        finally { try { proc?.Dispose(); } catch { } }
    }

    internal static void OpenBrowser(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            UI.ConsoleUI.Ok($"Открыт: {url}");
        }
        catch (Exception ex) { UI.ConsoleUI.Fail($"Не удалось открыть браузер: {ex.Message}"); }
    }

    public void Dispose() => _httpClient?.Dispose();
}
