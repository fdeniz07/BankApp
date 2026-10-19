using Application.Features.Auth.Constants;
using Application.Services.Repositories;
using Core.Application.Pipelines;
using Core.CrossCuttingConcerns.Exceptions.Types;
using Core.Security.Entities;
using Core.Security.Hashing;
using Domain.Entities;

namespace Application.Features.Auth.Rules;

public class AuthBusinessRules : BaseBusinessRules
{
    private readonly IApplicationUserRepository _applicationUserRepository;
    private readonly ICustomerRepository _customerRepository;

    public AuthBusinessRules(
        IApplicationUserRepository applicationUserRepository,
        ICustomerRepository customerRepository)
    {
        _applicationUserRepository = applicationUserRepository;
        _customerRepository = customerRepository;
    }

    public Task UserShouldExist(ApplicationUser? user)
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

    public Task EmailShouldBeConfirmed(ApplicationUser user)
    {
        // Staff accounts are always treated as confirmed after seed.
        if (user.CustomerId.HasValue && !user.EmailConfirmed)
            throw new BusinessException(AuthMessages.EmailNotConfirmed);

        return Task.CompletedTask;
    }

    public Task PasswordShouldBeCorrect(string password, byte[] passwordHash, byte[] passwordSalt)
    {
        if (!HashingHelper.VerifyPasswordHash(password, passwordHash, passwordSalt))
            throw new BusinessException(AuthMessages.PasswordIncorrect);

        return Task.CompletedTask;
    }

    public async Task EmailShouldBeUnique(string email, Guid? excludeUserId = null)
    {
        bool exists = await _applicationUserRepository.AnyAsync(
            u => u.Email == email && (excludeUserId == null || u.Id != excludeUserId.Value));
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
