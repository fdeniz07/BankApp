namespace Application.Services.Branches;

public interface ICatalogLocaleService
{
    /// <summary>
    /// Staff/Manager: branch country locale (tr|en|de). SuperAdmin: null (= no forced filter).
    /// Customer: null (UI may pass preferred locale).
    /// </summary>
    Task<string?> ResolveForcedCatalogLocaleAsync(CancellationToken cancellationToken = default);
}
