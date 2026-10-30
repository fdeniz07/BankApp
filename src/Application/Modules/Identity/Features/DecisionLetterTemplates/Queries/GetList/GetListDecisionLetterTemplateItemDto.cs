namespace Application.Modules.Identity.Features.DecisionLetterTemplates.Queries.GetList;

public class GetListDecisionLetterTemplateItemDto
{
    public Guid Id { get; set; }
    public string Kind { get; set; } = default!;
    public string Locale { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Body { get; set; } = default!;
}
