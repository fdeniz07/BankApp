using System.Linq.Expressions;
using Application.Modules.Credit.Features.CreditApplicationDocuments;
using Application.Services.Auth;
using Application.Services.Repositories;
using AutoMapper;
using Core.Application.Responses;
using Core.Repositories;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Modules.Credit.Features.CreditApplications.Queries.GetList;

public class GetListCreditApplicationRequestHandler
    : IRequestHandler<GetListCreditApplicationRequest, GetListResponse<GetListCreditApplicationListItemDto>>
{
    private readonly ICreditApplicationRepository _creditApplicationRepository;
    private readonly ICreditSubTypeRepository _creditSubTypeRepository;
    private readonly ICreditTypeRepository _creditTypeRepository;
    private readonly ICreditApplicationDocumentRepository _documentRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IBranchAuthorizationService _branchAuthorization;

    public GetListCreditApplicationRequestHandler(
        ICreditApplicationRepository creditApplicationRepository,
        ICreditSubTypeRepository creditSubTypeRepository,
        ICreditTypeRepository creditTypeRepository,
        ICreditApplicationDocumentRepository documentRepository,
        IMapper mapper,
        ICurrentUserService currentUserService,
        IBranchAuthorizationService branchAuthorization)
    {
        _creditApplicationRepository = creditApplicationRepository;
        _creditSubTypeRepository = creditSubTypeRepository;
        _creditTypeRepository = creditTypeRepository;
        _documentRepository = documentRepository;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _branchAuthorization = branchAuthorization;
    }

    public async Task<GetListResponse<GetListCreditApplicationListItemDto>> Handle(
        GetListCreditApplicationRequest request,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.IsCustomer && !_currentUserService.IsBankStaff)
            request.CustomerId = _currentUserService.GetRequiredCustomerId();

        if (_branchAuthorization.MustSeeNoBranchData)
        {
            return new GetListResponse<GetListCreditApplicationListItemDto>
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
        HashSet<Guid>? missingDocOnlyIds = null;
        if (request.MissingRequiredDocumentsOnly == true)
        {
            missingDocOnlyIds = await ResolveMissingRequiredDocumentApplicationIdsAsync(
                request,
                branchFilter,
                cancellationToken);
            if (missingDocOnlyIds.Count == 0)
            {
                return new GetListResponse<GetListCreditApplicationListItemDto>
                {
                    Items = [],
                    Index = request.PageRequest.PageIndex,
                    Size = request.PageRequest.PageSize,
                    Count = 0,
                    Pages = 0,
                    From = 0
                };
            }
        }

        Expression<Func<CreditApplication, bool>>? predicate = BuildPredicate(request, branchFilter, missingDocOnlyIds);
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

        HashSet<Guid> pendingIds = entities
            .Where(e => e.Status == CreditApplicationStatus.Pending)
            .Select(e => e.Id)
            .ToHashSet();

        Dictionary<Guid, List<string>> docTypesByAppId = [];
        if (pendingIds.Count > 0)
        {
            IPaginate<CreditApplicationDocument> docs = await _documentRepository.GetListAsync(
                predicate: d => pendingIds.Contains(d.CreditApplicationId) && !d.IsDeleted,
                size: Math.Max(pendingIds.Count * 10, 50),
                cancellationToken: cancellationToken);

            docTypesByAppId = docs.Items
                .GroupBy(d => d.CreditApplicationId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(d => d.DocumentType.Name).ToList());
        }

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

            CreditApplication? entity = entities.FirstOrDefault(e => e.Id == item.Id);
            if (entity is not null)
            {
                item.DecisionBy = entity.DecisionBy;
                item.DecisionBranchCode = entity.DecisionBranchCode;
                item.DecisionBranchName = entity.DecisionBranchName;
                item.DecisionTemplateName = entity.DecisionTemplateName;
            }

            if (item.Status == CreditApplicationStatus.Pending)
            {
                IEnumerable<string> types = docTypesByAppId.TryGetValue(item.Id, out List<string>? list)
                    ? list
                    : [];
                item.MissingRequiredDocuments = RequiredCreditDocuments.IsMissingRequired(types);
            }
        }
    }

    private async Task<HashSet<Guid>> ResolveMissingRequiredDocumentApplicationIdsAsync(
        GetListCreditApplicationRequest request,
        Guid? branchFilter,
        CancellationToken cancellationToken)
    {
        IPaginate<CreditApplication> pending = await _creditApplicationRepository.GetListAsync(
            predicate: c =>
                c.Status == CreditApplicationStatus.Pending
                && (!request.CustomerId.HasValue || c.CustomerId == request.CustomerId.Value)
                && (!branchFilter.HasValue || c.BranchId == branchFilter.Value),
            size: 500,
            cancellationToken: cancellationToken);

        if (pending.Items.Count == 0)
            return [];

        HashSet<Guid> pendingIds = pending.Items.Select(p => p.Id).ToHashSet();
        IPaginate<CreditApplicationDocument> docs = await _documentRepository.GetListAsync(
            predicate: d => pendingIds.Contains(d.CreditApplicationId) && !d.IsDeleted,
            size: Math.Max(pendingIds.Count * 10, 50),
            cancellationToken: cancellationToken);

        Dictionary<Guid, List<string>> byApp = docs.Items
            .GroupBy(d => d.CreditApplicationId)
            .ToDictionary(g => g.Key, g => g.Select(d => d.DocumentType.Name).ToList());

        return pendingIds
            .Where(id =>
                RequiredCreditDocuments.IsMissingRequired(
                    byApp.TryGetValue(id, out List<string>? types) ? types : []))
            .ToHashSet();
    }

    private static Expression<Func<CreditApplication, bool>>? BuildPredicate(
        GetListCreditApplicationRequest request,
        Guid? branchFilter,
        HashSet<Guid>? missingDocOnlyIds)
    {
        return c =>
            (!request.CustomerId.HasValue || c.CustomerId == request.CustomerId.Value)
            && (request.Status == null || c.Status == request.Status)
            && (!branchFilter.HasValue || c.BranchId == branchFilter.Value)
            && (missingDocOnlyIds == null || missingDocOnlyIds.Contains(c.Id));
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
