using Application.Services.Repositories;
using Core.Repositories;
using Domain.Entities;
using MediatR;

namespace Application.Features.CorporateCustomers.Queries.GetCatalog;

public class GetCorporateCustomerCatalogRequestHandler
    : IRequestHandler<GetCorporateCustomerCatalogRequest, GetCorporateCustomerCatalogResponse>
{
    private readonly ICorporateCustomerRepository _corporateCustomerRepository;

    public GetCorporateCustomerCatalogRequestHandler(
        ICorporateCustomerRepository corporateCustomerRepository)
    {
        _corporateCustomerRepository = corporateCustomerRepository;
    }

    public async Task<GetCorporateCustomerCatalogResponse> Handle(
        GetCorporateCustomerCatalogRequest request,
        CancellationToken cancellationToken)
    {
        // Catalog is built from existing (non-deleted) corporate records; new typed values appear after save.
        IPaginate<CorporateCustomer> page = await _corporateCustomerRepository.GetListAsync(
            predicate: c => !c.IsDeleted,
            index: 0,
            size: 500,
            cancellationToken: cancellationToken);

        IList<CorporateCustomer> items = page.Items;

        return new GetCorporateCustomerCatalogResponse
        {
            CompanyTypes = DistinctValues(items.Select(c => c.CompanyType)),
            CompanySectors = DistinctValues(items.Select(c => c.CompanySector)),
            CompanyIndustries = DistinctValues(items.Select(c => c.CompanyIndustry)),
            CompanySubIndustries = DistinctValues(items.Select(c => c.CompanySubIndustry))
        };
    }

    private static IReadOnlyList<string> DistinctValues(IEnumerable<string?> source) =>
        source
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
            .ToList();
}
