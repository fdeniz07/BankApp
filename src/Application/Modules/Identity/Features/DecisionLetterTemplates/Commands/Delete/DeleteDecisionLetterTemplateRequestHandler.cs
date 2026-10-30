using Application.Services.Auth;
using Application.Services.Repositories;
using Core.CrossCuttingConcerns.Exceptions.Types;
using Domain.Entities;
using MediatR;

namespace Application.Modules.Identity.Features.DecisionLetterTemplates.Commands.Delete;

public class DeleteDecisionLetterTemplateRequestHandler
    : IRequestHandler<DeleteDecisionLetterTemplateRequest, DeletedDecisionLetterTemplateResponse>
{
    private readonly IDecisionLetterTemplateRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public DeleteDecisionLetterTemplateRequestHandler(
        IDecisionLetterTemplateRepository repository,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<DeletedDecisionLetterTemplateResponse> Handle(
        DeleteDecisionLetterTemplateRequest request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            throw new AuthorizationException("You are not authenticated.");

        DecisionLetterTemplate? entity = await _repository.GetAsync(
            t => t.Id == request.Id && t.UserId == _currentUser.UserId.Value,
            cancellationToken: cancellationToken);
        if (entity is null)
            throw new BusinessException("Template not found.");

        await _repository.DeleteAsync(entity, cancellationToken: cancellationToken);
        return new DeletedDecisionLetterTemplateResponse { Id = entity.Id };
    }
}
