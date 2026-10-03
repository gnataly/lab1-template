using Microsoft.EntityFrameworkCore;
using Npgsql;
using PersonService.Data;

var builder = WebApplication.CreateBuilder(args);

// Heroku задаёт динамический PORT, локально используем 8080 (совпадает с Postman-окружением).
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://+:{port}");

var connectionString = BuildConnectionString(builder.Configuration);
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddControllers();

var app = builder.Build();

// Применяем миграции при старте (работает и на Heroku, и локально).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.MapControllers();

app.Run();

static string BuildConnectionString(IConfiguration configuration)
{
    // Подключение к Postgres передаётся через переменную DATABASE_URL вида:
    // postgres://user:password@host:port/dbname
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (!string.IsNullOrWhiteSpace(databaseUrl))
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':', 2);
        var csb = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            // Если порт в URL не указан — используем стандартный 5432.
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = uri.AbsolutePath.TrimStart('/'),
            Username = userInfo[0],
            Password = userInfo.Length > 1 ? userInfo[1] : string.Empty,
            SslMode = SslMode.Require
        };
        return csb.ConnectionString;
    }

    var configured = configuration.GetConnectionString("Default");
    if (!string.IsNullOrWhiteSpace(configured))
    {
        return configured;
    }

    // Локальные значения по умолчанию из docker-compose (порт 5433, т.к. 5432 может быть занят).
    return "Host=localhost;Port=5433;Database=persons;Username=program;Password=test";
}