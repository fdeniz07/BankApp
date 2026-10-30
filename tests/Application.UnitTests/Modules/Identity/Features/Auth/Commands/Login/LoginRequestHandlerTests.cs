using System.Linq.Expressions;
using Application.Modules.Identity.Features.Auth.Commands.Login;
using Application.Modules.Identity.Features.Auth.Constants;
using Application.Modules.Identity.Features.Auth.Rules;
using Application.Services.Logging;
using Application.Services.Repositories;
using Core.CrossCuttingConcerns.Exceptions.Types;
using Core.Security.Entities;
using Core.Security.Hashing;
using Core.Security.JWT;
using Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Options;
using Moq;

namespace Application.UnitTests.Features.Auth.Commands.Login;

public class LoginRequestHandlerTests
{
    private readonly Mock<IApplicationUserRepository> _applicationUserRepository = new();
    private readonly Mock<ICustomerRepository> _customerRepository = new();
    private readonly Mock<IIndividualCustomerRepository> _individualCustomerRepository = new();
    private readonly Mock<ICorporateCustomerRepository> _corporateCustomerRepository = new();
    private readonly Mock<ITokenHelper> _tokenHelper = new();
    private readonly Mock<IAuditLogger> _auditLogger = new();
    private readonly LoginRequestHandler _sut;

    public LoginRequestHandlerTests()
    {
        AuthBusinessRules businessRules = new(
            _applicationUserRepository.Object,
            _customerRepository.Object);

        IOptions<TokenOptions> tokenOptions = Options.Create(new TokenOptions
        {
            Audience = "test",
            Issuer = "test",
            AccessTokenExpiration = 60,
            RememberMeAccessTokenExpiration = 10080,
            SecurityKey = "BankApp-Super-Secret-Security-Key-For-Jwt-Must-Be-At-Least-64-Bytes-Long-1234567890"
        });

        _sut = new LoginRequestHandler(
            _applicationUserRepository.Object,
            _individualCustomerRepository.Object,
            _corporateCustomerRepository.Object,
            _tokenHelper.Object,
            tokenOptions,
            businessRules,
            _auditLogger.Object);
    }

    [Fact]
    public async Task Handle_WhenUserMissing_ThrowsUserNotFound()
    {
        SetupGetUser(null);

        LoginRequest request = new()
        {
            Email = "missing@bank.local",
            Password = "Password1!"
        };

        Func<Task> act = () => _sut.Handle(request, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage(AuthMessages.UserNotFound);
    }

    [Fact]
    public async Task Handle_WhenPasswordIncorrect_Throws()
    {
        HashingHelper.CreatePasswordHash("Correct123!", out byte[] hash, out byte[] salt);
        ApplicationUser user = CreateUser("staff@bank.local", hash, salt);

        SetupGetUser(user);

        LoginRequest request = new()
        {
            Email = user.Email,
            Password = "WrongPassword!"
        };

        Func<Task> act = () => _sut.Handle(request, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage(AuthMessages.PasswordIncorrect);
    }

    [Fact]
    public async Task Handle_WhenCredentialsValid_ReturnsAccessToken()
    {
        const string password = "Demo123!";
        HashingHelper.CreatePasswordHash(password, out byte[] hash, out byte[] salt);
        ApplicationUser user = CreateUser("staff@bank.local", hash, salt);
        List<OperationClaim> claims = [new() { Id = Guid.NewGuid(), Name = "BankStaff" }];
        AccessToken accessToken = new()
        {
            Token = "jwt-token",
            Expiration = DateTime.UtcNow.AddHours(1)
        };

        SetupGetUser(user);
        _applicationUserRepository
            .Setup(r => r.GetClaimsAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(claims);
        _tokenHelper
            .Setup(t => t.CreateToken(
                user,
                claims,
                user.CustomerId,
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<int?>(),
                It.IsAny<Guid?>()))
            .Returns(accessToken);

        LoginRequest request = new()
        {
            Email = user.Email,
            Password = password
        };

        LoggedResponse response = await _sut.Handle(request, CancellationToken.None);

        response.AccessToken.Should().NotBeNull();
        response.AccessToken.Token.Should().Be("jwt-token");
        response.DisplayName.Should().Be("Selin Yılmaz");
        _tokenHelper.Verify(
            t => t.CreateToken(user, claims, user.CustomerId, "Selin Yılmaz", null, null, user.BranchId),
            Times.Once);
    }

    private void SetupGetUser(ApplicationUser? user)
    {
        _applicationUserRepository
            .Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<ApplicationUser, bool>>>(),
                It.IsAny<Func<IQueryable<ApplicationUser>, IIncludableQueryable<ApplicationUser, object>>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
    }

    private static ApplicationUser CreateUser(string email, byte[] hash, byte[] salt) =>
        new()
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = hash,
            PasswordSalt = salt,
            IsActive = true
        };
}
