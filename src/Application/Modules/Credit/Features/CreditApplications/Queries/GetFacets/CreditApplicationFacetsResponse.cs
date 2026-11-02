using System.Text.Json.Serialization;

namespace Application.Modules.Credit.Features.CreditApplications.Queries.GetFacets;

public sealed class CreditApplicationFacetsResponse
{
    /// <summary>Elasticsearch | Sql</summary>
    public string Source { get; set; } = "Sql";

    public List<FacetBucketDto> Statuses { get; set; } = [];
    public List<FacetBucketDto> Branches { get; set; } = [];
    public List<FacetBucketDto> Currencies { get; set; } = [];
    public FacetMissingDocsDto MissingRequiredDocuments { get; set; } = new();
    public List<AmountBucketDto> AmountBuckets { get; set; } = [];
}

public sealed class FacetBucketDto
{
    public string Key { get; set; } = "";
    public string? Label { get; set; }
    public long Count { get; set; }
}

public sealed class FacetMissingDocsDto
{
    [JsonPropertyName("true")]
    public long True { get; set; }

    [JsonPropertyName("false")]
    public long False { get; set; }
}

public sealed class AmountBucketDto
{
    public decimal From { get; set; }
    public decimal? To { get; set; }
    public long Count { get; set; }
}
