using AspNetCoreRateLimit;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using System.Text;
using System.Threading.RateLimiting;
using UrlShortener.Api.Middleware;
using UrlShortener.Core.Entities;
using UrlShortener.Core.Interfaces;
using UrlShortener.Core.Services;
using UrlShortener.Core.Validators;
using UrlShortener.Infrastructure.BackgroundServices;
using UrlShortener.Infrastructure.Data;
using UrlShortener.Infrastructure.Extensions;
using UrlShortener.Infrastructure.Repositories;
using UrlShortener.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. إعداد الـ Logger (Serilog)
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.File("Logs/all-logs-.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.File("Logs/errors-.txt", LogEventLevel.Error, rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// 2. إعداد قاعدة البيانات و Redis
builder.Services.AddDbContextPool<UrlDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddStackExchangeRedisCache(options =>
{
    var redisConfig = builder.Configuration["Redis:Configuration"] ?? "localhost:6379";
    options.Configuration = redisConfig;
    options.InstanceName = "UrlShortener_";
});

// 3. إعداد Identity
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<UrlDbContext>()
    .AddDefaultTokenProviders();

// 4. تسجيل الخدمات (Dependency Injection) - تم التعديل لحل مشكلة الـ TestController
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUrlRepository, EfUrlRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddSingleton<IAnalyticsChannel, AnalyticsChannel>();

// تسجيل الخدمات بالكلاس والواجهة معاً لضمان التوافق
builder.Services.AddScoped<UrlService>();
builder.Services.AddScoped<IUrlService>(sp => sp.GetRequiredService<UrlService>());

builder.Services.AddScoped<AnalyticsService>();
builder.Services.AddScoped<IAnalyticsService>(sp => sp.GetRequiredService<AnalyticsService>());

// تسجيل الـ Worker (Background Service)
builder.Services.AddHostedService<AnalyticsWorker>();

// 5. إعداد الـ Authentication & JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

// 6. إعداد الـ Rate Limiting & CORS
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddDistributedRateLimiting();
builder.Services.AddSingleton<IIpPolicyStore, DistributedCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, DistributedCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter(policyName: "fixed", options =>
    {
        options.PermitLimit = 10; // عدد الطلبات المسموحة
        options.Window = TimeSpan.FromMinutes(1); // المدة الزمنية
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2; // عدد الطلبات التي تنتظر في الطابور قبل الرفض
    });
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// 7. إعداد OpenApi & Scalar
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<UrlRequestValidator>();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Servers = new List<OpenApiServer>
        {
            new OpenApiServer { Url = "http://localhost:5210" }
        };
        return Task.CompletedTask;
    });
});
var app = builder.Build();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.MapGet("/test-api", () => Results.Ok(new { Message = "API is running perfectly!" }));
// --- تشغيل الـ Middleware Pipeline ---

// 1. التحديث التلقائي لقاعدة البيانات (Migration)
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
        Console.WriteLine($"❌ Migration Error: {ex.Message}");
    }
}

app.UseSerilogRequestLogging();

// الترتيب هنا حيوي جداً لعمل الـ CORS مع الـ Scalar
app.UseCors("AllowAll");

app.UseIpRateLimiting();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();