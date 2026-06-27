using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

namespace CifraShop.Launcher.Services;

internal static class CommandRunner
{
    internal static async Task<string?> RunAsync(string cmd, string args, int timeoutMs = 60_000, CancellationToken ct = default)
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
            var readOut = p.StandardOutput.ReadToEndAsync();
            var readErr = p.StandardError.ReadToEndAsync();
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(timeoutMs);
            try
            {
                await p.WaitForExitAsync(cts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                try { p.Kill(); } catch { }
                await DrainAsync(readOut, readErr);
                return null;
            }
            var results = await DrainAsync(readOut, readErr);
            return p.ExitCode == 0 ? results[0] : null;
        }
        catch (System.IO.FileNotFoundException) { return null; }
        catch (System.ComponentModel.Win32Exception) { return null; }
        catch { return null; }
    }

    internal static async Task<(string? Output, int ExitCode)> RunWithOutputAsync(string cmd, string args, int timeoutMs = 120_000, CancellationToken ct = default)
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
            var readOut = p.StandardOutput.ReadToEndAsync();
            var readErr = p.StandardError.ReadToEndAsync();
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(timeoutMs);
            try
            {
                await p.WaitForExitAsync(cts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                try { p.Kill(); } catch { }
                var drained = await DrainAsync(readOut, readErr);
                return (drained[0] + "\n" + drained[1], -1);
            }
            var results = await DrainAsync(readOut, readErr);
            return (results[0] + "\n" + results[1], p.ExitCode);
        }
        catch { return (null, -1); }
    }

    internal static async Task<(string Output, int ExitCode)> RunStreamingAsync(string cmd, string args, Action<string>? onOutput = null, int timeoutMs = 600_000, CancellationToken ct = default)
    {
        var output = new StringBuilder();
        var outputLock = new object();
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
            p.OutputDataReceived += (_, e) =>
            {
                if (e.Data == null) return;
                lock (outputLock) { output.AppendLine(e.Data); }
                try { onOutput?.Invoke(e.Data); } catch { }
            };
            p.ErrorDataReceived += (_, e) =>
            {
                if (e.Data == null) return;
                lock (outputLock) { output.AppendLine(e.Data); }
                try { onOutput?.Invoke(e.Data); } catch { }
            };
            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(timeoutMs);
            try { await p.WaitForExitAsync(cts.Token).ConfigureAwait(false); }
            catch (OperationCanceledException)
            {
                try { p.Kill(); } catch { }
                try { await p.WaitForExitAsync(); } catch { }
                lock (outputLock) { return (output.ToString(), -1); }
            }
            lock (outputLock) { return (output.ToString(), p.ExitCode); }
        }
        catch (Exception ex)
        {
            lock (outputLock)
            {
                output.AppendLine(ex.Message);
                return (output.ToString(), -1);
            }
        }
    }

    internal static async Task<bool> IsPortOpenAsync(string host, int port)
    {
        try { using var c = new TcpClient(); await c.ConnectAsync(host, port); return true; }
        catch { return false; }
    }

    internal static async Task<bool> WaitForPortAsync(int port, int timeoutMs = 20_000, CancellationToken ct = default)
    {
        var iterations = timeoutMs / 500;
        for (int i = 0; i < iterations; i++)
        {
            if (ct.IsCancellationRequested) return false;
            await Task.Delay(500, ct);
            if (await IsPortOpenAsync("localhost", port)) return true;
        }
        return false;
    }

    internal static async Task KillPortAsync(int port, CancellationToken ct = default)
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                var result = await RunAsync("netstat", "-ano", ct: ct);
                if (result == null) return;
                foreach (var line in result.Split('\n'))
                {
                    if (ct.IsCancellationRequested) return;
                    if (line.Contains($":{port}") && line.Contains("LISTENING"))
                    {
                        var parts = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length > 0 && int.TryParse(parts[^1], out var pid))
                        {
                            try { using var proc = Process.GetProcessById(pid); proc.Kill(entireProcessTree: true); proc.WaitForExit(2000); }
                            catch { }
                        }
                    }
                }
            }
            else
            {
                var result = await RunAsync("lsof", $"-ti:{port}", ct: ct);
                if (string.IsNullOrWhiteSpace(result)) return;
                foreach (var pidStr in result.Split('\n', StringSplitOptions.RemoveEmptyEntries))
                {
                    if (int.TryParse(pidStr.Trim(), out var pid))
                        try { using var proc = Process.GetProcessById(pid); proc.Kill(); } catch { }
                }
            }
        }
        catch { }
    }

    internal static async Task<string?> RunDockerComposeAsync(string rootDir, string args, CancellationToken ct = default)
    {
        var composePath = Path.Combine(rootDir, "docker-compose.yml");
        if (!File.Exists(composePath)) { UI.ConsoleUI.Warn("docker-compose.yml не найден"); return null; }

        string content;
        try { content = File.ReadAllText(composePath); }
        catch (IOException) { UI.ConsoleUI.Warn("docker-compose.yml заблокирован"); return null; }

        if (!content.Contains("services:", StringComparison.OrdinalIgnoreCase))
        {
            UI.ConsoleUI.Warn("docker-compose.yml не содержит секцию 'services'");
            return null;
        }

        return await RunAsync("docker", $"compose -f \"{composePath}\" {args}", ct: ct);
    }

    private static async Task<string[]> DrainAsync(Task<string> readOut, Task<string> readErr)
    {
        try
        {
            var all = Task.WhenAll(readOut, readErr);
            if (await Task.WhenAny(all, Task.Delay(5000)) == all)
                return await all;
        }
        catch { }
        return new[] { await ReadSafeAsync(readOut), await ReadSafeAsync(readErr) };
    }

    private static async Task<string> ReadSafeAsync(Task<string> task)
    {
        try { return await task; }
        catch { return ""; }
    }
}
