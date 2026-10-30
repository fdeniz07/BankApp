using Application.Services.Auth;
using Application.Services.Repositories;
using Core.CrossCuttingConcerns.Exceptions.Types;
using Domain.Entities;
using MediatR;

namespace Application.Modules.Identity.Features.DecisionLetterTemplates.Commands.Update;

public class UpdateDecisionLetterTemplateRequestHandler
    : IRequestHandler<UpdateDecisionLetterTemplateRequest, UpdatedDecisionLetterTemplateResponse>
{
    private readonly IDecisionLetterTemplateRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public UpdateDecisionLetterTemplateRequestHandler(
        IDecisionLetterTemplateRepository repository,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<UpdatedDecisionLetterTemplateResponse> Handle(
        UpdateDecisionLetterTemplateRequest request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            throw new AuthorizationException("You are not authenticated.");

        DecisionLetterTemplate? entity = await _repository.GetAsync(
            t => t.Id == request.Id && t.UserId == _currentUser.UserId.Value,
            cancellationToken: cancellationToken);
        if (entity is null)
            throw new BusinessException("Template not found.");

        string name = request.Name.Trim();
        string body = request.Body.Trim();
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(body))
            throw new BusinessException("Template name and body are required.");

        entity.Kind = request.Kind.Equals("Reject", StringComparison.OrdinalIgnoreCase) ? "Reject" : "Approve";
        string locale = (request.Locale ?? "tr").Trim().ToLowerInvariant();
        entity.Locale = locale is "en" or "de" or "tr" ? locale : "tr";
        entity.Name = name;
        entity.Body = body;

        await _repository.UpdateAsync(entity, cancellationToken);

        return new UpdatedDecisionLetterTemplateResponse
        {
            Id = entity.Id,
            Kind = entity.Kind,
            Locale = entity.Locale,
            Name = entity.Name,
            Body = entity.Body
        };
    }
}
