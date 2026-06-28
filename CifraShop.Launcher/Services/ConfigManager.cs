using System.Text.Json;

namespace CifraShop.Launcher.Services;

/// <summary>
/// Фаза 2: настройка конфигурации (docker-compose.yml, appsettings.json).
/// </summary>
internal class ConfigManager
{
    private static readonly string DockerComposeContent = """
services:
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      ACCEPT_EULA: "Y"
      MSSQL_SA_PASSWORD: "Your_strong_Password123"
      MSSQL_PID: "Express"
    ports:
      - "1433:1433"
    volumes:
      - sqlserver-data:/var/opt/mssql

  api:
    build:
      context: .
      dockerfile: CifraShop.API/Dockerfile
    ports:
      - "5000:8086"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:8086
      - ConnectionStrings__DefaultConnection=Server=db;Database=CifraShopDb;User Id=sa;Password=Your_strong_Password123;TrustServerCertificate=True;
    depends_on:
      - db

volumes:
  sqlserver-data:
""";

    /// <summary>
    /// Создаёт docker-compose.yml (если нет) и проверяет строку подключения в appsettings.json.
    /// </summary>
    public async Task EnsureAsync(string rootDir, bool dockerAvailable)
    {
        // ── docker-compose.yml ────────────────────────────────────
        var composePath = Path.Combine(rootDir, "docker-compose.yml");
        if (!File.Exists(composePath))
        {
            try
            {
                await File.WriteAllTextAsync(composePath, DockerComposeContent);
                UI.ConsoleUI.Created("docker-compose.yml создан из шаблона");
            }
            catch (IOException)
            {
                UI.ConsoleUI.Warn("Не удалось создать docker-compose.yml (файл заблокирован)");
            }
        }
        else
            UI.ConsoleUI.Ok("docker-compose.yml существует");

        // ── Строка подключения ────────────────────────────────────
        var settingsPath = Path.Combine(rootDir, "CifraShop.API", "appsettings.json");
        if (File.Exists(settingsPath))
        {
            string content;
            try
            {
                content = await File.ReadAllTextAsync(settingsPath);
            }
            catch (IOException)
            {
                UI.ConsoleUI.Warn("appsettings.json заблокирован другим процессом");
                return;
            }

            if (!content.Contains("DefaultConnection"))
            {
                var idx = content.IndexOf('{');
                if (idx >= 0)
                {
                    var insertion = """
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CifraShopDb;User Id=sa;Password=Your_strong_Password123;TrustServerCertificate=True;"
  },
""";
                    var updated = content[..(idx + 1)] + "\n" + insertion + content[(idx + 1)..];
                    try
                    {
                        await File.WriteAllTextAsync(settingsPath, updated);
                        UI.ConsoleUI.Created("Строка подключения добавлена в appsettings.json");
                    }
                    catch (IOException)
                    {
                        UI.ConsoleUI.Warn("Не удалось записать в appsettings.json (файл заблокирован)");
                    }
                }
                else
                    UI.ConsoleUI.Fail("Не удалось найти начало JSON в appsettings.json");
            }
            else
                UI.ConsoleUI.Ok("Строка подключения найдена");
        }
        else
            UI.ConsoleUI.Fail("appsettings.json не найден!");

        // ── Обнаружение изменений в коде ──────────────────────────
        if (dockerAvailable && DetectCodeChanges(rootDir))
        {
            UI.ConsoleUI.Warn("Обнаружены изменения в коде после последней сборки Docker");
            UI.ConsoleUI.Log("    Рекомендуется пересборка образа (пункт [7] в меню)");
        }
    }

    /// <summary>
    /// Обнаруживает изменения в исходниках после последней Docker-сборки.
    /// Универсально: сканирует ВСЕ подкаталоги решения (не хардкодит имена проектов).
    /// </summary>
    internal static bool DetectCodeChanges(string rootDir)
    {
        var markerFile = Path.Combine(rootDir, ".last-docker-build");
        if (!File.Exists(markerFile)) return false;

        string content;
        try
        {
            content = File.ReadAllText(markerFile).Trim();
        }
        catch (IOException) { return false; }

        if (!DateTime.TryParse(content, out var lastBuild)) return false;

        var extensions = new[] { "*.cs", "*.csproj", "*.razor", "*.json", "*.css", "*.js" };
        var excludeDirs = new[] { "bin", "obj", ".git", "node_modules", ".vs", "CifraShop.Launcher", "CifraShop.Tests" };

        try
        {
            foreach (var ext in extensions)
            {
                foreach (var file in Directory.EnumerateFiles(rootDir, ext, SearchOption.AllDirectories))
                {
                    var relativePath = Path.GetRelativePath(rootDir, file);
                    if (excludeDirs.Any(d => relativePath.StartsWith(d, StringComparison.OrdinalIgnoreCase)))
                        continue;

                    if (File.GetLastWriteTime(file) > lastBuild)
                        return true;
                }
            }
        }
        catch { return false; }

        return false;
    }

    /// <summary>Записывает текущее время в маркер .last-docker-build.</summary>
    internal static void UpdateDockerBuildTimestamp(string rootDir)
    {
        try
        {
            File.WriteAllText(Path.Combine(rootDir, ".last-docker-build"), DateTime.Now.ToString("o"));
        }
        catch { }
    }
}
