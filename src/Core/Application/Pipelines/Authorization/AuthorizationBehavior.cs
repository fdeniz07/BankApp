using Core.CrossCuttingConcerns.Exceptions.Types;
using Core.Security.Constants;
using Core.Security.Extensions;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Core.Application.Pipelines.Authorization;

public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ISecuredRequest
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthorizationBehavior(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated != true)
            throw new AuthorizationException("You are not authenticated.");

        List<string>? userRoles = _httpContextAccessor.HttpContext.User.ClaimRoles();
        if (userRoles is null || userRoles.Count == 0)
            throw new AuthorizationException("You are not authorized.");

        if (userRoles.Contains(GeneralOperationClaims.SuperAdmin))
            return await next();

        bool isAuthorized = request.Roles.Any(requiredRole => userRoles.Contains(requiredRole));
        if (!isAuthorized)
            throw new AuthorizationException("You are not authorized.");

        return await next();
    }
}
