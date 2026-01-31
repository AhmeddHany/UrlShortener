
using AspNetCoreRateLimit;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;

using Serilog.Events;

using UrlShortener.Core.Interfaces;

using UrlShortener.Core.Services;

using UrlShortener.Infrastructure.Data;
using UrlShortener.Infrastructure.Extensions;
using UrlShortener.Infrastructure.Repositories;



var builder = WebApplication.CreateBuilder(args);



// 1. إعداد الـ Logger (Serilog Only)

Log.Logger = new LoggerConfiguration()

    .ReadFrom.Configuration(builder.Configuration)

    .Enrich.FromLogContext()

    .WriteTo.File("Logs/all-logs-.txt",

        rollingInterval: RollingInterval.Day,

        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")

    .WriteTo.File("Logs/errors-.txt", LogEventLevel.Error, rollingInterval: RollingInterval.Day)

    .CreateLogger();



builder.Host.UseSerilog();



// 2. إعداد الـ Database والـ Services

builder.Services.AddDbContextPool<UrlDbContext>(options =>

    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddStackExchangeRedisCache(options =>
{
    // بنقول له: لو لقيت إعدادات في الـ Environment (بتاعة الدوكر) خدها، لو ملقتش خليك localhost
    var redisConfig = builder.Configuration["Redis:Configuration"] ?? "localhost:6379";
    options.Configuration = redisConfig;
    options.InstanceName = "UrlShortener_";
});

// 4. إعداد الـ Rate Limiting (تأمين السيستم)

builder.Services.AddMemoryCache(); // مطلوب للـ Rate Limit

builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));

builder.Services.AddDistributedRateLimiting();

builder.Services.AddSingleton<IIpPolicyStore, DistributedCacheIpPolicyStore>();

builder.Services.AddSingleton<IRateLimitCounterStore, DistributedCacheRateLimitCounterStore>();

builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

builder.Services.AddScoped<IUrlRepository, EfUrlRepository>();

builder.Services.AddScoped<UrlService>();



builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddProblemDetails();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        // إحنا بنجبر الـ UI يكلم العنوان اللي المتصفح شايفه
        document.Servers = new List<Microsoft.OpenApi.Models.OpenApiServer>
        {
            new Microsoft.OpenApi.Models.OpenApiServer { Url = "http://localhost:5000" }
        };
        return Task.CompletedTask;
    });
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();



var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<UrlDbContext>();

        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();
        }
        Console.WriteLine("✅ Database Migration completed successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ An error occurred while migrating the database: {ex.Message}");
    }
}
app.UseIpRateLimiting();

app.UseExceptionHandler();

app.UseSerilogRequestLogging();



if (app.Environment.IsDevelopment())

{

    app.MapOpenApi();
    app.MapScalarApiReference(); // ✅ ضيف السطر ده عشان يطلع الـ UI
}



app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();



app.Run();
