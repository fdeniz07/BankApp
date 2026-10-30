using Application.Modules.Credit.Features.CreditApplications.Rules;
using Application.Services.Auth;
using Application.Services.Repositories;
using Domain.Entities;
using MediatR;

namespace Application.Modules.Credit.Features.CreditApplicationDocuments.Queries.GetList;

public class GetListCreditApplicationDocumentRequestHandler
    : IRequestHandler<GetListCreditApplicationDocumentRequest, IReadOnlyList<GetListCreditApplicationDocumentItemDto>>
{
    private readonly ICreditApplicationRepository _creditApplicationRepository;
    private readonly ICreditApplicationDocumentRepository _documentRepository;
    private readonly CreditApplicationBusinessRules _applicationRules;
    private readonly IBranchAuthorizationService _branchAuthorization;

    public GetListCreditApplicationDocumentRequestHandler(
        ICreditApplicationRepository creditApplicationRepository,
        ICreditApplicationDocumentRepository documentRepository,
        CreditApplicationBusinessRules applicationRules,
        IBranchAuthorizationService branchAuthorization)
    {
        _creditApplicationRepository = creditApplicationRepository;
        _documentRepository = documentRepository;
        _applicationRules = applicationRules;
        _branchAuthorization = branchAuthorization;
    }

    public async Task<IReadOnlyList<GetListCreditApplicationDocumentItemDto>> Handle(
        GetListCreditApplicationDocumentRequest request,
        CancellationToken cancellationToken)
    {
        CreditApplication? application = await _creditApplicationRepository.GetAsync(
            a => a.Id == request.CreditApplicationId,
            cancellationToken: cancellationToken);
        await _applicationRules.CreditApplicationShouldExistWhenSelected(application);
        await _branchAuthorization.EnsureCanViewApplicationAsync(application!, cancellationToken);

        var page = await _documentRepository.GetListAsync(
            predicate: d => d.CreditApplicationId == request.CreditApplicationId && !d.IsDeleted,
            orderBy: q => q.OrderByDescending(d => d.UploadedAt),
            size: 50,
            cancellationToken: cancellationToken);

        return page.Items
            .Select(d => new GetListCreditApplicationDocumentItemDto
            {
                Id = d.Id,
                DocumentType = d.DocumentType.Name,
                OriginalFileName = d.OriginalFileName,
                ContentType = d.ContentType,
                SizeBytes = d.SizeBytes,
                UploadedAt = d.UploadedAt
            })
            .ToList();
    }
}
