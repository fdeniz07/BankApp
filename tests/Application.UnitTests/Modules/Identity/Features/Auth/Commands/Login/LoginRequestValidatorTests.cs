using Application.Modules.Identity.Features.Auth.Commands.Login;
using FluentAssertions;
using FluentValidation.Results;

namespace Application.UnitTests.Features.Auth.Commands.Login;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _sut = new();

    [Fact]
    public void Validate_WhenValid_Passes()
    {
        LoginRequest request = new()
        {
            Email = "staff@bank.local",
            Password = "Demo123!"
        };

        ValidationResult result = _sut.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "Demo123!")]
    [InlineData("not-an-email", "Demo123!")]
    [InlineData("staff@bank.local", "")]
    [InlineData("staff@bank.local", "12345")]
    public void Validate_WhenInvalid_Fails(string email, string password)
    {
        LoginRequest request = new()
        {
            Email = email,
            Password = password
        };

        ValidationResult result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }
}
