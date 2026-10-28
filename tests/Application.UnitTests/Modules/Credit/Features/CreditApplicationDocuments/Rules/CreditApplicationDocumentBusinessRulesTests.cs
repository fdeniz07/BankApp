using Application.Modules.Credit.Features.CreditApplicationDocuments.Constants;
using Application.Modules.Credit.Features.CreditApplicationDocuments.Rules;
using Application.Services.Repositories;
using Core.CrossCuttingConcerns.Exceptions.Types;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Moq;

namespace Application.UnitTests.Modules.Credit.Features.CreditApplicationDocuments.Rules;

public class CreditApplicationDocumentBusinessRulesTests
{
    private readonly Mock<ICreditApplicationDocumentRepository> _documentRepository = new();
    private readonly CreditApplicationDocumentBusinessRules _sut;

    public CreditApplicationDocumentBusinessRulesTests()
    {
        _sut = new CreditApplicationDocumentBusinessRules(_documentRepository.Object);
    }

    [Fact]
    public async Task FileShouldNotExceedMaxSize_WhenTooLarge_Throws()
    {
        Func<Task> act = () => _sut.FileShouldNotExceedMaxSize(CreditApplicationDocumentBusinessRules.MaxBytes + 1);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage(CreditApplicationDocumentMessages.TooLarge);
    }

    [Fact]
    public async Task ContentTypeShouldBeAllowed_WhenGif_Throws()
    {
        Func<Task> act = () => _sut.ContentTypeShouldBeAllowed("image/gif");

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage(CreditApplicationDocumentMessages.InvalidContentType);
    }

    [Fact]
    public async Task ContentTypeShouldBeAllowed_WhenPdf_DoesNotThrow()
    {
        Func<Task> act = () => _sut.ContentTypeShouldBeAllowed("application/pdf");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ApplicationShouldBePendingForUpload_WhenApproved_Throws()
    {
        CreditApplication application = new() { Status = CreditApplicationStatus.Approved };

        Func<Task> act = () => _sut.ApplicationShouldBePendingForUpload(application);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage(CreditApplicationDocumentMessages.OnlyPendingCanUpload);
    }

    [Fact]
    public async Task DocumentTypeShouldBeValid_WhenUnknown_Throws()
    {
        Func<Task> act = () => _sut.DocumentTypeShouldBeValid("PassportScan");

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage(CreditApplicationDocumentMessages.InvalidDocumentType);
    }

    [Fact]
    public async Task ApplicationShouldNotExceedDocumentLimit_WhenAtCap_Throws()
    {
        _documentRepository
            .Setup(r => r.CountAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<CreditApplicationDocument, bool>>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreditApplicationDocumentBusinessRules.MaxDocumentsPerApplication);

        Func<Task> act = () => _sut.ApplicationShouldNotExceedDocumentLimit(Guid.NewGuid());

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage(CreditApplicationDocumentMessages.MaxDocumentsExceeded);
    }
}
