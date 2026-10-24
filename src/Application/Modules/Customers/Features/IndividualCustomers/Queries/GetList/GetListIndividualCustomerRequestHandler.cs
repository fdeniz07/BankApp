using System.Linq.Expressions;
using Application.Services.Auth;
using Application.Services.Repositories;
using AutoMapper;
using Core.Application.Responses;
using Core.Repositories;
using Domain.Entities;
using MediatR;

namespace Application.Modules.Customers.Features.IndividualCustomers.Queries.GetList;

public class GetListIndividualCustomerRequestHandler
    : IRequestHandler<GetListIndividualCustomerRequest, GetListResponse<GetListIndividualCustomerListItemDto>>
{
    private readonly IIndividualCustomerRepository _individualCustomerRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetListIndividualCustomerRequestHandler(
        IIndividualCustomerRepository individualCustomerRepository,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _individualCustomerRepository = individualCustomerRepository;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<GetListResponse<GetListIndividualCustomerListItemDto>> Handle(
        GetListIndividualCustomerRequest request,
        CancellationToken cancellationToken)
    {
        if (request.OnlyDeleted || request.IsActive == false)
            _currentUserService.EnsureBankStaff();

        Expression<Func<IndividualCustomer, bool>>? predicate = BuildPredicate(request);

        IPaginate<IndividualCustomer> individualCustomers = await _individualCustomerRepository.GetListAsync(
            predicate: predicate,
            index: request.PageRequest.PageIndex,
            size: request.PageRequest.PageSize,
            withDeleted: request.OnlyDeleted,
            cancellationToken: cancellationToken);

        return _mapper.Map<GetListResponse<GetListIndividualCustomerListItemDto>>(individualCustomers);
    }

    private Expression<Func<IndividualCustomer, bool>>? BuildPredicate(GetListIndividualCustomerRequest request)
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
