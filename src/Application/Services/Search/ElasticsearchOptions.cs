namespace Application.Services.Search;

public sealed class ElasticsearchOptions
{
    public const string SectionName = "Elasticsearch";

    public bool Enabled { get; set; }

    /// <summary>Node URLs, e.g. http://localhost:9200</summary>
    public string[] Nodes { get; set; } = ["http://localhost:9200"];

    public string ApplicationsIndex { get; set; } = "bankapp-credit-applications";
}
