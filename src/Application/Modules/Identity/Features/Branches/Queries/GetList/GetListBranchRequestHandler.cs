using Application.Services.Repositories;
using Domain.Entities;
using MediatR;

namespace Application.Modules.Identity.Features.Branches.Queries.GetList;

public class GetListBranchRequestHandler
    : IRequestHandler<GetListBranchRequest, IReadOnlyList<GetListBranchListItemDto>>
{
    private readonly IBranchRepository _branchRepository;

    public GetListBranchRequestHandler(IBranchRepository branchRepository)
    {
        _branchRepository = branchRepository;
    }

    public async Task<IReadOnlyList<GetListBranchListItemDto>> Handle(
        GetListBranchRequest request,
        CancellationToken cancellationToken)
    {
        var page = await _branchRepository.GetListAsync(
            predicate: b => !request.IsActive.HasValue || b.IsActive == request.IsActive.Value,
            orderBy: q => q.OrderBy(b => b.Code),
            size: 100,
            cancellationToken: cancellationToken);

        return page.Items
            .Select(b => new GetListBranchListItemDto
            {
                Id = b.Id,
                Code = b.Code,
                Name = b.Name,
                City = b.City,
                IsActive = b.IsActive
            })
            .ToList();
    }
}
