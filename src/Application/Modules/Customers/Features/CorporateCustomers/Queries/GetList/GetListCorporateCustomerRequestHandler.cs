using System.Linq.Expressions;
using Application.Services.Auth;
using Application.Services.Repositories;
using Application.Services.Search;
using AutoMapper;
using Core.Application.Responses;
using Core.Repositories;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Modules.Customers.Features.CorporateCustomers.Queries.GetList;

public class GetListCorporateCustomerRequestHandler
    : IRequestHandler<GetListCorporateCustomerRequest, GetListResponse<GetListCorporateCustomerListItemDto>>
{
    private readonly ICorporateCustomerRepository _corporateCustomerRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IBranchAuthorizationService _branchAuthorization;
    private readonly ICustomerSearchService _search;

    public GetListCorporateCustomerRequestHandler(
        ICorporateCustomerRepository corporateCustomerRepository,
        IMapper mapper,
        ICurrentUserService currentUserService,
        IBranchAuthorizationService branchAuthorization,
        ICustomerSearchService search)
    {
        _corporateCustomerRepository = corporateCustomerRepository;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _branchAuthorization = branchAuthorization;
        _search = search;
    }

    public async Task<GetListResponse<GetListCorporateCustomerListItemDto>> Handle(
        GetListCorporateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        if (request.OnlyDeleted || request.IsActive == false)
            _currentUserService.EnsureBankStaff();

        if (_branchAuthorization.MustSeeNoBranchData)
        {
            return new GetListResponse<GetListCorporateCustomerListItemDto>
            {
                Items = [],
                Index = request.PageRequest.PageIndex,
                Size = request.PageRequest.PageSize,
                Count = 0,
                Pages = 0,
                From = 0
            };
        }

        Guid? branchFilter = _branchAuthorization.ResolveListBranchFilter(request.BranchId);

        bool preferSearch = _search.IsAvailable
            && !_currentUserService.IsCustomer
            && (!string.IsNullOrWhiteSpace(request.Search) || request.BranchId.HasValue);

        if (preferSearch)
        {
            CustomerSearchPage? es = await _search.SearchAsync(
                new CustomerSearchQuery
                {
                    Text = request.Search ?? "",
                    PageIndex = request.PageRequest.PageIndex,
                    PageSize = request.PageRequest.PageSize,
                    Kind = CustomerSearchKinds.Corporate,
                    IsActive = request.IsActive,
                    OnlyDeleted = request.OnlyDeleted,
                    BranchId = request.BranchId,
                    RequiredBranchScope = _currentUserService.IsSuperAdmin ? null : branchFilter,
                    AllowCrossBranchFilters = _currentUserService.IsSuperAdmin || _currentUserService.IsBankManager
                },
                cancellationToken);

            if (es is not null)
            {
                return new GetListResponse<GetListCorporateCustomerListItemDto>
                {
                    Items = es.Items.Select(MapFromSearch).ToList(),
                    Index = es.Index,
                    Size = es.Size,
                    Count = es.Count,
                    Pages = es.Pages,
                    From = 0
                };
            }
        }

        Expression<Func<CorporateCustomer, bool>>? predicate = BuildPredicate(request, branchFilter);

        IPaginate<CorporateCustomer> corporateCustomers = await _corporateCustomerRepository.GetListAsync(
            predicate: predicate,
            include: q => q.Include(c => c.Branch),
            index: request.PageRequest.PageIndex,
            size: request.PageRequest.PageSize,
            withDeleted: request.OnlyDeleted,
            cancellationToken: cancellationToken);

        return _mapper.Map<GetListResponse<GetListCorporateCustomerListItemDto>>(corporateCustomers);
    }

    private Expression<Func<CorporateCustomer, bool>>? BuildPredicate(
        GetListCorporateCustomerRequest request,
        Guid? branchFilter)
    {
        if (_currentUserService.IsCustomer && !_currentUserService.IsBankStaff)
        {
            Guid customerId = _currentUserService.GetRequiredCustomerId();
            return c => c.Id == customerId;
        }

        string? search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim();

        if (request.OnlyDeleted)
            return c => c.IsDeleted
                && (!branchFilter.HasValue || c.BranchId == branchFilter.Value)
                && (search == null
                    || c.Email.Contains(search)
                    || c.CustomerNumber.Contains(search)
                    || c.CompanyName.Contains(search)
                    || c.TaxNumber.Contains(search)
                    || c.AuthorizedPersonName.Contains(search)
                    || c.PhoneNumber.Contains(search));

        if (request.IsActive.HasValue)
            return c =>
                c.IsActive == request.IsActive.Value
                && (!branchFilter.HasValue || c.BranchId == branchFilter.Value)
                && (search == null
                    || c.Email.Contains(search)
                    || c.CustomerNumber.Contains(search)
                    || c.CompanyName.Contains(search)
                    || c.TaxNumber.Contains(search)
                    || c.AuthorizedPersonName.Contains(search)
                    || c.PhoneNumber.Contains(search));

        if (branchFilter.HasValue || search is not null)
            return c =>
                (!branchFilter.HasValue || c.BranchId == branchFilter.Value)
                && (search == null
                    || c.Email.Contains(search)
                    || c.CustomerNumber.Contains(search)
                    || c.CompanyName.Contains(search)
                    || c.TaxNumber.Contains(search)
                    || c.AuthorizedPersonName.Contains(search)
                    || c.PhoneNumber.Contains(search));

        return null;
    }

    private static GetListCorporateCustomerListItemDto MapFromSearch(CustomerSearchDocument d) =>
        new()
        {
            Id = d.Id,
            CustomerNumber = d.CustomerNumber,
            Email = d.Email,
            PhoneNumber = d.PhoneNumber,
            CompanyName = d.CompanyName ?? "",
            TaxNumber = d.TaxNumber ?? "",
            AuthorizedPersonName = d.AuthorizedPersonName ?? "",
            IsActive = d.IsActive,
            IsDeleted = d.IsDeleted,
            BranchId = Guid.TryParse(d.BranchId, out Guid bid) ? bid : null,
            BranchCode = d.BranchCode,
            BranchName = d.BranchName
        };
}
