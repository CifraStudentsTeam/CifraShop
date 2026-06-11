var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

// Добавляем CORS-политику в контейнер зависимостей
builder.Services.AddCors(options =>
{
    options.AddPolicy("ClientCORS",
        policy =>
        {
            // Укажите точный URL, на котором работает ваш клиент (порт 5001)
            policy.WithOrigins("https://localhost:5001")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();



if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseStaticFiles();


// Используем CORS
app.UseCors("ClientCORS");

app.MapControllers();

app.Run();
