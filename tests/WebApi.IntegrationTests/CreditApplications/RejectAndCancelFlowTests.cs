using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using WebApi.IntegrationTests.Infrastructure;

namespace WebApi.IntegrationTests.CreditApplications;

public sealed class RejectAndCancelFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public RejectAndCancelFlowTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Staff_can_create_pending_application_then_reject_it()
    {
        await _factory.SeedStaffUserAsync();
        (Guid customerId, Guid creditSubTypeId) = await _factory.SeedCreditCatalogAsync();

        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await TestAuthHelper.LoginAsync(client, "staff@bank.local", "Staff123!"));

        Guid applicationId = await CreatePendingAsync(client, customerId, creditSubTypeId);

        HttpResponseMessage rejectResponse = await client.PostAsJsonAsync(
            $"/api/CreditApplications/{applicationId}/reject",
            new
            {
                rejectionReason = "Insufficient income",
                decisionBy = "Integration Tester",
                notes = "Rejected in integration test"
            });

        rejectResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        using JsonDocument rejected = JsonDocument.Parse(await rejectResponse.Content.ReadAsStringAsync());
        rejected.RootElement.GetProperty("status").GetString().Should().Be("Rejected");
    }

    [Fact]
    public async Task Staff_can_cancel_pending_application()
    {
        await _factory.SeedStaffUserAsync();
        (Guid customerId, Guid creditSubTypeId) = await _factory.SeedCreditCatalogAsync();

        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await TestAuthHelper.LoginAsync(client, "staff@bank.local", "Staff123!"));

        Guid applicationId = await CreatePendingAsync(client, customerId, creditSubTypeId);

        HttpResponseMessage cancelResponse = await client.PostAsJsonAsync(
            $"/api/CreditApplications/{applicationId}/cancel",
            new { notes = "Cancelled by staff" });

        cancelResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        using JsonDocument cancelled = JsonDocument.Parse(await cancelResponse.Content.ReadAsStringAsync());
        cancelled.RootElement.GetProperty("status").GetString().Should().Be("Cancelled");
    }

    private static async Task<Guid> CreatePendingAsync(
        HttpClient client,
        Guid customerId,
        Guid creditSubTypeId)
    {
        HttpResponseMessage createResponse = await client.PostAsJsonAsync(
            "/api/CreditApplications",
            new
            {
                customerId,
                creditSubTypeId,
                requestedAmount = 50_000m,
                requestedTermMonths = 24,
                monthlyIncome = 40_000m,
                purpose = "Reject/cancel flow"
            });

        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        using JsonDocument created = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        return created.RootElement.GetProperty("id").GetGuid();
    }
}
