using Application.Features.Auth.Constants;
using Application.Services.Repositories;
using Core.Application.Pipelines;
using Core.CrossCuttingConcerns.Exceptions.Types;
using Core.Security.Entities;
using Core.Security.Hashing;

namespace Application.Features.Auth.Rules;

public class AuthBusinessRules : BaseBusinessRules
{
    private readonly IUserRepository _userRepository;
    private readonly ICustomerRepository _customerRepository;

    public AuthBusinessRules(IUserRepository userRepository, ICustomerRepository customerRepository)
    {
        _userRepository = userRepository;
        _customerRepository = customerRepository;
    }

    public Task UserShouldExist(User? user)
    {
        if (user is null)
            throw new BusinessException(AuthMessages.UserNotFound);

        return Task.CompletedTask;
    }

    public Task UserShouldBeActive(User user)
    {
        if (!user.IsActive)
            throw new BusinessException(AuthMessages.UserNotActive);

        return Task.CompletedTask;
    }

    public Task PasswordShouldBeCorrect(string password, byte[] passwordHash, byte[] passwordSalt)
    {
        if (!HashingHelper.VerifyPasswordHash(password, passwordHash, passwordSalt))
            throw new BusinessException(AuthMessages.PasswordIncorrect);

        return Task.CompletedTask;
    }

    public async Task EmailShouldBeUnique(string email)
    {
        bool exists = await _userRepository.AnyAsync(u => u.Email == email);
        if (exists)
            throw new BusinessException(AuthMessages.EmailAlreadyExists);
    }

    public async Task CustomerShouldExistWhenProvided(Guid? customerId)
    {
        if (!customerId.HasValue)
            return;

        bool exists = await _customerRepository.AnyAsync(c => c.Id == customerId.Value);
        if (!exists)
            throw new BusinessException(AuthMessages.CustomerNotFound);
    }
}
