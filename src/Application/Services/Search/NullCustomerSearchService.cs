using Domain.Entities;

namespace Application.Services.Search;

public sealed class NullCustomerSearchService : ICustomerSearchService
{
    public bool IsAvailable => false;

    public Task EnsureIndexAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task IndexAsync(CustomerSearchDocument document, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task UpsertFromIndividualAsync(IndividualCustomer customer, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task UpsertFromCorporateAsync(CorporateCustomer customer, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task DeleteAsync(Guid customerId, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task ReindexAllAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task<CustomerSearchPage?> SearchAsync(CustomerSearchQuery query, CancellationToken cancellationToken = default) =>
        Task.FromResult<CustomerSearchPage?>(null);

    public Task<CustomerFacetsResponse?> GetFacetsAsync(
        CustomerSearchQuery query,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<CustomerFacetsResponse?>(null);
}
