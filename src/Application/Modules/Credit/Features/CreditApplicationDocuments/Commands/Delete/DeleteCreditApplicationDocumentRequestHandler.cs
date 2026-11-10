using Application.Modules.Credit.Features.CreditApplicationDocuments.Rules;
using Application.Modules.Credit.Features.CreditApplications.Rules;
using Application.Services.Auth;
using Application.Services.CreditDocuments;
using Application.Services.Repositories;
using Application.Services.Search;
using Core.CrossCuttingConcerns.Exceptions.Types;
using Domain.Entities;
using MediatR;

namespace Application.Modules.Credit.Features.CreditApplicationDocuments.Commands.Delete;

public class DeleteCreditApplicationDocumentRequestHandler
    : IRequestHandler<DeleteCreditApplicationDocumentRequest, DeleteCreditApplicationDocumentResponse>
{
    private readonly ICreditApplicationRepository _creditApplicationRepository;
    private readonly ICreditApplicationDocumentRepository _documentRepository;
    private readonly ICreditDocumentStorage _documentStorage;
    private readonly CreditApplicationBusinessRules _applicationRules;
    private readonly CreditApplicationDocumentBusinessRules _documentRules;
    private readonly ICurrentUserService _currentUserService;
    private readonly IBranchAuthorizationService _branchAuthorization;
    private readonly ICreditApplicationSearchService _search;

    public DeleteCreditApplicationDocumentRequestHandler(
        ICreditApplicationRepository creditApplicationRepository,
        ICreditApplicationDocumentRepository documentRepository,
        ICreditDocumentStorage documentStorage,
        CreditApplicationBusinessRules applicationRules,
        CreditApplicationDocumentBusinessRules documentRules,
        ICurrentUserService currentUserService,
        IBranchAuthorizationService branchAuthorization,
        ICreditApplicationSearchService search)
    {
        _creditApplicationRepository = creditApplicationRepository;
        _documentRepository = documentRepository;
        _documentStorage = documentStorage;
        _applicationRules = applicationRules;
        _documentRules = documentRules;
        _currentUserService = currentUserService;
        _branchAuthorization = branchAuthorization;
        _search = search;
    }

    public async Task<DeleteCreditApplicationDocumentResponse> Handle(
        DeleteCreditApplicationDocumentRequest request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
            throw new AuthorizationException("You are not authenticated.");

        CreditApplication? application = await _creditApplicationRepository.GetAsync(
            a => a.Id == request.CreditApplicationId,
            cancellationToken: cancellationToken);
        await _applicationRules.CreditApplicationShouldExistWhenSelected(application);
        CreditApplication entity = application!;
        await _branchAuthorization.EnsureCanModifyApplicationAsync(entity, cancellationToken);
        await _documentRules.ApplicationShouldBePendingForDelete(entity);

        CreditApplicationDocument? document = await _documentRepository.GetAsync(
            d => d.Id == request.DocumentId
                 && d.CreditApplicationId == request.CreditApplicationId
                 && !d.IsDeleted,
            cancellationToken: cancellationToken);
        await _documentRules.DocumentShouldExist(document);
        CreditApplicationDocument doc = document!;

        string storagePath = doc.StoragePath;
        string documentType = doc.DocumentType.Name;

        await _documentRepository.DeleteAsync(doc, permanent: false, cancellationToken);
        await _documentStorage.DeleteIfExistsAsync(storagePath, cancellationToken);

        entity.RecordDocumentDeleted(documentType);
        await _creditApplicationRepository.UpdateAsync(entity, cancellationToken);
        await _search.UpsertFromApplicationAsync(entity, cancellationToken);

        return new DeleteCreditApplicationDocumentResponse
        {
            Id = doc.Id,
            CreditApplicationId = entity.Id,
            DocumentType = documentType
        };
    }
}
