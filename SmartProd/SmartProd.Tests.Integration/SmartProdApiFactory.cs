using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SmartProd.API.Server.Data;

namespace SmartProd.Tests.Integration;

public sealed class SmartProdApiFactory : WebApplicationFactory<Program>
{
    private readonly Dictionary<string, string?> _originalEnvironment = new();
    private readonly string _databaseName = $"smartprod-integration-{Guid.NewGuid():N}";

    public SmartProdApiFactory()
    {
        SetEnvironmentVariable("DB_CONNECTION_STRING", "Host=localhost;Database=smartprod-tests");
        SetEnvironmentVariable("JWT_SECRET", "integration-test-signing-key-32-bytes-minimum");
        SetEnvironmentVariable("Jwt__Issuer", "SmartProd.Tests");
        SetEnvironmentVariable("Database__ApplyMigrations", "false");
        SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureTestServices(services =>
        {
            services.AddProblemDetails();
            services.RemoveAll<AppDbContext>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(_databaseName));
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthenticationHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthenticationHandler.SchemeName;
                    options.DefaultForbidScheme = TestAuthenticationHandler.SchemeName;
                })
                .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestAuthenticationHandler>(
                    TestAuthenticationHandler.SchemeName,
                    _ => { });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            foreach (var (name, value) in _originalEnvironment)
                Environment.SetEnvironmentVariable(name, value);
        }
    }

    private void SetEnvironmentVariable(string name, string value)
    {
        _originalEnvironment[name] = Environment.GetEnvironmentVariable(name);
        Environment.SetEnvironmentVariable(name, value);
    }
}
