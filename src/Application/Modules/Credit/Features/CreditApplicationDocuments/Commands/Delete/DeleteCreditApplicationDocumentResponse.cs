namespace Application.Modules.Credit.Features.CreditApplicationDocuments.Commands.Delete;

public class DeleteCreditApplicationDocumentResponse
{
    public Guid Id { get; set; }
    public Guid CreditApplicationId { get; set; }
    public string DocumentType { get; set; } = default!;
}
