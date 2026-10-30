using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using FluentAssertions;
using WebApi.IntegrationTests.Infrastructure;

namespace WebApi.IntegrationTests.CreditApplications;

public sealed class CreateAndApproveFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public CreateAndApproveFlowTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Staff_can_create_pending_application_then_approve_it()
    {
        await _factory.SeedStaffUserAsync();
        (Guid customerId, Guid creditSubTypeId) = await _factory.SeedCreditCatalogAsync();

        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await TestAuthHelper.LoginAsync(client, "staff@bank.local", "Demo123!"));

        const decimal amount = 50_000m;
        const int termMonths = 24;

        HttpResponseMessage createResponse = await client.PostAsJsonWithIdempotencyAsync(
            "/api/CreditApplications",
            new
            {
                customerId,
                creditSubTypeId,
                requestedAmount = amount,
                requestedTermMonths = termMonths,
                monthlyIncome = 40_000m,
                existingDebtAmount = 0m,
                purpose = "Integration test",
                notes = (string?)null
            });

        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        using JsonDocument created = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        Guid applicationId = created.RootElement.GetProperty("id").GetGuid();
        created.RootElement.GetProperty("status").GetString().Should().Be("Pending");

        HttpResponseMessage approveResponse = await client.PostAsJsonWithIdempotencyAsync(
            $"/api/CreditApplications/{applicationId}/approve",
            new
            {
                approvedAmount = amount,
                approvedTermMonths = termMonths,
                approvedInterestRate = 2.5m,
                decisionBy = "Integration Tester",
                notes = "Approved in integration test"
            });

        approveResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        using JsonDocument approved = JsonDocument.Parse(await approveResponse.Content.ReadAsStringAsync());
        approved.RootElement.GetProperty("status").GetString().Should().Be("Approved");
        approved.RootElement.GetProperty("approvedAmount").GetDecimal().Should().Be(amount);
        approved.RootElement.GetProperty("approvedTermMonths").GetInt32().Should().Be(termMonths);
    }

    [Fact]
    public async Task Create_without_token_is_unauthorized()
    {
        (Guid customerId, Guid creditSubTypeId) = await _factory.SeedCreditCatalogAsync();
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonWithIdempotencyAsync(
            "/api/CreditApplications",
            new
            {
                customerId,
                creditSubTypeId,
                requestedAmount = 50_000m,
                requestedTermMonths = 24
            });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
