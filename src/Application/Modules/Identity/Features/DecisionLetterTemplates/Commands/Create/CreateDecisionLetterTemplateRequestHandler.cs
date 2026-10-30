using Application.Services.Auth;
using Application.Services.Repositories;
using Core.CrossCuttingConcerns.Exceptions.Types;
using Domain.Entities;
using MediatR;

namespace Application.Modules.Identity.Features.DecisionLetterTemplates.Commands.Create;

public class CreateDecisionLetterTemplateRequestHandler
    : IRequestHandler<CreateDecisionLetterTemplateRequest, CreatedDecisionLetterTemplateResponse>
{
    private readonly IDecisionLetterTemplateRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public CreateDecisionLetterTemplateRequestHandler(
        IDecisionLetterTemplateRepository repository,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<CreatedDecisionLetterTemplateResponse> Handle(
        CreateDecisionLetterTemplateRequest request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            throw new AuthorizationException("You are not authenticated.");

        string kind = NormalizeKind(request.Kind);
        string locale = NormalizeLocale(request.Locale);
        string name = request.Name.Trim();
        string body = request.Body.Trim();
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(body))
            throw new BusinessException("Template name and body are required.");

        DecisionLetterTemplate entity = new()
        {
            UserId = _currentUser.UserId.Value,
            Kind = kind,
            Locale = locale,
            Name = name,
            Body = body
        };
        await _repository.AddAsync(entity, cancellationToken);

        return new CreatedDecisionLetterTemplateResponse
        {
            Id = entity.Id,
            Kind = entity.Kind,
            Locale = entity.Locale,
            Name = entity.Name,
            Body = entity.Body
        };
    }

    private static string NormalizeKind(string? kind)
    {
        string value = (kind ?? "Approve").Trim();
        if (value.Equals("Reject", StringComparison.OrdinalIgnoreCase))
            return "Reject";
        return "Approve";
    }

    private static string NormalizeLocale(string? locale)
    {
        string value = (locale ?? "tr").Trim().ToLowerInvariant();
        return value is "en" or "de" or "tr" ? value : "tr";
    }
}
