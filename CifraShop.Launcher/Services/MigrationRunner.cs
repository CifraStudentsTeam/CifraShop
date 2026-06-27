namespace CifraShop.Launcher.Services;

/// <summary>
/// Фаза 4: применение EF Core миграций.
/// </summary>
internal class MigrationRunner
{
    private readonly string _rootDir;

    public MigrationRunner(string rootDir)
    {
        _rootDir = rootDir;
    }

    /// <summary>
    /// Применяет миграции. Устанавливает dotnet-ef если нужно.
    /// Повторяет до 3 попыток (SQL Server может быть ещё не готов).
    /// Если модель изменилась — автоматически создаёт и применяет миграцию.
    /// </summary>
    public async Task ApplyAsync()
    {
        var infraDir = Path.Combine(_rootDir, "CifraShop.Infrastructure");
        var apiDir = Path.Combine(_rootDir, "CifraShop.API");

        var efList = await CommandRunner.RunAsync("dotnet", "tool list -g");
        if (efList != null && !efList.Contains("dotnet-ef"))
        {
            UI.ConsoleUI.Log("    Установка dotnet-ef...");
            var installResult = await CommandRunner.RunAsync("dotnet", "tool install --global dotnet-ef --version 10.0.*");
            if (installResult == null)
                UI.ConsoleUI.Warn("Не удалось установить dotnet-ef. Миграции могут потребовать ручной установки.");
        }

        var startTime = DateTime.UtcNow;

        for (int attempt = 1; attempt <= 3; attempt++)
        {
            UI.ConsoleUI.Log($"    Применение миграций (попытка {attempt}/3)...");
            var (output, exitCode) = await CommandRunner.RunWithOutputAsync("dotnet",
                $"ef database update --project \"{infraDir}\" --startup-project \"{apiDir}\"");

            if (exitCode != 0)
            {
                var result = output ?? "";
                if (result.Contains("connection", StringComparison.OrdinalIgnoreCase) ||
                    result.Contains("connect", StringComparison.OrdinalIgnoreCase) ||
                    result.Contains("timeout", StringComparison.OrdinalIgnoreCase) ||
                    result.Contains("login failed", StringComparison.OrdinalIgnoreCase) ||
                    result.Contains("cannot open database", StringComparison.OrdinalIgnoreCase))
                {
                    UI.ConsoleUI.Warn($"    SQL Server ещё не готов (попытка {attempt}/3)...");
                    if (attempt < 3) await Task.Delay(5000);
                    continue;
                }

                if (attempt < 3) { UI.ConsoleUI.Warn($"    Попытка {attempt} не удалась, повтор через 5 сек..."); await Task.Delay(5000); continue; }
                UI.ConsoleUI.Warn("Не удалось применить миграции:");
                if (!string.IsNullOrWhiteSpace(result))
                    foreach (var line in result.Split('\n'))
                        if (line.Contains("error", StringComparison.OrdinalIgnoreCase) ||
                            line.Contains("Error"))
                            UI.ConsoleUI.Log($"    {line.Trim()}");
                return;
            }

            var resultText = output ?? "";
            var lowerResult = resultText.ToLowerInvariant();

            if (resultText.Contains("Done") ||
                lowerResult.Contains("no migrations were applied") ||
                lowerResult.Contains("no runtime changes are necessary") ||
                lowerResult.Contains("already up to date"))
            {
                var elapsed = DateTime.UtcNow - startTime;
                if (lowerResult.Contains("no migrations were applied"))
                    UI.ConsoleUI.Ok($"БД уже актуальна ({elapsed.TotalSeconds:F1}с)");
                else
                    UI.ConsoleUI.Ok($"БД обновлена ({elapsed.TotalSeconds:F1}с)");
                return;
            }

            if (lowerResult.Contains("pending model changes"))
            {
                var migrationName = $"AutoSync_{DateTime.UtcNow:yyyyMMddHHmmss}";
                UI.ConsoleUI.Log("    Есть несохранённые изменения модели. Создаю миграцию...");
                var (addOutput, addExitCode) = await CommandRunner.RunWithOutputAsync("dotnet",
                    $"ef migrations add {migrationName} --project \"{infraDir}\" --startup-project \"{apiDir}\"");
                if (addExitCode == 0)
                {
                    UI.ConsoleUI.Log("    Применение новой миграции...");
                    var (updateOutput, updateExitCode) = await CommandRunner.RunWithOutputAsync("dotnet",
                        $"ef database update --project \"{infraDir}\" --startup-project \"{apiDir}\"");
                    if (updateExitCode == 0)
                    {
                        var elapsed = DateTime.UtcNow - startTime;
                        UI.ConsoleUI.Ok($"БД синхронизирована (авто-миграция, {elapsed.TotalSeconds:F1}с)");
                    }
                    else { UI.ConsoleUI.Warn("Миграция создана, но применение не удалось:"); UI.ConsoleUI.Log($"    {updateOutput}"); }
                }
                else { UI.ConsoleUI.Warn("Не удалось создать миграцию:"); UI.ConsoleUI.Log($"    {addOutput}"); }
                return;
            }

            if (exitCode == 0)
            {
                var elapsed = DateTime.UtcNow - startTime;
                UI.ConsoleUI.Ok($"БД обновлена ({elapsed.TotalSeconds:F1}с)");
                return;
            }

            UI.ConsoleUI.Warn("Не удалось применить миграции:");
            if (!string.IsNullOrWhiteSpace(resultText))
                foreach (var line in resultText.Split('\n'))
                    if (line.Contains("error", StringComparison.OrdinalIgnoreCase) ||
                        line.Contains("Error"))
                        UI.ConsoleUI.Log($"    {line.Trim()}");
            return;
        }
    }
}
