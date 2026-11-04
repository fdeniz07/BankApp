using Application.Modules.Credit.Features.CreditApplications.Queries.GetFacets;
using Domain.Entities;

namespace Application.Services.Search;

public interface ICustomerSearchService
{
    bool IsAvailable { get; }

    Task EnsureIndexAsync(CancellationToken cancellationToken = default);

    Task IndexAsync(CustomerSearchDocument document, CancellationToken cancellationToken = default);

    Task UpsertFromIndividualAsync(IndividualCustomer customer, CancellationToken cancellationToken = default);

    Task UpsertFromCorporateAsync(CorporateCustomer customer, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task ReindexAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns null when ES unavailable or query should fall back to SQL.</summary>
    Task<CustomerSearchPage?> SearchAsync(CustomerSearchQuery query, CancellationToken cancellationToken = default);

    Task<CustomerFacetsResponse?> GetFacetsAsync(CustomerSearchQuery query, CancellationToken cancellationToken = default);
}

public sealed class CustomerSearchQuery
{
    public string Text { get; set; } = "";
    public int PageIndex { get; set; }
    public int PageSize { get; set; } = 10;
    public string Kind { get; set; } = CustomerSearchKinds.Individual;
    public bool? IsActive { get; set; }
    public bool OnlyDeleted { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? RequiredBranchScope { get; set; }
    public bool AllowCrossBranchFilters { get; set; }
}

public sealed class CustomerSearchPage
{
    public List<CustomerSearchDocument> Items { get; set; } = [];
    public int Index { get; set; }
    public int Size { get; set; }
    public int Count { get; set; }
    public int Pages { get; set; }
}

public sealed class CustomerFacetsResponse
{
    public string Source { get; set; } = "Sql";
    public List<FacetBucketDto> Branches { get; set; } = [];
}
