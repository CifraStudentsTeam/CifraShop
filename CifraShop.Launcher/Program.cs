using System.Text;
using CifraShop.Launcher;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

var config = LauncherConfig.Parse(args);

if (config == null)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("  [-] Не удалось найти корень проекта (CifraShop.slnx).");
    Console.WriteLine("    Убедитесь, что лаунчер запущен из папки решения.");
    Console.ResetColor();
    try { Console.ReadKey(true); } catch (InvalidOperationException) { }
    return;
}

if (config.ShowVersion)
{
    Console.WriteLine("CifraShop Launcher v2.0.0");
    return;
}

if (config.ShowStatus)
{
    using var launcher = new ProjectLauncher(config);
    await launcher.ShowStatusAsync();
    return;
}

if (config.ResetAll)
{
    using var launcher = new ProjectLauncher(config);
    await launcher.ResetAsync();
    return;
}

using var mutex = new Mutex(false, "CifraShop_Launcher_SingleInstance");
if (!mutex.WaitOne(0))
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("  [!] CifraShop Launcher уже запущен.");
    Console.WriteLine("    Закройте предыдущий экземпляр или подождите его завершения.");
    Console.ResetColor();
    try { Console.ReadKey(true); } catch (InvalidOperationException) { }
    return;
}

using var projectLauncher = new ProjectLauncher(config);
var exitTcs = new TaskCompletionSource();

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("\n  Завершение работы...");
    Console.ResetColor();
    exitTcs.TrySetResult();
};

_ = Task.Run(async () =>
{
    await exitTcs.Task;
    projectLauncher.Dispose();
    Environment.Exit(0);
});

await projectLauncher.RunAsync();
