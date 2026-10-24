using Application.Modules.Identity.Features.Auth.Constants;
using Application.Services.Logging;
using Application.Services.Repositories;
using Core.CrossCuttingConcerns.Exceptions.Types;
using Domain.Entities;
using MediatR;

namespace Application.Modules.Identity.Features.Auth.Commands.ConfirmEmail;

public class ConfirmEmailRequestHandler : IRequestHandler<ConfirmEmailRequest, ConfirmEmailResponse>
{
    private readonly IApplicationUserRepository _applicationUserRepository;
    private readonly IAuditLogger _auditLogger;

    public ConfirmEmailRequestHandler(
        IApplicationUserRepository applicationUserRepository,
        IAuditLogger auditLogger)
    {
        _applicationUserRepository = applicationUserRepository;
        _auditLogger = auditLogger;
    }

    public async Task<ConfirmEmailResponse> Handle(ConfirmEmailRequest request, CancellationToken cancellationToken)
    {
        string token = request.Token.Trim();
        ApplicationUser? user = await _applicationUserRepository.GetAsync(
            u => u.EmailConfirmationToken == token,
            cancellationToken: cancellationToken);

        if (user is null)
            throw new BusinessException(AuthMessages.InvalidOrExpiredConfirmationToken);

        if (user.EmailConfirmed)
            throw new BusinessException(AuthMessages.EmailAlreadyConfirmed);

        if (user.EmailConfirmationTokenExpiresAt is null ||
            user.EmailConfirmationTokenExpiresAt < DateTimeOffset.UtcNow)
            throw new BusinessException(AuthMessages.InvalidOrExpiredConfirmationToken);

        user.EmailConfirmed = true;
        user.EmailConfirmationToken = null;
        user.EmailConfirmationTokenExpiresAt = null;
        await _applicationUserRepository.UpdateAsync(user, cancellationToken);

        _auditLogger.Log(
            AuditEventCategories.Auth,
            "EmailConfirmed",
            "Email address confirmed.",
            actorUserId: user.Id,
            actorEmail: user.Email);

        return new ConfirmEmailResponse
        {
            Email = user.Email,
            Confirmed = true
        };
    }
}