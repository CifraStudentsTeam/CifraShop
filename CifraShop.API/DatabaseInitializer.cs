using CifraShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

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
                    await EnsureSeedPasswordsHashedAsync(db);
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

    // Сид-пароли хранились как открытый текст, а AuthService сравнивает SHA256.
    // Если пароль не похож на SHA256-хеш — значит он открытый, хешируем его.
    private static async Task EnsureSeedPasswordsHashedAsync(ApplicationContext db)
    {
        var users = await db.Users.ToListAsync();
        var changed = false;

        foreach (var user in users)
        {
            if (string.IsNullOrEmpty(user.Password)) continue;

            // SHA256-хеш всегда 44 символа base64 (с '=' на конце)
            if (user.Password.Length == 44 && user.Password.EndsWith('='))
                continue;

            user.Password = HashPassword(user.Password);
            changed = true;
            Console.WriteLine($"[API] Захеширован пароль для {user.Email}");
        }

        if (changed)
            await db.SaveChangesAsync();
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
