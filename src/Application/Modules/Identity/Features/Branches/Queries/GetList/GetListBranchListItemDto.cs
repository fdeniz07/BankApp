namespace Application.Modules.Identity.Features.Branches.Queries.GetList;

public class GetListBranchListItemDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? City { get; set; }
    public bool IsActive { get; set; }
}
