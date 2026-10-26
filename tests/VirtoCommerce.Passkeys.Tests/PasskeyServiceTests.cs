using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using VirtoCommerce.Passkeys.Core.Models;
using VirtoCommerce.Passkeys.Data.Services;
using VirtoCommerce.Platform.Core.Security;
using Xunit;

namespace VirtoCommerce.Passkeys.Tests;

[Trait("Category", "Unit")]
public class PasskeyServiceTests
{
    private const string _credential = "credential";

    private readonly Dictionary<string, ApplicationUser> _credentialOwners = [];
    private readonly List<(string UserId, UserPasskeyInfo Passkey)> _storedPasskeys = [];

    [Fact]
    public async Task CreateAsync_Should_Fail_When_ThePasskeyWasCreatedForAnotherUser()
    {
        // Arrange
        var user = new ApplicationUser { Id = "user-id", UserName = "buyer" };
        _credentialOwners[_credential] = new ApplicationUser { Id = "other-user-id", UserName = "other-buyer" };

        var service = CreateService();

        // Act
        var result = await service.CreateAsync(user, _credential, name: null);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(PasskeyErrorDescriber.PasskeyForAnotherUser().Code, Assert.Single(result.Errors).Code);
        Assert.Empty(_storedPasskeys);
    }

    private PasskeyService CreateService()
    {
        var userManager = new Mock<UserManager<ApplicationUser>>(Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

        userManager
            .Setup(x => x.AddOrUpdatePasskeyAsync(It.IsAny<ApplicationUser>(), It.IsAny<UserPasskeyInfo>()))
            .ReturnsAsync((ApplicationUser user, UserPasskeyInfo passkey) =>
            {
                _storedPasskeys.Add((user.Id, passkey));

                return IdentityResult.Success;
            });

        var signInManager = new Mock<SignInManager<ApplicationUser>>(
            userManager.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
            null, null, null, null);

        signInManager
            .Setup(x => x.PerformPasskeyAssertionAsync(It.IsAny<string>()))
            .ReturnsAsync((string credential) => _credentialOwners.TryGetValue(credential, out var user)
                ? PasskeyAssertionResult.Success(CreatePasskey(), user)
                : PasskeyAssertionResult.Fail<ApplicationUser>(new PasskeyException("Invalid assertion.")));

        signInManager
            .Setup(x => x.PerformPasskeyAttestationAsync(It.IsAny<string>()))
            .ReturnsAsync((string credential) => _credentialOwners.TryGetValue(credential, out var user)
                ? PasskeyAttestationResult.Success(CreatePasskey(), new PasskeyUserEntity { Id = user.Id, Name = user.UserName, DisplayName = user.UserName })
                : PasskeyAttestationResult.Fail(new PasskeyException("Invalid attestation.")));

        return new PasskeyService(signInManager.Object, Options.Create(new PasskeyOptions()), Options.Create(new AuthorizationOptions()));
    }

    private static UserPasskeyInfo CreatePasskey()
    {
        return new UserPasskeyInfo(
            credentialId: [1],
            publicKey: [2],
            createdAt: DateTimeOffset.UtcNow,
            signCount: 0,
            transports: [],
            isUserVerified: true,
            isBackupEligible: true,
            isBackedUp: true,
            attestationObject: [3],
            clientDataJson: [4]);
    }
}
