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

var connectionString = builder.Configuration["DB_CONNECTION_STRING"]
    ?? builder.Configuration["SMARTPROD_DB_CONNECTION"]
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Configure DB_CONNECTION_STRING or ConnectionStrings:DefaultConnection.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, HttpTenantContext>();

var corsOrigins = (builder.Configuration["CORS_ORIGINS"] ?? builder.Configuration["Cors:Origins"] ?? string.Empty)
    .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(corsOrigins.Length == 0 ? ["http://localhost:5173"] : corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
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

// JWT Authentication
var jwtSecret = builder.Configuration["JWT_SECRET"]
    ?? builder.Configuration["SMARTPROD_JWT_SECRET"]
    ?? builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Configure JWT_SECRET or Jwt:Secret.");
if (Encoding.UTF8.GetByteCount(jwtSecret) < 32)
    throw new InvalidOperationException("JWT_SECRET must contain at least 32 bytes.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer not configured");

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
}

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

app.Run();

public partial class Program
{
}
