using System.Linq.Expressions;
using Application.Services.Auth;
using Application.Services.Repositories;
using AutoMapper;
using Core.Application.Responses;
using Core.Repositories;
using Domain.Entities;
using MediatR;

namespace Application.Features.CorporateCustomers.Queries.GetList;

public class GetListCorporateCustomerRequestHandler
    : IRequestHandler<GetListCorporateCustomerRequest, GetListResponse<GetListCorporateCustomerListItemDto>>
{
    private readonly ICorporateCustomerRepository _corporateCustomerRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetListCorporateCustomerRequestHandler(
        ICorporateCustomerRepository corporateCustomerRepository,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _corporateCustomerRepository = corporateCustomerRepository;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<GetListResponse<GetListCorporateCustomerListItemDto>> Handle(
        GetListCorporateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        if (request.OnlyDeleted || request.IsActive == false)
            _currentUserService.EnsureBankStaff();

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

        if (request.OnlyDeleted)
            return c => c.IsDeleted;

        if (request.IsActive.HasValue)
            return c => c.IsActive == request.IsActive.Value;

        return null;
    }
}
