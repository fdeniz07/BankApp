using Application.Services.Auth;
using Application.Services.Repositories;
using Core.CrossCuttingConcerns.Exceptions.Types;
using Domain.Entities;
using MediatR;

namespace Application.Modules.Identity.Features.DecisionLetterTemplates.Queries.GetList;

public class GetListDecisionLetterTemplateRequestHandler
    : IRequestHandler<GetListDecisionLetterTemplateRequest, IReadOnlyList<GetListDecisionLetterTemplateItemDto>>
{
    private readonly IDecisionLetterTemplateRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public GetListDecisionLetterTemplateRequestHandler(
        IDecisionLetterTemplateRepository repository,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<GetListDecisionLetterTemplateItemDto>> Handle(
        GetListDecisionLetterTemplateRequest request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            throw new AuthorizationException("You are not authenticated.");

        Guid userId = _currentUser.UserId.Value;
        string? kind = string.IsNullOrWhiteSpace(request.Kind) ? null : request.Kind.Trim();

        var page = await _repository.GetListAsync(
            predicate: t =>
                t.UserId == userId
                && (kind == null || t.Kind == kind),
            orderBy: q => q.OrderBy(t => t.Locale).ThenBy(t => t.Name),
            size: 100,
            cancellationToken: cancellationToken);

        return page.Items
            .Select(Map)
            .ToList();
    }

    private static GetListDecisionLetterTemplateItemDto Map(DecisionLetterTemplate t) =>
        new()
        {
            Id = t.Id,
            Kind = t.Kind,
            Locale = t.Locale,
            Name = t.Name,
            Body = t.Body
        };
}
