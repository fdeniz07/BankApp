using Application.Features.Auth.Rules;
using Application.Services.Repositories;
using Core.Security.Entities;
using Core.Security.JWT;
using MediatR;

namespace Application.Features.Auth.Commands.Login;

public class LoginRequestHandler : IRequestHandler<LoginRequest, LoggedResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenHelper _tokenHelper;
    private readonly AuthBusinessRules _businessRules;

    public LoginRequestHandler(
        IUserRepository userRepository,
        ITokenHelper tokenHelper,
        AuthBusinessRules businessRules)
    {
        _userRepository = userRepository;
        _tokenHelper = tokenHelper;
        _businessRules = businessRules;
    }

    public async Task<LoggedResponse> Handle(LoginRequest request, CancellationToken cancellationToken)
    {
        User? user = await _userRepository.GetAsync(
            u => u.Email == request.Email,
            cancellationToken: cancellationToken);

        await _businessRules.UserShouldExist(user);
        await _businessRules.UserShouldBeActive(user!);
        await _businessRules.PasswordShouldBeCorrect(request.Password, user!.PasswordHash, user.PasswordSalt);

        IList<OperationClaim> claims = await _userRepository.GetClaimsAsync(user, cancellationToken);
        AccessToken accessToken = _tokenHelper.CreateToken(user, claims);

        return new LoggedResponse { AccessToken = accessToken };
    }
}
