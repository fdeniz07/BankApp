using Application.Modules.Identity.Features.ProfileImages.Rules;
using Application.Services.Auth;
using Application.Services.Logging;
using Application.Services.ProfileImages;
using Application.Services.Repositories;
using Core.CrossCuttingConcerns.Exceptions.Types;
using Domain.Entities;
using MediatR;

namespace Application.Modules.Identity.Features.ProfileImages.Commands.Upload;

public class UploadProfileImageRequestHandler
    : IRequestHandler<UploadProfileImageRequest, UploadProfileImageResponse>
{
    private readonly IApplicationUserRepository _applicationUserRepository;
    private readonly IProfileImageStorage _profileImageStorage;
    private readonly ProfileImageBusinessRules _businessRules;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogger _auditLogger;

    public UploadProfileImageRequestHandler(
        IApplicationUserRepository applicationUserRepository,
        IProfileImageStorage profileImageStorage,
        ProfileImageBusinessRules businessRules,
        ICurrentUserService currentUserService,
        IAuditLogger auditLogger)
    {
        _applicationUserRepository = applicationUserRepository;
        _profileImageStorage = profileImageStorage;
        _businessRules = businessRules;
        _currentUserService = currentUserService;
        _auditLogger = auditLogger;
    }

    public async Task<UploadProfileImageResponse> Handle(
        UploadProfileImageRequest request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
            throw new AuthorizationException("You are not authenticated.");

        Guid userId = _currentUserService.UserId.Value;
        await _businessRules.FileShouldBePresent(request.Content, request.Length);
        await _businessRules.FileShouldNotExceedMaxSize(request.Length);
        await _businessRules.ContentTypeShouldBeAllowed(request.ContentType);

        // Buffer so magic-byte check + save both see the full stream (FormFile streams may not seek).
        await using MemoryStream buffer = new();
        if (request.Content.CanSeek)
            request.Content.Position = 0;
        await request.Content.CopyToAsync(buffer, cancellationToken);
        buffer.Position = 0;

        await _businessRules.ContentShouldMatchDeclaredType(buffer, request.ContentType, cancellationToken);
        buffer.Position = 0;

        ApplicationUser? user = await _applicationUserRepository.GetAsync(
            u => u.Id == userId,
            cancellationToken: cancellationToken);
        if (user is null)
            throw new AuthorizationException("User not found.");

        string? previous = user.ProfileImagePath;
        string path = await _profileImageStorage.SaveAsync(
            userId,
            buffer,
            request.ContentType,
            cancellationToken);

        user.ProfileImagePath = path;
        await _applicationUserRepository.UpdateAsync(user, cancellationToken);

        if (!string.Equals(previous, path, StringComparison.OrdinalIgnoreCase))
            await _profileImageStorage.DeleteIfExistsAsync(previous, cancellationToken);

        _auditLogger.Log(
            AuditEventCategories.Profile,
            AuditEventActions.Upload,
            "Profile image uploaded.",
            actorUserId: userId,
            actorEmail: user.Email,
            targetType: nameof(ApplicationUser),
            targetId: userId);

        return new UploadProfileImageResponse
        {
            UserId = userId,
            ProfileImageUrl = path
        };
    }
}
