using Core.Security.JWT;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;

namespace Application.UnitTests.Security;

public class TokenSigningKeyFactoryTests
{
    [Fact]
    public void CreateValidationKeys_WhenNoPrevious_ReturnsOnlyCurrent()
    {
        TokenOptions options = new()
        {
            Audience = "a",
            Issuer = "i",
            AccessTokenExpiration = 60,
            SecurityKey = "BankApp-Super-Secret-Security-Key-For-Jwt-Must-Be-At-Least-64-Bytes-Long-1234567890"
        };

        List<SecurityKey> keys = TokenSigningKeyFactory.CreateValidationKeys(options).ToList();

        keys.Should().HaveCount(1);
    }

    [Fact]
    public void CreateValidationKeys_WhenPreviousSet_ReturnsTwoDistinctKeys()
    {
        TokenOptions options = new()
        {
            Audience = "a",
            Issuer = "i",
            AccessTokenExpiration = 60,
            SecurityKey = "BankApp-Super-Secret-Security-Key-For-Jwt-Must-Be-At-Least-64-Bytes-Long-CURRENT000",
            SecurityKeyPrevious = "BankApp-Super-Secret-Security-Key-For-Jwt-Must-Be-At-Least-64-Bytes-Long-PREVIOUS0"
        };

        List<SecurityKey> keys = TokenSigningKeyFactory.CreateValidationKeys(options).ToList();

        keys.Should().HaveCount(2);
    }

    [Fact]
    public void CreateValidationKeys_WhenPreviousEqualsCurrent_ReturnsSingleKey()
    {
        string key = "BankApp-Super-Secret-Security-Key-For-Jwt-Must-Be-At-Least-64-Bytes-Long-1234567890";
        TokenOptions options = new()
        {
            Audience = "a",
            Issuer = "i",
            AccessTokenExpiration = 60,
            SecurityKey = key,
            SecurityKeyPrevious = key
        };

        TokenSigningKeyFactory.CreateValidationKeys(options).Should().HaveCount(1);
    }
}
