using System.Text.Json;
using Application.Services.Messaging;
using FluentAssertions;

namespace Application.UnitTests.Services.Messaging;

public class DecisionSmsRequestedTests
{
    private static readonly JsonSerializerOptions WebJson = new(JsonSerializerDefaults.Web);

    [Fact]
    public void Serializes_with_camelCase_properties_for_queue_consumer()
    {
        DecisionSmsRequested message = new(
            ToPhone: "+905551112233",
            CustomerName: "Ada Customer",
            ApplicationNumber: "APP-100",
            Decision: "Approved",
            Body: "BankApp: APP-100 approved.",
            OccurredOnUtc: new DateTimeOffset(2026, 10, 27, 12, 0, 0, TimeSpan.Zero));

        string json = JsonSerializer.Serialize(message, WebJson);

        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;

        root.GetProperty("toPhone").GetString().Should().Be("+905551112233");
        root.GetProperty("customerName").GetString().Should().Be("Ada Customer");
        root.GetProperty("applicationNumber").GetString().Should().Be("APP-100");
        root.GetProperty("decision").GetString().Should().Be("Approved");
        root.GetProperty("body").GetString().Should().Contain("approved");
        root.GetProperty("occurredOnUtc").GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Roundtrips_through_json()
    {
        DecisionSmsRequested original = new(
            "+491701234567",
            null,
            "APP-1",
            "Rejected",
            "BankApp: APP-1 rejected.",
            DateTimeOffset.UtcNow);

        string json = JsonSerializer.Serialize(original, WebJson);
        DecisionSmsRequested? restored =
            JsonSerializer.Deserialize<DecisionSmsRequested>(json, WebJson);

        restored.Should().NotBeNull();
        restored!.ToPhone.Should().Be(original.ToPhone);
        restored.Decision.Should().Be("Rejected");
        restored.Body.Should().Be("BankApp: APP-1 rejected.");
        restored.CustomerName.Should().BeNull();
    }
}
