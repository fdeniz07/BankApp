using Application.Modules.Identity.Features.StaffUsers.Queries.GetList;
using Application.Services.Repositories;
using Application.Services.Search;
using Core.Application.Responses;
using Core.Security.Constants;
using Domain.Entities;
using Domain.Enums;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Mapping;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace WebApi.Modules.Identity.Search;

public sealed class ElasticStaffUserSearchService : IStaffUserSearchService
{
    private readonly ElasticsearchClient _client;
    private readonly ElasticsearchOptions _options;
    private readonly IApplicationUserRepository _users;
    private readonly IBranchRepository _branches;
    private readonly ILogger<ElasticStaffUserSearchService> _logger;

    public ElasticStaffUserSearchService(
        ElasticsearchClient client,
        IOptions<ElasticsearchOptions> options,
        IApplicationUserRepository users,
        IBranchRepository branches,
        ILogger<ElasticStaffUserSearchService> logger)
    {
        _client = client;
        _options = options.Value;
        _users = users;
        _branches = branches;
        _logger = logger;
    }

    public bool IsAvailable => _options.Enabled;

    private string IndexName => _options.StaffIndex;

    public async Task EnsureIndexAsync(CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
            return;

        var exists = await _client.Indices.ExistsAsync(IndexName, cancellationToken);
        if (exists.Exists)
            return;

        var created = await _client.Indices.CreateAsync(IndexName, c => c
            .Mappings(m => m
                .Properties(new Properties
                {
                    { "id", new KeywordProperty() },
                    { "email", new TextProperty { Fields = new Properties { { "keyword", new KeywordProperty() } } } },
                    { "firstName", new TextProperty { Fields = new Properties { { "keyword", new KeywordProperty() } } } },
                    { "lastName", new TextProperty { Fields = new Properties { { "keyword", new KeywordProperty() } } } },
                    { "title", new KeywordProperty() },
                    { "gender", new KeywordProperty() },
                    { "dateOfBirth", new KeywordProperty() },
                    { "position", new KeywordProperty() },
                    { "branchId", new KeywordProperty() },
                    { "branchCode", new KeywordProperty() },
                    { "branchName", new TextProperty { Fields = new Properties { { "keyword", new KeywordProperty() } } } },
                    { "isActive", new BooleanProperty() },
                    { "isManager", new BooleanProperty() },
                    { "roles", new KeywordProperty() },
                    { "profileImageUrl", new KeywordProperty() },
                    { "createdDate", new DateProperty() }
                })), cancellationToken);

        if (!created.IsValidResponse)
            _logger.LogWarning("Elasticsearch staff index create issue: {Error}", created.DebugInformation);
    }

    public async Task IndexAsync(StaffUserSearchDocument document, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
            return;

        try
        {
            await EnsureIndexAsync(cancellationToken);
            await _client.IndexAsync(document, i => i.Index(IndexName).Id(document.Id.ToString()), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to index staff user {Id}", document.Id);
        }
    }

    public async Task UpsertFromUserAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable || user.CustomerId.HasValue)
            return;

        StaffUserSearchDocument doc = await BuildDocumentAsync(user, cancellationToken);
        await IndexAsync(doc, cancellationToken);
    }

    public async Task ReindexAllAsync(CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
            return;

        await EnsureIndexAsync(cancellationToken);

        var page = await _users.GetListAsync(
            predicate: u => u.CustomerId == null,
            include: q => q
                .Include(u => u.Branch)
                .Include(u => u.UserOperationClaims)
                .ThenInclude(uoc => uoc.OperationClaim),
            index: 0,
            size: 500,
            cancellationToken: cancellationToken);

        foreach (ApplicationUser user in page.Items)
            await UpsertFromUserAsync(user, cancellationToken);

        _logger.LogInformation("Reindexed {Count} staff users into {Index}", page.Items.Count, IndexName);
    }

    public async Task<GetListResponse<GetListStaffUserListItemDto>?> SearchAsync(
        StaffUserSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
            return null;

        bool hasText = !string.IsNullOrWhiteSpace(query.Text);
        bool hasFilters = query.BranchId.HasValue
            || query.RequiredBranchScope.HasValue
            || query.IsManager.HasValue
            || query.IsActive.HasValue;

        if (!hasText && !hasFilters)
            return null;

        try
        {
            await EnsureIndexAsync(cancellationToken);

            int from = Math.Max(query.PageIndex, 0) * Math.Max(query.PageSize, 1);
            BoolQuery boolQuery = BuildBoolQuery(query);

            SearchResponse<StaffUserSearchDocument> response =
                await _client.SearchAsync<StaffUserSearchDocument>(s =>
                {
                    s.Index(IndexName).From(from).Size(query.PageSize);
                    s.Query(q => q.Bool(boolQuery));
                    s.Sort(new[]
                    {
                        SortOptions.Field(
                            new Field("email.keyword"),
                            new FieldSort { Order = SortOrder.Asc })
                    });
                }, cancellationToken);

            if (!response.IsValidResponse)
            {
                _logger.LogWarning("Elasticsearch staff search failed: {Info}", response.DebugInformation);
                return null;
            }

            long total = response.Total;
            List<GetListStaffUserListItemDto> items = response.Documents.Select(MapDto).ToList();
            int size = Math.Max(query.PageSize, 1);
            int pages = size == 0 ? 0 : (int)Math.Ceiling(total / (double)size);

            return new GetListResponse<GetListStaffUserListItemDto>
            {
                Items = items,
                Index = query.PageIndex,
                Size = size,
                Count = (int)Math.Min(total, int.MaxValue),
                Pages = pages,
                From = 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Elasticsearch staff search unavailable; caller should fall back to SQL");
            return null;
        }
    }

    private async Task<StaffUserSearchDocument> BuildDocumentAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        List<string> roles;
        if (user.UserOperationClaims is { Count: > 0 })
        {
            roles = user.UserOperationClaims
                .Where(uoc => !uoc.IsDeleted && uoc.OperationClaim is not null && !uoc.OperationClaim.IsDeleted)
                .Select(uoc => uoc.OperationClaim.Name)
                .Distinct()
                .OrderBy(n => n)
                .ToList();
        }
        else
        {
            List<Core.Security.Entities.OperationClaim> claims =
                await _users.GetClaimsAsync(user, cancellationToken);
            roles = claims.Select(c => c.Name).Distinct().OrderBy(n => n).ToList();
        }

        string? branchCode = user.Branch?.Code;
        string? branchName = user.Branch?.Name;
        if (user.BranchId.HasValue && (branchCode is null || branchName is null))
        {
            Branch? branch = await _branches.GetAsync(
                b => b.Id == user.BranchId.Value,
                cancellationToken: cancellationToken);
            branchCode ??= branch?.Code;
            branchName ??= branch?.Name;
        }

        return new StaffUserSearchDocument
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Title = user.Title?.Name,
            Gender = user.Gender?.Name,
            DateOfBirth = user.DateOfBirth?.ToString("yyyy-MM-dd"),
            Position = user.Position?.Name,
            BranchId = user.BranchId?.ToString(),
            BranchCode = branchCode,
            BranchName = branchName,
            IsActive = user.IsActive,
            IsManager = roles.Contains(GeneralOperationClaims.BankManager)
                || roles.Contains(GeneralOperationClaims.SuperAdmin),
            Roles = roles,
            ProfileImageUrl = user.ProfileImagePath,
            CreatedDate = user.CreatedDate
        };
    }

    private static BoolQuery BuildBoolQuery(StaffUserSearchQuery query)
    {
        List<Query> must = [];
        List<Query> filter = [];

        if (!string.IsNullOrWhiteSpace(query.Text))
        {
            string text = query.Text.Trim();
            must.Add(new MultiMatchQuery
            {
                Query = text,
                Fields = new[]
                {
                    "email^3",
                    "firstName^2",
                    "lastName^2",
                    "branchCode",
                    "branchName"
                },
                Fuzziness = new Fuzziness("AUTO")
            });
        }

        string? branchScope = query.RequiredBranchScope?.ToString()
            ?? (query.AllowCrossBranchFilters ? query.BranchId?.ToString() : null);
        if (!string.IsNullOrEmpty(branchScope))
            filter.Add(new TermQuery(new Field("branchId")) { Value = branchScope });

        if (query.IsActive.HasValue)
            filter.Add(new TermQuery(new Field("isActive")) { Value = query.IsActive.Value });

        if (query.IsManager.HasValue)
            filter.Add(new TermQuery(new Field("isManager")) { Value = query.IsManager.Value });

        return new BoolQuery
        {
            Must = must.Count > 0 ? must : null,
            Filter = filter.Count > 0 ? filter : null
        };
    }

    private static GetListStaffUserListItemDto MapDto(StaffUserSearchDocument doc)
    {
        PersonTitle? title = PersonTitle.TryFromName(doc.Title ?? "", ignoreCase: true, out PersonTitle? t) ? t : null;
        Gender? gender = Gender.TryFromName(doc.Gender ?? "", ignoreCase: true, out Gender? g) ? g : null;
        StaffPosition? position = StaffPosition.TryFromName(doc.Position ?? "", ignoreCase: true, out StaffPosition? p)
            ? p
            : null;
        DateOnly? dob = DateOnly.TryParse(doc.DateOfBirth, out DateOnly d) ? d : null;
        Guid? branchId = Guid.TryParse(doc.BranchId, out Guid bid) ? bid : null;

        return new GetListStaffUserListItemDto
        {
            Id = doc.Id,
            Email = doc.Email,
            IsActive = doc.IsActive,
            IsManager = doc.IsManager,
            FirstName = doc.FirstName,
            LastName = doc.LastName,
            Title = title,
            Gender = gender,
            DateOfBirth = dob,
            Position = position,
            BranchId = branchId,
            BranchCode = doc.BranchCode,
            BranchName = doc.BranchName,
            ProfileImageUrl = doc.ProfileImageUrl,
            Roles = doc.Roles,
            CreatedDate = doc.CreatedDate
        };
    }
}
