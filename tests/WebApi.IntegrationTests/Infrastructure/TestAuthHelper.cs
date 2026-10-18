using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace WebApi.IntegrationTests.Infrastructure;

internal static class TestAuthHelper
{
    public static async Task<string> LoginAsync(HttpClient client, string email, string password)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/Auth/login",
            new { email, password });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        string? token = doc.RootElement.GetProperty("accessToken").GetProperty("token").GetString();
        token.Should().NotBeNullOrWhiteSpace();
        return token!;
    }
}
