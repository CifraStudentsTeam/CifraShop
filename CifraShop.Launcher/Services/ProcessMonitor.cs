using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

namespace CifraShop.Launcher.Services;

internal class ProcessMonitor : IDisposable
{
    private readonly ConcurrentDictionary<string, int> _restartCounts = new();
    private CancellationTokenSource? _cts;
    private Task? _monitorTask;
    private readonly object _startStopLock = new();
    private const int MaxRestarts = 3;

    private Process? _apiProcess;
    private Process? _clientProcess;
    private bool _apiInDocker;
    private readonly object _stateLock = new();

    private readonly StringBuilder _apiLogBuffer;
    private readonly object _apiLogLock;
    private readonly StringBuilder _clientLogBuffer;
    private readonly object _clientLogLock;

    private readonly string _rootDir;
    private readonly ProcessLauncher _launcher;

    private int _apiPort;
    private int _clientPort;

    public ProcessMonitor(string rootDir, ProcessLauncher launcher, StringBuilder apiLogBuffer, object apiLogLock, StringBuilder clientLogBuffer, object clientLogLock)
    {
        _rootDir = rootDir;
        _launcher = launcher;
        _apiLogBuffer = apiLogBuffer;
        _apiLogLock = apiLogLock;
        _clientLogBuffer = clientLogBuffer;
        _clientLogLock = clientLogLock;
    }

    public void SetProcesses(Process? apiProcess, Process? clientProcess, bool apiInDocker, int apiPort, int clientPort)
    {
        lock (_stateLock)
        {
            _apiProcess = apiProcess;
            _clientProcess = clientProcess;
            _apiInDocker = apiInDocker;
            _apiPort = apiPort;
            _clientPort = clientPort;
        }
    }

    public void UpdateApiProcess(Process? process, bool inDocker)
    {
        lock (_stateLock) { _apiProcess = process; _apiInDocker = inDocker; }
    }

    public void UpdateClientProcess(Process? process)
    {
        lock (_stateLock) { _clientProcess = process; }
    }

    public (Process? api, Process? client, bool apiInDocker, int apiPort, int clientPort) GetState()
    {
        lock (_stateLock)
            return (_apiProcess, _clientProcess, _apiInDocker, _apiPort, _clientPort);
    }

    public void Start()
    {
        lock (_startStopLock)
        {
            _cts?.Cancel();
            try { _monitorTask?.Wait(TimeSpan.FromSeconds(1)); } catch { }
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            _monitorTask = MonitorAsync(_cts.Token);
        }
    }

    public void Stop()
    {
        lock (_startStopLock)
        {
            _cts?.Cancel();
            try { _monitorTask?.Wait(TimeSpan.FromSeconds(2)); } catch { }
            _cts?.Dispose();
            _cts = null;
            _monitorTask = null;
        }
    }

    public void ResetRestartCounts() => _restartCounts.Clear();

    private async Task MonitorAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try { await Task.Delay(5000, ct); }
            catch (OperationCanceledException) { return; }
            catch (ObjectDisposedException) { return; }

            try
            {
                Process? apiProcess;
                Process? clientProcess;
                bool apiInDocker;
                int apiPort;
                int clientPort;

                lock (_stateLock)
                {
                    apiProcess = _apiProcess;
                    clientProcess = _clientProcess;
                    apiInDocker = _apiInDocker;
                    apiPort = _apiPort;
                    clientPort = _clientPort;
                }

                if (apiInDocker)
                    await MonitorDockerServiceAsync("api_docker", "api", "API", apiPort);
                else
                {
                    var restarted = await MonitorLocalProcessAsync("api_local", apiProcess, "API", _apiLogBuffer, _apiLogLock);
                    if (restarted != null)
                    {
                        lock (_stateLock) { _apiProcess = restarted; }
                        ProcessLauncher.StopProc(apiProcess);
                    }
                }

                {
                    var restarted = await MonitorLocalProcessAsync("client", clientProcess, "Клиент", _clientLogBuffer, _clientLogLock);
                    if (restarted != null)
                    {
                        lock (_stateLock) { _clientProcess = restarted; }
                        ProcessLauncher.StopProc(clientProcess);
                    }
                }
            }
            catch (Exception ex)
            {
                lock (_apiLogLock)
                {
                    _apiLogBuffer.AppendLine($"[MONITOR] Ошибка: {ex.Message}");
                }
            }
        }
    }

    private async Task MonitorDockerServiceAsync(string restartKey, string serviceName, string displayName, int apiPort)
    {
        var ps = await CommandRunner.RunDockerComposeAsync(_rootDir, $"ps {serviceName} --format '{{{{.Status}}}}'");
        if (ps == null) return;

        var isUp = ps.Contains("Up") || ps.Contains("running");
        if (isUp) { _restartCounts.TryRemove(restartKey, out _); return; }

        _restartCounts.TryGetValue(restartKey, out var count);
        if (count < MaxRestarts)
        {
            _restartCounts[restartKey] = count + 1;
            LogMonitor($"[!] {displayName} контейнер упал ({count + 1}/{MaxRestarts}). Перезапуск...");
            await CommandRunner.RunDockerComposeAsync(_rootDir, $"up -d {serviceName}");
            await CommandRunner.WaitForPortAsync(apiPort);
        }
        else if (count == MaxRestarts)
        {
            _restartCounts[restartKey] = MaxRestarts + 1;
            LogMonitor($"[!!!] {displayName} упал {MaxRestarts} раз. Авто-перезапуск отключён.");
        }
    }

    private async Task<Process?> MonitorLocalProcessAsync(string restartKey, Process? process, string displayName, StringBuilder logBuffer, object logLock)
    {
        if (process == null) return null;

        if (process.HasExited && process.ExitCode != 0)
        {
            _restartCounts.TryGetValue(restartKey, out var count);
            if (count < MaxRestarts)
            {
                _restartCounts[restartKey] = count + 1;
                LogMonitor($"[!] {displayName} упал (код {process.ExitCode}). Перезапуск ({count + 1}/{MaxRestarts})...");
                var project = restartKey == "client" ? "CifraShop.Client" : "CifraShop.API";
                return _launcher.StartDotnetWithLogs(project, logBuffer, logLock);
            }
            else if (count == MaxRestarts)
            {
                _restartCounts[restartKey] = MaxRestarts + 1;
                LogMonitor($"[!!!] {displayName} упал {MaxRestarts} раз. Авто-перезапуск отключён.");
            }
        }
        else if (process is { HasExited: false })
        {
            _restartCounts.TryRemove(restartKey, out _);
        }
        return null;
    }

    private void LogMonitor(string msg)
    {
        lock (_apiLogLock) { _apiLogBuffer.AppendLine(msg); }
    }

    public void Dispose() => Stop();
}
