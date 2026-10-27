using System.Text.Json;
using Application.Services.Messaging;
using FluentAssertions;

namespace Application.UnitTests.Services.Messaging;

public class DecisionLetterRequestedTests
{
    private static readonly JsonSerializerOptions WebJson = new(JsonSerializerDefaults.Web);

    [Fact]
    public void Serializes_with_camelCase_properties_for_queue_consumer()
    {
        DecisionLetterRequested message = new(
            ToEmail: "customer@bank.local",
            CustomerName: "Ada Customer",
            ApplicationNumber: "APP-100",
            Decision: "Approved",
            Subject: "BankApp — Application APP-100 approved",
            Body: "Dear customer, your loan was approved.",
            OccurredOnUtc: new DateTimeOffset(2026, 10, 27, 12, 0, 0, TimeSpan.Zero));

        string json = JsonSerializer.Serialize(message, WebJson);

        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;

        root.GetProperty("toEmail").GetString().Should().Be("customer@bank.local");
        root.GetProperty("customerName").GetString().Should().Be("Ada Customer");
        root.GetProperty("applicationNumber").GetString().Should().Be("APP-100");
        root.GetProperty("decision").GetString().Should().Be("Approved");
        root.GetProperty("subject").GetString().Should().Contain("approved");
        root.GetProperty("body").GetString().Should().Contain("approved");
        root.GetProperty("occurredOnUtc").GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Roundtrips_through_json()
    {
        DecisionLetterRequested original = new(
            "a@b.c",
            null,
            "APP-1",
            "Rejected",
            "Subject",
            "Body text",
            DateTimeOffset.UtcNow);

        string json = JsonSerializer.Serialize(original, WebJson);
        DecisionLetterRequested? restored =
            JsonSerializer.Deserialize<DecisionLetterRequested>(json, WebJson);

        restored.Should().NotBeNull();
        restored!.ToEmail.Should().Be(original.ToEmail);
        restored.Decision.Should().Be("Rejected");
        restored.Body.Should().Be("Body text");
        restored.CustomerName.Should().BeNull();
    }
}
