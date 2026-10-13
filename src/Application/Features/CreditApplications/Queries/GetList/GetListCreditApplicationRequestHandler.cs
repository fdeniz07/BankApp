using System.Linq.Expressions;
using Application.Services.Repositories;
using AutoMapper;
using Core.Application.Responses;
using Core.Repositories;
using Domain.Entities;
using MediatR;

namespace Application.Features.CreditApplications.Queries.GetList;

public class GetListCreditApplicationRequestHandler
    : IRequestHandler<GetListCreditApplicationRequest, GetListResponse<GetListCreditApplicationListItemDto>>
{
    private readonly ICreditApplicationRepository _creditApplicationRepository;
    private readonly IMapper _mapper;

    public GetListCreditApplicationRequestHandler(
        ICreditApplicationRepository creditApplicationRepository,
        IMapper mapper)
    {
        _creditApplicationRepository = creditApplicationRepository;
        _mapper = mapper;
    }

    public async Task<GetListResponse<GetListCreditApplicationListItemDto>> Handle(
        GetListCreditApplicationRequest request,
        CancellationToken cancellationToken)
    {
        Expression<Func<CreditApplication, bool>>? predicate = BuildPredicate(request);

        IPaginate<CreditApplication> creditApplications = await _creditApplicationRepository.GetListAsync(
            predicate: predicate,
            index: request.PageRequest.PageIndex,
            size: request.PageRequest.PageSize,
            cancellationToken: cancellationToken);

        return _mapper.Map<GetListResponse<GetListCreditApplicationListItemDto>>(creditApplications);
    }

    private static Expression<Func<CreditApplication, bool>>? BuildPredicate(GetListCreditApplicationRequest request)
    {
        if (request.CustomerId.HasValue && request.Status is not null)
            return c => c.CustomerId == request.CustomerId.Value && c.Status == request.Status;

        if (request.CustomerId.HasValue)
            return c => c.CustomerId == request.CustomerId.Value;

        if (request.Status is not null)
            return c => c.Status == request.Status;

        return null;
    }
}
