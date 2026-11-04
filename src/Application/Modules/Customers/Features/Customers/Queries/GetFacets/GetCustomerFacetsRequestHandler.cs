using Application.Modules.Credit.Features.CreditApplications.Queries.GetFacets;
using Application.Services.Auth;
using Application.Services.Repositories;
using Application.Services.Search;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Modules.Customers.Features.Customers.Queries.GetFacets;

public class GetCustomerFacetsRequestHandler
    : IRequestHandler<GetCustomerFacetsRequest, CustomerFacetsResponse>
{
    private readonly IIndividualCustomerRepository _individuals;
    private readonly ICorporateCustomerRepository _corporates;
    private readonly IBranchRepository _branches;
    private readonly ICurrentUserService _currentUser;
    private readonly IBranchAuthorizationService _branchAuthorization;
    private readonly ICustomerSearchService _search;

    public GetCustomerFacetsRequestHandler(
        IIndividualCustomerRepository individuals,
        ICorporateCustomerRepository corporates,
        IBranchRepository branches,
        ICurrentUserService currentUser,
        IBranchAuthorizationService branchAuthorization,
        ICustomerSearchService search)
    {
        _individuals = individuals;
        _corporates = corporates;
        _branches = branches;
        _currentUser = currentUser;
        _branchAuthorization = branchAuthorization;
        _search = search;
    }

    public async Task<CustomerFacetsResponse> Handle(
        GetCustomerFacetsRequest request,
        CancellationToken cancellationToken)
    {
        if (_branchAuthorization.MustSeeNoBranchData)
            return new CustomerFacetsResponse { Source = "Sql" };

        Guid? branchFilter = _branchAuthorization.ResolveListBranchFilter(request.BranchId);
        CustomerSearchQuery query = new()
        {
            Text = request.Search ?? "",
            Kind = string.Equals(request.Kind, CustomerSearchKinds.Corporate, StringComparison.OrdinalIgnoreCase)
                ? CustomerSearchKinds.Corporate
                : CustomerSearchKinds.Individual,
            IsActive = request.IsActive,
            OnlyDeleted = request.OnlyDeleted,
            BranchId = request.BranchId,
            RequiredBranchScope = _currentUser.IsSuperAdmin ? null : branchFilter,
            AllowCrossBranchFilters = _currentUser.IsSuperAdmin || _currentUser.IsBankManager,
            PageSize = 0
        };

        CustomerFacetsResponse? es = await _search.GetFacetsAsync(query, cancellationToken);
        if (es is not null)
        {
            await EnrichBranchLabelsAsync(es, cancellationToken);
            return es;
        }

        CustomerFacetsResponse sql = await BuildSqlFacetsAsync(query, branchFilter, cancellationToken);
        await EnrichBranchLabelsAsync(sql, cancellationToken);
        return sql;
    }

    private async Task<CustomerFacetsResponse> BuildSqlFacetsAsync(
        CustomerSearchQuery query,
        Guid? branchFilter,
        CancellationToken cancellationToken)
    {
        CustomerFacetsResponse result = new() { Source = "Sql" };

        if (query.Kind == CustomerSearchKinds.Corporate)
        {
            var page = await _corporates.GetListAsync(
                predicate: BuildCorporatePredicate(query, branchFilter),
                withDeleted: query.OnlyDeleted,
                index: 0,
                size: 500,
                cancellationToken: cancellationToken);
            result.Branches = page.Items
                .Where(c => c.BranchId.HasValue)
                .GroupBy(c => c.BranchId!.Value)
                .Select(g => new FacetBucketDto { Key = g.Key.ToString(), Count = g.Count() })
                .ToList();
        }
        else
        {
            var page = await _individuals.GetListAsync(
                predicate: BuildIndividualPredicate(query, branchFilter),
                withDeleted: query.OnlyDeleted,
                index: 0,
                size: 500,
                cancellationToken: cancellationToken);
            result.Branches = page.Items
                .Where(c => c.BranchId.HasValue)
                .GroupBy(c => c.BranchId!.Value)
                .Select(g => new FacetBucketDto { Key = g.Key.ToString(), Count = g.Count() })
                .ToList();
        }

        return result;
    }

    private static System.Linq.Expressions.Expression<Func<IndividualCustomer, bool>>? BuildIndividualPredicate(
        CustomerSearchQuery query,
        Guid? branchFilter)
    {
        string? search = string.IsNullOrWhiteSpace(query.Text) ? null : query.Text.Trim();
        if (query.OnlyDeleted)
            return c => c.IsDeleted
                && (!branchFilter.HasValue || c.BranchId == branchFilter.Value)
                && (search == null || c.Email.Contains(search) || c.CustomerNumber.Contains(search));
        if (query.IsActive.HasValue)
            return c => c.IsActive == query.IsActive.Value
                && (!branchFilter.HasValue || c.BranchId == branchFilter.Value)
                && (search == null || c.Email.Contains(search) || c.CustomerNumber.Contains(search));
        if (branchFilter.HasValue || search is not null)
            return c => (!branchFilter.HasValue || c.BranchId == branchFilter.Value)
                && (search == null || c.Email.Contains(search) || c.CustomerNumber.Contains(search));
        return null;
    }

    private static System.Linq.Expressions.Expression<Func<CorporateCustomer, bool>>? BuildCorporatePredicate(
        CustomerSearchQuery query,
        Guid? branchFilter)
    {
        string? search = string.IsNullOrWhiteSpace(query.Text) ? null : query.Text.Trim();
        if (query.OnlyDeleted)
            return c => c.IsDeleted
                && (!branchFilter.HasValue || c.BranchId == branchFilter.Value)
                && (search == null || c.Email.Contains(search) || c.CompanyName.Contains(search));
        if (query.IsActive.HasValue)
            return c => c.IsActive == query.IsActive.Value
                && (!branchFilter.HasValue || c.BranchId == branchFilter.Value)
                && (search == null || c.Email.Contains(search) || c.CompanyName.Contains(search));
        if (branchFilter.HasValue || search is not null)
            return c => (!branchFilter.HasValue || c.BranchId == branchFilter.Value)
                && (search == null || c.Email.Contains(search) || c.CompanyName.Contains(search));
        return null;
    }

    private async Task EnrichBranchLabelsAsync(CustomerFacetsResponse response, CancellationToken cancellationToken)
    {
        if (response.Branches.Count == 0)
            return;

        var branches = await _branches.GetListAsync(index: 0, size: 200, cancellationToken: cancellationToken);
        Dictionary<string, string> labels = branches.Items.ToDictionary(
            b => b.Id.ToString(),
            b => $"{b.Code} · {b.Name}");

        foreach (FacetBucketDto bucket in response.Branches)
        {
            if (labels.TryGetValue(bucket.Key, out string? label))
                bucket.Label = label;
        }
    }
}
