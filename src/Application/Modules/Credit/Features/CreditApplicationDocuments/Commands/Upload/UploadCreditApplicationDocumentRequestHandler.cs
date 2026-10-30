using Application.Modules.Credit.Features.CreditApplicationDocuments.Rules;
using Application.Modules.Credit.Features.CreditApplications.Rules;
using Application.Services.Auth;
using Application.Services.CreditDocuments;
using Application.Services.Repositories;
using Core.CrossCuttingConcerns.Exceptions.Types;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Modules.Credit.Features.CreditApplicationDocuments.Commands.Upload;

public class UploadCreditApplicationDocumentRequestHandler
    : IRequestHandler<UploadCreditApplicationDocumentRequest, UploadCreditApplicationDocumentResponse>
{
    private readonly ICreditApplicationRepository _creditApplicationRepository;
    private readonly ICreditApplicationDocumentRepository _documentRepository;
    private readonly ICreditDocumentStorage _documentStorage;
    private readonly CreditApplicationBusinessRules _applicationRules;
    private readonly CreditApplicationDocumentBusinessRules _documentRules;
    private readonly ICurrentUserService _currentUserService;
    private readonly IBranchAuthorizationService _branchAuthorization;

    public UploadCreditApplicationDocumentRequestHandler(
        ICreditApplicationRepository creditApplicationRepository,
        ICreditApplicationDocumentRepository documentRepository,
        ICreditDocumentStorage documentStorage,
        CreditApplicationBusinessRules applicationRules,
        CreditApplicationDocumentBusinessRules documentRules,
        ICurrentUserService currentUserService,
        IBranchAuthorizationService branchAuthorization)
    {
        _creditApplicationRepository = creditApplicationRepository;
        _documentRepository = documentRepository;
        _documentStorage = documentStorage;
        _applicationRules = applicationRules;
        _documentRules = documentRules;
        _currentUserService = currentUserService;
        _branchAuthorization = branchAuthorization;
    }

    public async Task<UploadCreditApplicationDocumentResponse> Handle(
        UploadCreditApplicationDocumentRequest request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
            throw new AuthorizationException("You are not authenticated.");

        await _documentRules.DocumentTypeShouldBeValid(request.DocumentType);
        await _documentRules.FileShouldBePresent(request.Content, request.Length);
        await _documentRules.FileShouldNotExceedMaxSize(request.Length);
        await _documentRules.ContentTypeShouldBeAllowed(request.ContentType);

        CreditApplication? application = await _creditApplicationRepository.GetAsync(
            a => a.Id == request.CreditApplicationId,
            cancellationToken: cancellationToken);
        await _applicationRules.CreditApplicationShouldExistWhenSelected(application);
        CreditApplication entity = application!;
        await _branchAuthorization.EnsureCanModifyApplicationAsync(entity, cancellationToken);
        await _documentRules.ApplicationShouldBePendingForUpload(entity);
        await _documentRules.ApplicationShouldNotExceedDocumentLimit(entity.Id);

        await using MemoryStream buffer = new();
        if (request.Content.CanSeek)
            request.Content.Position = 0;
        await request.Content.CopyToAsync(buffer, cancellationToken);
        buffer.Position = 0;

        await _documentRules.ContentShouldMatchDeclaredType(buffer, request.ContentType, cancellationToken);
        buffer.Position = 0;

        CreditDocumentType documentType = CreditDocumentType.FromName(request.DocumentType.Trim(), ignoreCase: true);
        CreditApplicationDocument document = new()
        {
            CreditApplicationId = entity.Id,
            DocumentType = documentType,
            OriginalFileName = SanitizeFileName(request.OriginalFileName),
            ContentType = request.ContentType.Trim(),
            SizeBytes = request.Length,
            UploadedByUserId = _currentUserService.UserId.Value,
            UploadedAt = DateTimeOffset.UtcNow
        };

        string storagePath = await _documentStorage.SaveAsync(
            entity.Id,
            document.Id,
            buffer,
            request.ContentType,
            cancellationToken);
        document.StoragePath = storagePath;

        await _documentRepository.AddAsync(document, cancellationToken);

        entity.RecordDocumentUploaded(document.DocumentType.Name);
        await _creditApplicationRepository.UpdateAsync(entity, cancellationToken);

        return new UploadCreditApplicationDocumentResponse
        {
            Id = document.Id,
            CreditApplicationId = document.CreditApplicationId,
            DocumentType = document.DocumentType.Name,
            OriginalFileName = document.OriginalFileName,
            ContentType = document.ContentType,
            SizeBytes = document.SizeBytes,
            UploadedAt = document.UploadedAt
        };
    }

    private static string SanitizeFileName(string? fileName)
    {
        string name = string.IsNullOrWhiteSpace(fileName) ? "document" : Path.GetFileName(fileName.Trim());
        if (name.Length > 260)
            name = name[^260..];
        return name;
    }
}
