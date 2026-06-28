using System.Diagnostics;
using System.Text;

namespace CifraShop.Launcher.UI;

/// <summary>
/// Консольный UI: баннер, меню, рамки, статус-строки, логирование.
/// </summary>
internal static class ConsoleUI
{
    private const int W = 62;

    // ── Логирование ──────────────────────────────────────────────

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

    // ── Баннер ───────────────────────────────────────────────────

    internal static void PrintBanner(bool quickMode, bool skipDocker, string rootDir)
    {
        try { Console.Clear(); } catch (IOException) { }
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("┌" + new string('─', W) + "┐");
        Console.WriteLine("│" + Center("Удобное управление контейнерами Docker") + "│");
        Console.WriteLine("│" + Center("MS SQL Server, Web API") + "│");
        Console.WriteLine("│" + Center("─ CifraShop ─") + "│");
        Console.WriteLine("└" + new string('─', W) + "┘");
        Console.ResetColor();
        Console.WriteLine();
        var mode = quickMode ? " * быстрый режим" : "";
        if (skipDocker) mode += " * без Docker";
        Log($"  {Path.GetFileName(rootDir)}{mode}  |  {DateTime.Now:HH:mm:ss dd.MM.yyyy}");
        Console.WriteLine();
    }

    // ── Фазы ─────────────────────────────────────────────────────

    internal static void PrintPhaseStart(int num, int total, string title)
    {
        var phase = $"[{num}/{total}]";
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"  {phase} ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(title);
        Console.ResetColor();
        Console.WriteLine();
    }

    // ── Итоговый статус ──────────────────────────────────────────

    internal static async Task PrintLaunchResultAsync(string rootDir, bool dockerAvailable, bool apiInDocker, Process? apiProcess, int apiPort, Process? clientProcess, int clientPort)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("┌" + new string('─', W) + "┐");
        Console.WriteLine("│" + Center("ВСЁ ГОТОВО К РАБОТЕ") + "│");
        Console.WriteLine("├" + new string('─', W) + "┤");
        Console.ResetColor();

        await PrintServiceStatusesAsync(rootDir, dockerAvailable, apiInDocker, apiProcess, apiPort, clientProcess, clientPort);

        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("└" + new string('─', W) + "┘");
        Console.ResetColor();
    }

    internal static async Task PrintServiceStatusesAsync(string rootDir, bool dockerAvailable, bool apiInDocker, Process? apiProcess, int apiPort, Process? clientProcess, int clientPort)
    {
        if (dockerAvailable)
        {
            var dbPs = await Services.CommandRunner.RunDockerComposeAsync(rootDir, "ps db --format '{{.Status}}'");
            var dbOk = dbPs != null && dbPs.Contains("Up");
            PrintStatusLine("БД", dbOk, dbOk ? "SQL Server (Docker)" : "не запущен");
        }

        if (apiInDocker)
        {
            var apiPs = await Services.CommandRunner.RunDockerComposeAsync(rootDir, "ps api --format '{{.Status}}'");
            var apiOk = apiPs != null && (apiPs.Contains("Up") || apiPs.Contains("running"));
            PrintStatusLine("API", apiOk, apiOk ? $"http://localhost:{apiPort} (Docker)" : "не запущен");
        }
        else
        {
            var apiOk = apiProcess is { HasExited: false };
            PrintStatusLine("API", apiOk, apiOk ? $"https://localhost:{apiPort}" : "не запущен");
        }

        var clientOk = clientProcess is { HasExited: false };
        PrintStatusLine("Клиент", clientOk, clientOk ? $"http://localhost:{clientPort}/admin" : "не запущен");
    }

    private static void PrintStatusLine(string name, bool ok, string detail)
    {
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write("│ ");
        Console.ForegroundColor = ok ? ConsoleColor.Green : ConsoleColor.Red;
        Console.Write(ok ? "  [+] " : "  [-] ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"{name,-8}");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write(detail);
        Console.ResetColor();
        PadLine(W - 15 - detail.Length);
    }

    internal static void PrintStatusLinePublic(string name, bool ok, string detail) => PrintStatusLine(name, ok, detail);

    // ── Меню ─────────────────────────────────────────────────────

    internal static void DrawMenu(int apiPort, bool apiOn, int clientPort, bool clientOn)
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
        MenuRow("10", "MailHog (Почтовый сервер)", ConsoleColor.Green);
        MenuRow("0", "Выход", ConsoleColor.DarkGray);

        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write("  ╰");
        Console.Write(new string('─', W - 2));
        Console.WriteLine("╯");
        Console.ResetColor();

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

    // ── Хелперы отрисовки ────────────────────────────────────────

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
        Console.WriteLine("  │  " + new string('─', W - 4) + "│");
        Console.ResetColor();
    }

    private static string Center(string s)
    {
        var displayWidth = GetDisplayWidth(s);
        if (displayWidth >= W) return s[..(W - 3)] + "...";
        var pad = W - displayWidth;
        var left = pad / 2;
        return new string(' ', left) + s + new string(' ', pad - left);
    }

    private static int GetDisplayWidth(string s)
    {
        int width = 0;
        var enumerator = System.Globalization.StringInfo.GetTextElementEnumerator(s);
        while (enumerator.MoveNext())
        {
            var element = enumerator.GetTextElement();
            if (element.Length > 1 && char.IsHighSurrogate(element[0]))
                width += 2;
            else
                width += element.Length;
        }
        return width;
    }

    private static void PadLine(int width)
    {
        if (width > 0) Console.Write(new string(' ', width));
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("│");
        Console.ResetColor();
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
