using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace WebApi.Modules.Credit.Idempotency;

[AttributeUsage(AttributeTargets.Method)]
public sealed class IdempotencyKeyAttribute : Attribute, IFilterFactory
{
    public bool IsReusable => false;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider) =>
        serviceProvider.GetRequiredService<IdempotencyKeyFilter>();
}
