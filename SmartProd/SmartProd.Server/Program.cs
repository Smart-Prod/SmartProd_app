using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Threading.RateLimiting;
using SmartProd.API.Server.Data;
using SmartProd.API.Server.Enum;
using SmartProd.API.Server.Middlewares;
using SmartProd.API.Server.Models;
using SmartProd.API.Server.Services;
using SmartProd.API.Server.Utils;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var rawConn = builder.Configuration["DATABASE_URL_UNPOOLED"]
    ?? builder.Configuration["DATABASE_URL"]
    ?? builder.Configuration["DB_CONNECTION_STRING"]
    ?? builder.Configuration["SMARTPROD_DB_CONNECTION"]
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Configure DATABASE_URL_UNPOOLED, DATABASE_URL, DB_CONNECTION_STRING or ConnectionStrings:DefaultConnection.");

string connectionString = rawConn;
if (rawConn.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
    rawConn.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
{
    var uri = new Uri(rawConn);
    var userInfo = uri.UserInfo.Split(':');
    var builderConn = new Npgsql.NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port > 0 ? uri.Port : 5432,
        Database = uri.AbsolutePath.TrimStart('/'),
        Username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : string.Empty,
        Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty,
        SslMode = Npgsql.SslMode.Require
    };
    connectionString = builderConn.ConnectionString;
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, HttpTenantContext>();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
                               Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var configuredOrigins = (builder.Configuration["CORS_ORIGINS"] ?? builder.Configuration["Cors:Origins"] ?? string.Empty)
    .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
var defaultOrigins = new[] { "http://localhost", "http://localhost:5173", "http://localhost:3000", "http://localhost:80", "http://localhost:8080" };
var allowedOrigins = configuredOrigins.Length > 0 ? configuredOrigins.Concat(defaultOrigins).Distinct().ToArray() : defaultOrigins;

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (configuredOrigins.Contains("*"))
        {
            policy.SetIsOriginAllowed(_ => true)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

builder.Services.AddControllers(options =>
    {
        options.Filters.Add<ApiResponseFilter>();
        options.Filters.Add(new AuthorizeFilter(new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build()));
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Register services
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IProdutoService, ProdutoService>();
builder.Services.AddScoped<IBomService, BomService>();
builder.Services.AddScoped<ISeedService, SeedService>();
builder.Services.AddScoped<ProductionOrderService>();
builder.Services.AddScoped<MovimentoEstoqueService>();
builder.Services.AddScoped<NotaFiscalService>();
builder.Services.AddScoped<RelatorioService>();
builder.Services.AddScoped<ReportExportService>();
builder.Services.AddScoped<SeedService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<IndustrialOperationsService>();
builder.Services.AddScoped<TenantProvisioningService>();
builder.Services.AddSignalR();

// JWT Authentication
var jwtSecret = builder.Configuration["JWT_SECRET"]
    ?? builder.Configuration["SMARTPROD_JWT_SECRET"]
    ?? builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Configure JWT_SECRET or Jwt:Secret.");
if (Encoding.UTF8.GetByteCount(jwtSecret) < 32)
    throw new InvalidOperationException("JWT_SECRET must contain at least 32 bytes.");
var jwtIssuer = builder.Configuration["JWT_ISSUER"]
    ?? builder.Configuration["Jwt:Issuer"]
    ?? "SmartProd";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

builder.AddServiceDefaults();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks().AddCheck<PostgresHealthCheck>("postgresql", tags: ["ready"]);
builder.Services.AddOpenApi();

var app = builder.Build();
app.UseForwardedHeaders();

if (builder.Configuration.GetValue("Database:ApplyMigrations", true))
{
    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
    if (!await dbContext.Usuarios.AnyAsync())
    {
        var adminEmail = builder.Configuration["ADMIN_INITIAL_EMAIL"];
        var adminPassword = builder.Configuration["ADMIN_INITIAL_PASSWORD"];
        if (string.IsNullOrWhiteSpace(adminEmail) ||
            !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(adminEmail) ||
            string.IsNullOrWhiteSpace(adminPassword) ||
            adminPassword.Length < 12)
        {
            throw new InvalidOperationException(
                "An empty database requires ADMIN_INITIAL_EMAIL and ADMIN_INITIAL_PASSWORD (at least 12 characters) to provision the first administrator.");
        }

        dbContext.Usuarios.Add(new Usuario
        {
            Name = "Administrador",
            Email = adminEmail,
            Senha = BCrypt.Net.BCrypt.HashPassword(adminPassword),
            Active = true,
            CreatedAt = DateTime.UtcNow,
            Role = UserRole.Admin
        });
        await dbContext.SaveChangesAsync();
    }

    var platformAdminEmail = builder.Configuration["PLATFORM_ADMIN_EMAIL"];
    var platformAdminPassword = builder.Configuration["PLATFORM_ADMIN_PASSWORD"];
    if (!string.IsNullOrWhiteSpace(platformAdminEmail) || !string.IsNullOrWhiteSpace(platformAdminPassword))
    {
        if (string.IsNullOrWhiteSpace(platformAdminEmail) ||
            !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(platformAdminEmail) ||
            string.IsNullOrWhiteSpace(platformAdminPassword) ||
            platformAdminPassword.Length < 12)
        {
            throw new InvalidOperationException(
                "Configure both PLATFORM_ADMIN_EMAIL and PLATFORM_ADMIN_PASSWORD (at least 12 characters) to bootstrap the platform administrator.");
        }

        if (!await dbContext.Usuarios.IgnoreQueryFilters().AnyAsync(user => user.Role == UserRole.SuperAdmin))
        {
            if (await dbContext.Usuarios.IgnoreQueryFilters().AnyAsync(user => user.Email == platformAdminEmail))
                throw new InvalidOperationException(
                    "PLATFORM_ADMIN_EMAIL is already assigned to another user. Use a new email; existing accounts are never silently promoted.");

            dbContext.Usuarios.Add(new Usuario
            {
                TenantId = 1,
                Name = "Platform Administrator",
                Email = platformAdminEmail,
                Senha = BCrypt.Net.BCrypt.HashPassword(platformAdminPassword),
                Active = true,
                CreatedAt = DateTime.UtcNow,
                Role = UserRole.SuperAdmin
            });
            await dbContext.SaveChangesAsync();
        }
    }
}

app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapDefaultEndpoints();
app.UseFileServer();
app.UseRouting();
app.UseRateLimiter();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<SmartProd.API.Server.Hubs.ProductionHub>("/hubs/production");

app.Run();

public partial class Program
{
}
