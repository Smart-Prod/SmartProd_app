using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SmartProd.Tests.Integration;

public sealed class TestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "IntegrationTest";
    public const string IdentityHeader = "X-Test-Identity";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var identity = Request.Headers[IdentityHeader].ToString();
        if (string.IsNullOrWhiteSpace(identity))
            return Task.FromResult(AuthenticateResult.NoResult());

        var role = identity switch
        {
            "admin" => "Admin",
            "superadmin" => "SuperAdmin",
            "member" => "User",
            _ => null
        };

        if (role is null)
            return Task.FromResult(AuthenticateResult.Fail("Unknown test identity."));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, identity == "admin" ? "1" : "2"),
            new Claim(ClaimTypes.Name, $"integration-{identity}"),
            new Claim(ClaimTypes.Role, role)
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, SchemeName));
        var ticket = new AuthenticationTicket(principal, SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
