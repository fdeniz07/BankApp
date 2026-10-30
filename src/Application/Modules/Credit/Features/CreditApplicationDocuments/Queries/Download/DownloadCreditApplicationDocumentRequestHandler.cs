using Application.Modules.Credit.Features.CreditApplicationDocuments.Rules;
using Application.Modules.Credit.Features.CreditApplications.Rules;
using Application.Services.Auth;
using Application.Services.CreditDocuments;
using Application.Services.Repositories;
using Domain.Entities;
using MediatR;

namespace Application.Modules.Credit.Features.CreditApplicationDocuments.Queries.Download;

public class DownloadCreditApplicationDocumentRequestHandler
    : IRequestHandler<DownloadCreditApplicationDocumentRequest, DownloadCreditApplicationDocumentResponse>
{
    private readonly ICreditApplicationRepository _creditApplicationRepository;
    private readonly ICreditApplicationDocumentRepository _documentRepository;
    private readonly ICreditDocumentStorage _documentStorage;
    private readonly CreditApplicationBusinessRules _applicationRules;
    private readonly CreditApplicationDocumentBusinessRules _documentRules;
    private readonly IBranchAuthorizationService _branchAuthorization;

    public DownloadCreditApplicationDocumentRequestHandler(
        ICreditApplicationRepository creditApplicationRepository,
        ICreditApplicationDocumentRepository documentRepository,
        ICreditDocumentStorage documentStorage,
        CreditApplicationBusinessRules applicationRules,
        CreditApplicationDocumentBusinessRules documentRules,
        IBranchAuthorizationService branchAuthorization)
    {
        _creditApplicationRepository = creditApplicationRepository;
        _documentRepository = documentRepository;
        _documentStorage = documentStorage;
        _applicationRules = applicationRules;
        _documentRules = documentRules;
        _branchAuthorization = branchAuthorization;
    }

    public async Task<DownloadCreditApplicationDocumentResponse> Handle(
        DownloadCreditApplicationDocumentRequest request,
        CancellationToken cancellationToken)
    {
        CreditApplication? application = await _creditApplicationRepository.GetAsync(
            a => a.Id == request.CreditApplicationId,
            cancellationToken: cancellationToken);
        await _applicationRules.CreditApplicationShouldExistWhenSelected(application);
        await _branchAuthorization.EnsureCanViewApplicationAsync(application!, cancellationToken);

        CreditApplicationDocument? document = await _documentRepository.GetAsync(
            d => d.Id == request.DocumentId
                 && d.CreditApplicationId == request.CreditApplicationId
                 && !d.IsDeleted,
            cancellationToken: cancellationToken);
        await _documentRules.DocumentShouldExist(document);

        Stream content = await _documentStorage.OpenReadAsync(document!.StoragePath, cancellationToken);

        return new DownloadCreditApplicationDocumentResponse
        {
            Content = content,
            ContentType = document.ContentType,
            FileName = document.OriginalFileName
        };
    }
}
