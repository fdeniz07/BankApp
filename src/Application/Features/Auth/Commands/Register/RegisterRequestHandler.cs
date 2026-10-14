using Application.Features.Auth.Rules;
using Application.Services.Repositories;
using Core.Security.Constants;
using Core.Security.Entities;
using Core.Security.Hashing;
using Core.Security.JWT;
using Domain.Entities;
using MediatR;

namespace Application.Features.Auth.Commands.Register;

public class RegisterRequestHandler : IRequestHandler<RegisterRequest, RegisteredResponse>
{
    private readonly IApplicationUserRepository _applicationUserRepository;
    private readonly IOperationClaimRepository _operationClaimRepository;
    private readonly IUserOperationClaimRepository _userOperationClaimRepository;
    private readonly ITokenHelper _tokenHelper;
    private readonly AuthBusinessRules _businessRules;

    public RegisterRequestHandler(
        IApplicationUserRepository applicationUserRepository,
        IOperationClaimRepository operationClaimRepository,
        IUserOperationClaimRepository userOperationClaimRepository,
        ITokenHelper tokenHelper,
        AuthBusinessRules businessRules)
    {
        _applicationUserRepository = applicationUserRepository;
        _operationClaimRepository = operationClaimRepository;
        _userOperationClaimRepository = userOperationClaimRepository;
        _tokenHelper = tokenHelper;
        _businessRules = businessRules;
    }

    public async Task<RegisteredResponse> Handle(RegisterRequest request, CancellationToken cancellationToken)
    {
        await _businessRules.EmailShouldBeUnique(request.Email);
        await _businessRules.CustomerShouldExistWhenProvided(request.CustomerId);

        HashingHelper.CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

        ApplicationUser user = new()
        {
            Email = request.Email,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            CustomerId = request.CustomerId,
            IsActive = true
        };

        await _applicationUserRepository.AddAsync(user, cancellationToken);

        OperationClaim? customerClaim = await _operationClaimRepository.GetAsync(
            c => c.Name == GeneralOperationClaims.Customer,
            cancellationToken: cancellationToken);

        if (customerClaim is null)
            throw new InvalidOperationException($"Operation claim '{GeneralOperationClaims.Customer}' is not seeded.");

        await _userOperationClaimRepository.AddAsync(
            new UserOperationClaim
            {
                UserId = user.Id,
                OperationClaimId = customerClaim.Id
            },
            cancellationToken);

        IList<OperationClaim> claims = await _applicationUserRepository.GetClaimsAsync(user, cancellationToken);
        AccessToken accessToken = _tokenHelper.CreateToken(user, claims, user.CustomerId);

        return new RegisteredResponse { AccessToken = accessToken };
    }
}
