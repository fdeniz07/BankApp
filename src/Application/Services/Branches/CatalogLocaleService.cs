using Application.Services.Auth;
using Application.Services.Repositories;
using Domain.Constants;
using Domain.Entities;

namespace Application.Services.Branches;

public class CatalogLocaleService : ICatalogLocaleService
{
    private readonly ICurrentUserService _currentUser;
    private readonly IBranchRepository _branchRepository;

    public CatalogLocaleService(ICurrentUserService currentUser, IBranchRepository branchRepository)
    {
        _currentUser = currentUser;
        _branchRepository = branchRepository;
    }

    public async Task<string?> ResolveForcedCatalogLocaleAsync(CancellationToken cancellationToken = default)
    {
        if (_currentUser.IsSuperAdmin)
            return null;

        if (!_currentUser.IsBankStaff)
            return null;

        if (!_currentUser.BranchId.HasValue)
            return null;

        Branch? branch = await _branchRepository.GetAsync(
            b => b.Id == _currentUser.BranchId.Value,
            cancellationToken: cancellationToken);

        return BranchLocaleMap.ResolveLocale(branch?.Code);
    }
}
