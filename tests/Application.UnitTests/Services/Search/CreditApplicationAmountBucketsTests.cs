using Application.Services.Search;
using FluentAssertions;

namespace Application.UnitTests.Services.Search;

public class CreditApplicationAmountBucketsTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(49_999, 0)]
    [InlineData(50_000, 1)]
    [InlineData(99_999, 1)]
    [InlineData(100_000, 2)]
    [InlineData(250_000, 3)]
    [InlineData(500_000, 4)]
    [InlineData(1_000_000, 4)]
    public void BucketIndex_MapsAmountToFixedLabRanges(decimal amount, int expectedIndex)
    {
        CreditApplicationAmountBuckets.BucketIndex(amount).Should().Be(expectedIndex);
    }

    [Fact]
    public void Ranges_AreContiguousLabEdges()
    {
        CreditApplicationAmountBuckets.Ranges.Should().HaveCount(5);
        CreditApplicationAmountBuckets.Ranges[0].From.Should().Be(0m);
        CreditApplicationAmountBuckets.Ranges[0].To.Should().Be(50_000m);
        CreditApplicationAmountBuckets.Ranges[^1].To.Should().BeNull();
    }
}
