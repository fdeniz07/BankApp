using Application.Features.Auth.Rules;
using Application.Services.Repositories;
using Core.Security.Entities;
using Core.Security.JWT;
using Domain.Entities;
using MediatR;

namespace Application.Features.Auth.Commands.Login;

public class LoginRequestHandler : IRequestHandler<LoginRequest, LoggedResponse>
{
    private readonly IApplicationUserRepository _applicationUserRepository;
    private readonly ITokenHelper _tokenHelper;
    private readonly AuthBusinessRules _businessRules;

    public LoginRequestHandler(
        IApplicationUserRepository applicationUserRepository,
        ITokenHelper tokenHelper,
        AuthBusinessRules businessRules)
    {
        _applicationUserRepository = applicationUserRepository;
        _tokenHelper = tokenHelper;
        _businessRules = businessRules;
    }

    public async Task<LoggedResponse> Handle(LoginRequest request, CancellationToken cancellationToken)
    {
        ApplicationUser? user = await _applicationUserRepository.GetAsync(
            u => u.Email == request.Email,
            cancellationToken: cancellationToken);

        await _businessRules.UserShouldExist(user);
        await _businessRules.UserShouldBeActive(user!);
        await _businessRules.PasswordShouldBeCorrect(request.Password, user!.PasswordHash, user.PasswordSalt);

        IList<OperationClaim> claims = await _applicationUserRepository.GetClaimsAsync(user, cancellationToken);
        AccessToken accessToken = _tokenHelper.CreateToken(user, claims, user.CustomerId);

        return new LoggedResponse { AccessToken = accessToken };
    }
}
