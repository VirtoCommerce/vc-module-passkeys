using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using VirtoCommerce.Passkeys.Core;
using VirtoCommerce.Passkeys.Core.Models;
using VirtoCommerce.Passkeys.Data.Services;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;
using Xunit;

namespace VirtoCommerce.Passkeys.Tests;

[Trait("Category", "Unit")]
public class PasskeyServiceTests
{
    private const string _credential = "credential";
    private const string _storeId = "store-id";

    private readonly Dictionary<string, ApplicationUser> _credentialOwners = [];
    private readonly List<(string UserId, UserPasskeyInfo Passkey)> _storedPasskeys = [];
    private readonly List<StoreAuthenticationScheme> _storeSchemes = [];

    [Fact]
    public async Task CreateAsync_Should_Fail_When_PasskeyWasCreatedForAnotherUser()
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

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task IsPasskeyEnabledAsync_Should_FollowStorePasskeyScheme(bool isActive)
    {
        // Arrange
        _storeSchemes.Add(new StoreAuthenticationScheme { StoreId = _storeId, Name = ModuleConstants.Security.SignInType, IsActive = isActive });

        var service = CreateService();

        // Act
        var result = await service.IsPasskeyEnabledAsync(_storeId);

        // Assert
        Assert.Equal(isActive, result);
    }

    [Fact]
    public async Task IsPasskeyEnabledAsync_Should_ReturnFalse_When_StoreHasNoPasskeyScheme()
    {
        // Arrange
        _storeSchemes.Add(new StoreAuthenticationScheme { StoreId = _storeId, Name = "Password", IsActive = true });

        var service = CreateService();

        // Act
        var result = await service.IsPasskeyEnabledAsync(_storeId);

        // Assert
        Assert.False(result);
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

        var storeAuthenticationService = new Mock<IStoreAuthenticationService>();

        storeAuthenticationService
            .Setup(x => x.GetStoreSchemesAsync(_storeId, clone: false))
            .ReturnsAsync(_storeSchemes);

        return new PasskeyService(signInManager.Object, Options.Create(new PasskeyOptions()), Options.Create(new AuthorizationOptions()), storeAuthenticationService.Object);
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
