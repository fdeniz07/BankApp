using System.Linq.Expressions;
using Application.Services.Auth;
using Application.Services.Repositories;
using AutoMapper;
using Core.Application.Responses;
using Core.Repositories;
using Domain.Entities;
using MediatR;

namespace Application.Modules.Customers.Features.CorporateCustomers.Queries.GetList;

public class GetListCorporateCustomerRequestHandler
    : IRequestHandler<GetListCorporateCustomerRequest, GetListResponse<GetListCorporateCustomerListItemDto>>
{
    private readonly ICorporateCustomerRepository _corporateCustomerRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IBranchAuthorizationService _branchAuthorization;

    public GetListCorporateCustomerRequestHandler(
        ICorporateCustomerRepository corporateCustomerRepository,
        IMapper mapper,
        ICurrentUserService currentUserService,
        IBranchAuthorizationService branchAuthorization)
    {
        _corporateCustomerRepository = corporateCustomerRepository;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _branchAuthorization = branchAuthorization;
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

        Expression<Func<CorporateCustomer, bool>>? predicate = BuildPredicate(request);

        IPaginate<CorporateCustomer> corporateCustomers = await _corporateCustomerRepository.GetListAsync(
            predicate: predicate,
            index: request.PageRequest.PageIndex,
            size: request.PageRequest.PageSize,
            withDeleted: request.OnlyDeleted,
            cancellationToken: cancellationToken);

        return _mapper.Map<GetListResponse<GetListCorporateCustomerListItemDto>>(corporateCustomers);
    }

    private Expression<Func<CorporateCustomer, bool>>? BuildPredicate(GetListCorporateCustomerRequest request)
    {
        if (_currentUserService.IsCustomer && !_currentUserService.IsBankStaff)
        {
            Guid customerId = _currentUserService.GetRequiredCustomerId();
            return c => c.Id == customerId;
        }

        Guid? branchFilter = _branchAuthorization.ResolveListBranchFilter(request.BranchId);

        if (request.OnlyDeleted)
            return c => c.IsDeleted && (!branchFilter.HasValue || c.BranchId == branchFilter.Value);

        if (request.IsActive.HasValue)
            return c =>
                c.IsActive == request.IsActive.Value
                && (!branchFilter.HasValue || c.BranchId == branchFilter.Value);

        if (branchFilter.HasValue)
            return c => c.BranchId == branchFilter.Value;

        return null;
    }
}