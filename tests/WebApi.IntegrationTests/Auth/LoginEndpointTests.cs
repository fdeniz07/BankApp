using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using WebApi.IntegrationTests.Infrastructure;

namespace WebApi.IntegrationTests.Auth;

public sealed class LoginEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public LoginEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_with_valid_credentials_returns_access_token()
    {
        await _factory.SeedStaffUserAsync();
        HttpClient client = _factory.CreateClient();

        string token = await TestAuthHelper.LoginAsync(client, "staff@bank.local", "Demo123!");
        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_with_wrong_password_returns_bad_request()
    {
        await _factory.SeedStaffUserAsync();
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/Auth/login",
            new { email = "staff@bank.local", password = "WrongPassword1!" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_with_empty_body_returns_bad_request()
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/Auth/login",
            new { email = "", password = "" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
