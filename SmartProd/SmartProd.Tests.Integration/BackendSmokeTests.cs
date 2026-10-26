using System.Net;
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
