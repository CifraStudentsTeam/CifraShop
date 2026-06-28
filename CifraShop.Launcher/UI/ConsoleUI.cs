using System.Diagnostics;
using System.Text;

namespace CifraShop.Launcher.UI;

internal static class ConsoleUI
{
    private const int TotalWidth = 64;

    internal static void Ok(string msg) => WriteColored("    [+] ", ConsoleColor.Green, ConsoleColor.Gray, msg);
    internal static void Created(string msg) => WriteColored("    [~] ", ConsoleColor.DarkCyan, ConsoleColor.Gray, msg);
    internal static void Warn(string msg) => WriteColored("    [!] ", ConsoleColor.DarkYellow, ConsoleColor.Gray, msg);
    internal static void Fail(string msg) => WriteColored("    [-] ", ConsoleColor.Red, ConsoleColor.Gray, msg);

    internal static void Log(string msg)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(msg);
        Console.ResetColor();
    }

    internal static void WriteColored(string prefix, ConsoleColor prefixColor, ConsoleColor msgColor, string msg)
    {
        Console.ForegroundColor = prefixColor;
        Console.Write(prefix);
        Console.ForegroundColor = msgColor;
        Console.WriteLine(msg);
        Console.ResetColor();
    }

    // ── Универсальные рисовалки ──────────────────────────────────

    private static void Line(ConsoleColor color, string content)
    {
        Debug.Assert(content.Length == TotalWidth, $"Line width {content.Length} != {TotalWidth}");
        Console.ForegroundColor = color;
        Console.WriteLine(content);
        Console.ResetColor();
    }

    private static string P(string s) => s.PadRight(TotalWidth);

    private static string BoxBorder(char left, char fill, char right) =>
        $"{left}{new string(fill, TotalWidth - 2)}{right}";

    private static string BoxInner(string text)
    {
        var pad = TotalWidth - 2 - text.Length;
        if (pad < 0) { text = text[..(TotalWidth - 5)] + "..."; pad = 3; }
        var lp = pad / 2;
        return $"│{new string(' ', lp)}{text}{new string(' ', pad - lp)}│";
    }

    // ── Баннер ───────────────────────────────────────────────────

    internal static void PrintBanner(bool quickMode, bool skipDocker, string rootDir)
    {
        try { Console.Clear(); } catch (IOException) { }
        var c = ConsoleColor.DarkCyan;
        Line(c, BoxBorder('┌', '─', '┐'));
        Line(c, BoxInner("Удобное управление контейнерами Docker"));
        Line(c, BoxInner("MS SQL Server, Web API"));
        Line(c, BoxInner("─ CifraShop ─"));
        Line(c, BoxBorder('└', '─', '┘'));
        Console.WriteLine();
        var mode = quickMode ? " * быстрый режим" : "";
        if (skipDocker) mode += " * без Docker";
        Log($"  {Path.GetFileName(rootDir)}{mode}  |  {DateTime.Now:HH:mm:ss dd.MM.yyyy}");
        Console.WriteLine();
    }

    // ── Фазы ─────────────────────────────────────────────────────

    internal static void PrintPhaseStart(int num, int total, string title)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"  [{num}/{total}] ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(title);
        Console.ResetColor();
        Console.WriteLine();
    }

    // ── Итоговый статус ──────────────────────────────────────────

    internal static async Task PrintLaunchResultAsync(string rootDir, bool dockerAvailable, bool apiInDocker, Process? apiProcess, int apiPort, Process? clientProcess, int clientPort)
    {
        Console.WriteLine();
        var c = ConsoleColor.DarkCyan;
        Line(c, BoxBorder('┌', '─', '┐'));
        Line(c, BoxInner("ВСЁ ГОТОВО К РАБОТЕ"));
        Line(c, BoxBorder('├', '─', '┤'));

        await PrintServiceStatusesAsync(rootDir, dockerAvailable, apiInDocker, apiProcess, apiPort, clientProcess, clientPort);

        Line(c, BoxBorder('└', '─', '┘'));
    }

    internal static async Task PrintServiceStatusesAsync(string rootDir, bool dockerAvailable, bool apiInDocker, Process? apiProcess, int apiPort, Process? clientProcess, int clientPort)
    {
        if (dockerAvailable)
        {
            var dbPs = await Services.CommandRunner.RunDockerComposeAsync(rootDir, "ps db --format '{{.Status}}'");
            var dbOk = dbPs != null && dbPs.Contains("Up");
            StatusRow("БД", dbOk, dbOk ? "SQL Server (Docker)" : "не запущен");
        }

        if (apiInDocker)
        {
            var apiPs = await Services.CommandRunner.RunDockerComposeAsync(rootDir, "ps api --format '{{.Status}}'");
            var apiOk = apiPs != null && (apiPs.Contains("Up") || apiPs.Contains("running"));
            StatusRow("API", apiOk, apiOk ? $"http://localhost:{apiPort} (Docker)" : "не запущен");
        }
        else
        {
            var apiOk = apiProcess is { HasExited: false };
            StatusRow("API", apiOk, apiOk ? $"https://localhost:{apiPort}" : "не запущен");
        }

        var clientOk = clientProcess is { HasExited: false };
        StatusRow("Клиент", clientOk, clientOk ? $"http://localhost:{clientPort}/admin" : "не запущен");
    }

    private static void StatusRow(string name, bool ok, string detail)
    {
        var icon = ok ? "[+] " : "[-] ";
        var tag = $"{name,-8}";
        var content = $"│ {icon}{tag}{detail}";
        var pad = TotalWidth - content.Length - 1;
        if (pad < 0) pad = 0;
        var line = content + new string(' ', pad) + "│";

        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write("│ ");
        Console.ForegroundColor = ok ? ConsoleColor.Green : ConsoleColor.Red;
        Console.Write(icon);
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(tag);
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write(detail);
        Console.ResetColor();
        Console.Write(new string(' ', pad));
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("│");
        Console.ResetColor();
    }

    internal static void PrintStatusLinePublic(string name, bool ok, string detail) => StatusRow(name, ok, detail);

    // ── Меню ─────────────────────────────────────────────────────

    private const string Indent = "  ";

    internal static void DrawMenu(int apiPort, bool apiOn, int clientPort, bool clientOn)
    {
        var c = ConsoleColor.DarkCyan;
        Console.WriteLine();
        Line(c, $"{Indent}╭─ МЕНЮ УПРАВЛЕНИЯ {new string('─', TotalWidth - 22)}╮");

        MRow("1", "Открыть админ-панель", ConsoleColor.Cyan);
        MRow("2", "Открыть главную страницу", ConsoleColor.Cyan);
        MSep();
        MRow("3", "Перезапустить API", ConsoleColor.Yellow);
        MRow("4", "Перезапустить клиент", ConsoleColor.Yellow);
        MRow("5", "Перезапустить всё", ConsoleColor.Yellow);
        MSep();
        MRow("6", "Остановить всё", ConsoleColor.Red);
        MSep();
        MRow("7", "Пересобрать API (Docker)", ConsoleColor.Magenta);
        MRow("8", "Очистить Docker-образы", ConsoleColor.DarkCyan);
        MSep();
        MRow("9", "Показать логи", ConsoleColor.Gray);
        MRow("10", "MailHog (Почтовый сервер)", ConsoleColor.Green);
        MRow("0", "Выход", ConsoleColor.DarkGray);

        Line(c, $"{Indent}╰{new string('─', TotalWidth - 4)}╯");

        Console.ForegroundColor = ConsoleColor.DarkGray;
        var apiStatus = apiOn ? " [ON]" : " [OFF]";
        var clientStatus = clientOn ? " [ON]" : " [OFF]";
        Console.WriteLine($"    API: {apiPort}{apiStatus}   Клиент: {clientPort}{clientStatus}");
        Console.ResetColor();
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("  ▸ ");
        Console.ResetColor();
    }

    private static void MRow(string key, string label, ConsoleColor color)
    {
        var inner = $"{Indent}│ [{key}] {label}";
        var pad = TotalWidth - inner.Length - 1;
        if (pad < 0) pad = 0;

        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write($"{Indent}│ ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"[{key}]");
        Console.ForegroundColor = color;
        Console.Write($" {label}");
        Console.ResetColor();
        Console.Write(new string(' ', pad));
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("│");
        Console.ResetColor();
    }

    private static void MSep()
    {
        Line(ConsoleColor.DarkCyan, $"{Indent}│{new string('─', TotalWidth - 4)}│");
    }

    internal static void ShowLogsMenu()
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("    [1] Логи API    [2] Логи клиента    [3] Статус сервисов    [0] Назад");
        Console.ResetColor();
        Console.Write("    ▸ ");
    }

    internal static void ShowFullLog(StringBuilder buffer, object lockObj, string prefix)
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
            var c = ConsoleColor.DarkCyan;
            Line(c, BoxBorder('┌', '─', '┐'));
            Line(c, BoxInner($"ЛОГИ {prefix}"));
            Line(c, BoxBorder('├', '─', '┤'));
            Console.ResetColor();
            foreach (var line in content.Split('\n').TakeLast(50))
            {
                var trimmed = line.TrimEnd();
                if (trimmed.Length > TotalWidth - 3) trimmed = trimmed[..(TotalWidth - 6)] + "...";
                var pad = TotalWidth - 3 - trimmed.Length;
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("│ " + trimmed + new string(' ', Math.Max(0, pad)) + "│");
            }
            Line(c, BoxBorder('└', '─', '┘'));
        }
    }

    internal static void ShowLastLogLines(StringBuilder buffer, object lockObj, string prefix)
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

    internal static void PressKey()
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("\n  Нажмите любую клавишу...");
        Console.ResetColor();
        try { Console.ReadKey(true); }
        catch (InvalidOperationException) { }
    }
}
