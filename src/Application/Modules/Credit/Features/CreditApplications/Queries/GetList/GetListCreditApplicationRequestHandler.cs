using System.Linq.Expressions;
using Application.Services.Auth;
using Application.Services.Repositories;
using AutoMapper;
using Core.Application.Responses;
using Core.Repositories;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Modules.Credit.Features.CreditApplications.Queries.GetList;

public class GetListCreditApplicationRequestHandler
    : IRequestHandler<GetListCreditApplicationRequest, GetListResponse<GetListCreditApplicationListItemDto>>
{
    private readonly ICreditApplicationRepository _creditApplicationRepository;
    private readonly ICreditSubTypeRepository _creditSubTypeRepository;
    private readonly ICreditTypeRepository _creditTypeRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetListCreditApplicationRequestHandler(
        ICreditApplicationRepository creditApplicationRepository,
        ICreditSubTypeRepository creditSubTypeRepository,
        ICreditTypeRepository creditTypeRepository,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _creditApplicationRepository = creditApplicationRepository;
        _creditSubTypeRepository = creditSubTypeRepository;
        _creditTypeRepository = creditTypeRepository;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<GetListResponse<GetListCreditApplicationListItemDto>> Handle(
        GetListCreditApplicationRequest request,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.IsCustomer && !_currentUserService.IsBankStaff)
            request.CustomerId = _currentUserService.GetRequiredCustomerId();

        Expression<Func<CreditApplication, bool>>? predicate = BuildPredicate(request);
        Func<IQueryable<CreditApplication>, IOrderedQueryable<CreditApplication>>? orderBy =
            BuildOrderBy(request.SortBy, request.SortDesc);

        IPaginate<CreditApplication> creditApplications = await _creditApplicationRepository.GetListAsync(
            predicate: predicate,
            orderBy: orderBy,
            include: q => q.Include(c => c.CreditSubType),
            index: request.PageRequest.PageIndex,
            size: request.PageRequest.PageSize,
            cancellationToken: cancellationToken);

        GetListResponse<GetListCreditApplicationListItemDto> response =
            _mapper.Map<GetListResponse<GetListCreditApplicationListItemDto>>(creditApplications);

        await PopulateEnrichmentAsync(response.Items, creditApplications.Items, cancellationToken);
        return response;
    }

    private async Task PopulateEnrichmentAsync(
        IList<GetListCreditApplicationListItemDto> items,
        IList<CreditApplication> entities,
        CancellationToken cancellationToken)
    {
        if (items.Count == 0)
            return;

        Dictionary<Guid, string> subTypeNameById = entities
            .Where(e => e.CreditSubType is not null)
            .GroupBy(e => e.CreditSubTypeId)
            .ToDictionary(g => g.Key, g => g.First().CreditSubType.Name);

        HashSet<Guid> missingNameIds = items
            .Where(i => !subTypeNameById.ContainsKey(i.CreditSubTypeId))
            .Select(i => i.CreditSubTypeId)
            .ToHashSet();

        if (missingNameIds.Count > 0)
        {
            IPaginate<CreditSubType> missingSubTypes = await _creditSubTypeRepository.GetListAsync(
                predicate: s => missingNameIds.Contains(s.Id),
                size: Math.Max(missingNameIds.Count, 1),
                cancellationToken: cancellationToken);

            foreach (CreditSubType subType in missingSubTypes.Items)
                subTypeNameById[subType.Id] = subType.Name;
        }

        HashSet<Guid> subTypeIds = items.Select(i => i.CreditSubTypeId).ToHashSet();
        IPaginate<CreditSubType> subTypes = await _creditSubTypeRepository.GetListAsync(
            predicate: s => subTypeIds.Contains(s.Id),
            size: Math.Max(subTypeIds.Count, 1),
            cancellationToken: cancellationToken);

        HashSet<Guid> typeIds = subTypes.Items.Select(s => s.CreditTypeId).ToHashSet();
        IPaginate<CreditType> creditTypes = await _creditTypeRepository.GetListAsync(
            predicate: t => typeIds.Contains(t.Id),
            size: Math.Max(typeIds.Count, 1),
            cancellationToken: cancellationToken);

        Dictionary<Guid, string> currencyBySubTypeId = subTypes.Items
            .Join(
                creditTypes.Items,
                s => s.CreditTypeId,
                t => t.Id,
                (s, t) => new { s.Id, t.CurrencyCode })
            .ToDictionary(x => x.Id, x => string.IsNullOrWhiteSpace(x.CurrencyCode) ? "USD" : x.CurrencyCode);

        Dictionary<Guid, decimal> interestBySubTypeId = subTypes.Items
            .ToDictionary(s => s.Id, s => s.InterestRate);

        Dictionary<Guid, decimal?> approvedInterestById = entities
            .ToDictionary(e => e.Id, e => e.ApprovedInterestRate);

        foreach (GetListCreditApplicationListItemDto item in items)
        {
            item.CreditSubTypeName = subTypeNameById.TryGetValue(item.CreditSubTypeId, out string? name)
                ? name
                : string.Empty;
            item.CurrencyCode = currencyBySubTypeId.TryGetValue(item.CreditSubTypeId, out string? code)
                ? code
                : "USD";
            item.InterestRate = interestBySubTypeId.TryGetValue(item.CreditSubTypeId, out decimal rate)
                ? rate
                : 0m;
            item.ApprovedInterestRate = approvedInterestById.TryGetValue(item.Id, out decimal? approvedRate)
                ? approvedRate
                : null;
        }
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

    private static Func<IQueryable<CreditApplication>, IOrderedQueryable<CreditApplication>>? BuildOrderBy(
        string? sortBy,
        bool sortDesc)
    {
        string key = (sortBy ?? "applicationDate").Trim().ToLowerInvariant();

        return key switch
        {
            "applicationnumber" or "number" => sortDesc
                ? q => q.OrderByDescending(c => c.ApplicationNumber)
                : q => q.OrderBy(c => c.ApplicationNumber),
            "requestedamount" or "amount" => sortDesc
                ? q => q.OrderByDescending(c => c.RequestedAmount)
                : q => q.OrderBy(c => c.RequestedAmount),
            "requestedtermmonths" or "term" => sortDesc
                ? q => q.OrderByDescending(c => c.RequestedTermMonths)
                : q => q.OrderBy(c => c.RequestedTermMonths),
            "status" => sortDesc
                ? q => q.OrderByDescending(c => c.Status)
                : q => q.OrderBy(c => c.Status),
            "creditsubtypename" or "creditsubtype" or "subtype" => sortDesc
                ? q => q.OrderByDescending(c => c.CreditSubType.Name)
                : q => q.OrderBy(c => c.CreditSubType.Name),
            _ => sortDesc
                ? q => q.OrderByDescending(c => c.ApplicationDate)
                : q => q.OrderBy(c => c.ApplicationDate)
        };
    }
}
