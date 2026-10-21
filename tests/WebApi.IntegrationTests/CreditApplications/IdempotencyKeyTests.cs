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

public sealed class IdempotencyKeyTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public IdempotencyKeyTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Create_without_idempotency_key_returns_400()
    {
        await _factory.SeedStaffUserAsync();
        (Guid customerId, Guid creditSubTypeId) = await _factory.SeedCreditCatalogAsync();

        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await TestAuthHelper.LoginAsync(client, "staff@bank.local", "Staff123!"));

        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/CreditApplications",
            new
            {
                customerId,
                creditSubTypeId,
                requestedAmount = 50_000m,
                requestedTermMonths = 24
            });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        string body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Idempotency-Key");
    }

    [Fact]
    public async Task Same_key_same_body_replays_without_second_outbox_created()
    {
        await _factory.SeedStaffUserAsync();
        (Guid customerId, Guid creditSubTypeId) = await _factory.SeedCreditCatalogAsync();

        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await TestAuthHelper.LoginAsync(client, "staff@bank.local", "Staff123!"));

        string key = Guid.NewGuid().ToString("N");
        var body = new
        {
            customerId,
            creditSubTypeId,
            requestedAmount = 50_000m,
            requestedTermMonths = 24,
            monthlyIncome = 40_000m,
            purpose = "Idempotency replay"
        };

        HttpResponseMessage first = await client.PostAsJsonWithIdempotencyAsync(
            "/api/CreditApplications",
            body,
            key);
        first.StatusCode.Should().Be(HttpStatusCode.OK);
        string firstJson = await first.Content.ReadAsStringAsync();
        using JsonDocument firstDoc = JsonDocument.Parse(firstJson);
        Guid applicationId = firstDoc.RootElement.GetProperty("id").GetGuid();

        HttpResponseMessage second = await client.PostAsJsonWithIdempotencyAsync(
            "/api/CreditApplications",
            body,
            key);
        second.StatusCode.Should().Be(HttpStatusCode.OK);
        string secondJson = await second.Content.ReadAsStringAsync();
        using JsonDocument secondDoc = JsonDocument.Parse(secondJson);
        secondDoc.RootElement.GetProperty("id").GetGuid().Should().Be(applicationId);

        using IServiceScope scope = _factory.Services.CreateScope();
        BankingDbContext db = scope.ServiceProvider.GetRequiredService<BankingDbContext>();
        string createdType = typeof(CreditApplicationCreated).FullName!;
        int createdCount = await db.OutboxMessages.CountAsync(m =>
            m.Type == createdType && m.Payload.Contains(applicationId.ToString()));
        createdCount.Should().Be(1);
    }

    [Fact]
    public async Task Same_key_different_body_returns_409()
    {
        await _factory.SeedStaffUserAsync();
        (Guid customerId, Guid creditSubTypeId) = await _factory.SeedCreditCatalogAsync();

        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await TestAuthHelper.LoginAsync(client, "staff@bank.local", "Staff123!"));

        string key = Guid.NewGuid().ToString("N");

        HttpResponseMessage first = await client.PostAsJsonWithIdempotencyAsync(
            "/api/CreditApplications",
            new
            {
                customerId,
                creditSubTypeId,
                requestedAmount = 50_000m,
                requestedTermMonths = 24,
                purpose = "First body"
            },
            key);
        first.StatusCode.Should().Be(HttpStatusCode.OK);

        HttpResponseMessage second = await client.PostAsJsonWithIdempotencyAsync(
            "/api/CreditApplications",
            new
            {
                customerId,
                creditSubTypeId,
                requestedAmount = 60_000m,
                requestedTermMonths = 24,
                purpose = "Different body"
            },
            key);

        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
        string body = await second.Content.ReadAsStringAsync();
        body.Should().Contain("different request payload");
    }
}
