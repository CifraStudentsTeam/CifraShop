using System.Text.Json;

namespace CifraShop.Launcher;

public record LauncherConfig
{
    public const int DefaultDbPort = 1433;

    public string RootDir { get; }
    public bool QuickMode { get; }
    public bool SkipDocker { get; }
    public bool ShowStatus { get; }
    public bool ShowVersion { get; }
    public bool ResetAll { get; }
    public int ApiPort { get; }
    public int ClientPort { get; }
    public int DbPort { get; }

    private LauncherConfig(string rootDir, bool quickMode, bool skipDocker, bool showStatus, bool showVersion, bool resetAll, int apiPort, int clientPort, int dbPort)
    {
        RootDir = rootDir;
        QuickMode = quickMode;
        SkipDocker = skipDocker;
        ShowStatus = showStatus;
        ShowVersion = showVersion;
        ResetAll = resetAll;
        ApiPort = apiPort;
        ClientPort = clientPort;
        DbPort = dbPort;
    }

    public static LauncherConfig? Parse(string[] args)
    {
        var rootDir = FindProjectRoot();
        if (rootDir == null) return null;

        var myArgs = args;
        var dashDashIdx = Array.IndexOf(args, "--");
        if (dashDashIdx >= 0 && dashDashIdx + 1 < args.Length)
            myArgs = args[(dashDashIdx + 1)..];

        var quickMode = myArgs.Contains("--quick", StringComparer.OrdinalIgnoreCase);
        var skipDocker = myArgs.Contains("--skip-docker", StringComparer.OrdinalIgnoreCase);
        var showStatus = myArgs.Contains("--status", StringComparer.OrdinalIgnoreCase);
        var showVersion = myArgs.Contains("--version", StringComparer.OrdinalIgnoreCase);
        var resetAll = myArgs.Contains("--reset", StringComparer.OrdinalIgnoreCase);
        var customApiPort = int.TryParse(GetArgValue(myArgs, "--port-api"), out var ap) ? ap : (int?)null;
        var customClientPort = int.TryParse(GetArgValue(myArgs, "--port-client"), out var cp) ? cp : (int?)null;
        var customDbPort = int.TryParse(GetArgValue(myArgs, "--port-db"), out var dp) ? dp : (int?)null;

        var apiPort = ReadPortFromLaunchSettings(
            Path.Combine(rootDir, "CifraShop.API", "Properties", "launchSettings.json")) ?? 5000;
        var clientPort = ReadPortFromLaunchSettings(
            Path.Combine(rootDir, "CifraShop.Client", "Properties", "launchSettings.json")) ?? 5001;
        var dbPort = customDbPort ?? DefaultDbPort;

        if (customApiPort.HasValue) apiPort = customApiPort.Value;
        if (customClientPort.HasValue) clientPort = customClientPort.Value;

        return new LauncherConfig(rootDir, quickMode, skipDocker, showStatus, showVersion, resetAll, apiPort, clientPort, dbPort);
    }

    public static string? FindProjectRoot()
    {
        var dir = AppContext.BaseDirectory;
        for (int i = 0; i < 15; i++)
        {
            if (File.Exists(Path.Combine(dir, "CifraShop.slnx")) || File.Exists(Path.Combine(dir, "CifraShop.sln")))
                return dir;
            var parent = Directory.GetParent(dir);
            if (parent == null) break;
            dir = parent.FullName;
        }
        return null;
    }

    private static string? GetArgValue(string[] args, string flag)
    {
        var idx = Array.IndexOf(args, flag);
        if (idx >= 0 && idx + 1 < args.Length) return args[idx + 1];
        return null;
    }

    private static int? ReadPortFromLaunchSettings(string path)
    {
        if (!File.Exists(path)) return null;
        try
        {
            using var json = JsonDocument.Parse(File.ReadAllText(path));
            var profiles = json.RootElement.GetProperty("profiles");
            foreach (var prop in profiles.EnumerateObject())
            {
                if (prop.Value.TryGetProperty("applicationUrl", out var url))
                {
                    string? urlStr = null;
                    if (url.ValueKind == JsonValueKind.String)
                        urlStr = url.GetString();
                    else if (url.ValueKind == JsonValueKind.Array)
                    {
                        var first = url.EnumerateArray().FirstOrDefault();
                        if (first.ValueKind != JsonValueKind.Undefined)
                            urlStr = first.GetString();
                    }

                    if (urlStr != null && urlStr.Contains("localhost"))
                    {
                        var lastColon = urlStr.LastIndexOf(':');
                        if (lastColon > 0)
                        {
                            var portStr = urlStr[(lastColon + 1)..].TrimEnd('/');
                            if (int.TryParse(portStr, out var port) && port > 0)
                                return port;
                        }
                    }
                }
            }
        }
        catch { }
        return null;
    }
}
