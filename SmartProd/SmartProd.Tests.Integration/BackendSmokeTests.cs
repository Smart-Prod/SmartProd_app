using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace SmartProd.Tests.Integration;

public sealed class BackendSmokeTests(SmartProdApiFactory factory) : IClassFixture<SmartProdApiFactory>
{
    [Fact]
    public async Task ProtectedProductsEndpoint_RequiresAuthentication()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/v1/Produto");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AdminEndpoint_RejectsAuthenticatedNonAdmin()
    {
        using var client = CreateAuthenticatedClient("member");

        using var response = await client.PostAsync("/api/v1/Seed/kaixote", content: null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task TenantProvisioning_RequiresPlatformAdministrator()
    {
        using var client = CreateAuthenticatedClient("admin");
        using var response = await client.PostAsJsonAsync("/api/v1/tenants", new
        {
            name = "Factory One",
            slug = "factory-one",
            adminName = "Tenant Admin",
            adminEmail = "admin@factory-one.test",
            adminPassword = "a-strong-password-123"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task PlatformAdministrator_CanProvisionTenantAndInitialAdmin()
    {
        using var client = CreateAuthenticatedClient("superadmin");
        using var response = await client.PostAsJsonAsync("/api/v1/tenants", new
        {
            name = "Factory One",
            slug = "factory-one",
            adminName = "Tenant Admin",
            adminEmail = "admin@factory-one.test",
            adminPassword = "a-strong-password-123"
        });

        Assert.True(response.StatusCode == HttpStatusCode.Created,
            $"Expected Created, received {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
    }

    [Fact]
    public async Task MissingProduct_ReturnsNotFoundForAuthenticatedUser()
    {
        using var client = CreateAuthenticatedClient("member");

        using var response = await client.GetAsync("/api/v1/Produto/404");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task LivenessEndpoint_ReturnsHealthyInDevelopment()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/alive");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", body);
    }

    [Fact]
    public async Task ReadinessEndpoint_ReturnsHealthyWithAvailableInMemoryProvider()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private HttpClient CreateAuthenticatedClient(string identity)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthenticationHandler.IdentityHeader, identity);
        return client;
    }
}
