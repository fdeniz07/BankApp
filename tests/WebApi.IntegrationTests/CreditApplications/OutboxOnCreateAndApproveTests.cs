using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Domain.Events;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Contexts;
using WebApi.IntegrationTests.Infrastructure;

namespace WebApi.IntegrationTests.CreditApplications;

public sealed class OutboxOnCreateAndApproveTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public OutboxOnCreateAndApproveTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Create_and_approve_write_outbox_messages()
    {
        await _factory.SeedStaffUserAsync();
        (Guid customerId, Guid creditSubTypeId) = await _factory.SeedCreditCatalogAsync();

        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await TestAuthHelper.LoginAsync(client, "staff@bank.local", "Staff123!"));

        const decimal amount = 50_000m;
        const int termMonths = 24;

        HttpResponseMessage createResponse = await client.PostAsJsonAsync(
            "/api/CreditApplications",
            new
            {
                customerId,
                creditSubTypeId,
                requestedAmount = amount,
                requestedTermMonths = termMonths,
                monthlyIncome = 40_000m,
                existingDebtAmount = 0m,
                purpose = "Outbox test",
                notes = (string?)null
            });

        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        using JsonDocument created = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        Guid applicationId = created.RootElement.GetProperty("id").GetGuid();

        HttpResponseMessage approveResponse = await client.PostAsJsonAsync(
            $"/api/CreditApplications/{applicationId}/approve",
            new
            {
                approvedAmount = amount,
                approvedTermMonths = termMonths,
                approvedInterestRate = 2.5m,
                decisionBy = "Outbox Tester",
                notes = "Outbox approve"
            });

        approveResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using IServiceScope scope = _factory.Services.CreateScope();
        BankingDbContext db = scope.ServiceProvider.GetRequiredService<BankingDbContext>();

        string createdType = typeof(CreditApplicationCreated).FullName!;
        string approvedType = typeof(CreditApplicationApproved).FullName!;

        (await db.OutboxMessages.CountAsync(m => m.Type == createdType)).Should().BeGreaterThanOrEqualTo(1);
        (await db.OutboxMessages.CountAsync(m => m.Type == approvedType)).Should().BeGreaterThanOrEqualTo(1);

        var approvedMessage = await db.OutboxMessages
            .Where(m => m.Type == approvedType)
            .OrderByDescending(m => m.OccurredOnUtc)
            .FirstAsync();

        approvedMessage.ProcessedOnUtc.Should().BeNull();
        approvedMessage.Payload.Should().Contain(applicationId.ToString());
        approvedMessage.Payload.Should().Contain("approvedAmount");
    }
}
