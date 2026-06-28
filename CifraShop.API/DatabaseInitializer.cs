using CifraShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CifraShop.API;

public static class DatabaseInitializer
{
    public static void ApplyMigrationsAsync(WebApplication app)
    {
        _ = Task.Run(async () =>
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            for (int attempt = 1; attempt <= 15; attempt++)
            {
                try
                {
                    db.Database.Migrate();
                    Console.WriteLine("[API] БД обновлена");
                    return;
                }
                catch (Exception ex) when (attempt < 15)
                {
                    Console.WriteLine($"[API] БД недоступна (попытка {attempt}/15): {ex.Message}. Повтор через 2 сек...");
                    await Task.Delay(2000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[API] Не удалось подключиться к БД после 15 попыток: {ex.Message}");
                }
            }
        });
    }
}
