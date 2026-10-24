namespace Application.Modules.Credit.Features.CreditTypes.Commands.SetActive;

public class SetCreditTypeActiveResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public bool IsActive { get; set; }
}
