using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using FluentAssertions;
using WebApi.IntegrationTests.Infrastructure;

namespace WebApi.IntegrationTests.CreditApplications;

public sealed class CustomerFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public CustomerFlowTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Customer_can_create_own_application_then_cancel_it()
    {
        (Guid customerId, Guid creditSubTypeId) = await _factory.SeedCreditCatalogAsync();
        await _factory.SeedCustomerUserAsync(customerId);

        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await TestAuthHelper.LoginAsync(client, "customer@bank.local", "Demo123!"));

        HttpResponseMessage createResponse = await client.PostAsJsonWithIdempotencyAsync(
            "/api/CreditApplications",
            new
            {
                customerId = Guid.Empty,
                creditSubTypeId,
                requestedAmount = 50_000m,
                requestedTermMonths = 24,
                monthlyIncome = 35_000m,
                purpose = "Customer self-service"
            });

        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        using JsonDocument created = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        Guid applicationId = created.RootElement.GetProperty("id").GetGuid();
        created.RootElement.GetProperty("status").GetString().Should().Be("Pending");
        created.RootElement.GetProperty("customerId").GetGuid().Should().Be(customerId);

        HttpResponseMessage cancelResponse = await client.PostAsJsonWithIdempotencyAsync(
            $"/api/CreditApplications/{applicationId}/cancel",
            new { notes = "Changed my mind" });

        cancelResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        using JsonDocument cancelled = JsonDocument.Parse(await cancelResponse.Content.ReadAsStringAsync());
        cancelled.RootElement.GetProperty("status").GetString().Should().Be("Cancelled");
    }

    [Fact]
    public async Task Customer_cannot_approve_application()
    {
        await _factory.SeedStaffUserAsync();
        (Guid customerId, Guid creditSubTypeId) = await _factory.SeedCreditCatalogAsync();
        await _factory.SeedCustomerUserAsync(customerId);

        HttpClient staffClient = _factory.CreateClient();
        staffClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await TestAuthHelper.LoginAsync(staffClient, "staff@bank.local", "Demo123!"));

        HttpResponseMessage createResponse = await staffClient.PostAsJsonWithIdempotencyAsync(
            "/api/CreditApplications",
            new
            {
                customerId,
                creditSubTypeId,
                requestedAmount = 50_000m,
                requestedTermMonths = 24,
                monthlyIncome = 40_000m,
                purpose = "For customer approve denial"
            });
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        using JsonDocument created = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        Guid applicationId = created.RootElement.GetProperty("id").GetGuid();

        HttpClient customerClient = _factory.CreateClient();
        customerClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await TestAuthHelper.LoginAsync(customerClient, "customer@bank.local", "Demo123!"));

        HttpResponseMessage approveResponse = await customerClient.PostAsJsonWithIdempotencyAsync(
            $"/api/CreditApplications/{applicationId}/approve",
            new
            {
                approvedAmount = 50_000m,
                approvedTermMonths = 24,
                approvedInterestRate = 2.5m
            });

        approveResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
