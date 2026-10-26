using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using OpenIddict.Abstractions;
using VirtoCommerce.Passkeys.Core;
using VirtoCommerce.Passkeys.Core.Models;
using VirtoCommerce.Passkeys.Core.Services;
using VirtoCommerce.Passkeys.Data.Services;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Security.SignInLog;
using VirtoCommerce.Platform.Security;
using VirtoCommerce.Platform.Security.OpenIddict;
using Xunit;

namespace VirtoCommerce.Passkeys.Tests;

[Trait("Category", "Unit")]
public class PasskeyGrantTypeHandlerTests
{
    private const string _credential = "credential";

    private readonly Mock<IPasskeyService> _passkeyService = new();
    private readonly Mock<IEventPublisher> _eventPublisher = new();
    private readonly Mock<UserManager<ApplicationUser>> _userManager = new(Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

    [Fact]
    public async Task HandleAsync_Should_ReturnMissingParameter_When_TheCredentialIsMissing()
    {
        // Arrange
        var context = CreateContext(credential: null);
        var handler = CreateHandler();

        // Act
        var result = await handler.HandleAsync(context);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(SecurityErrorDescriber.MissingParameters([ModuleConstants.Security.Parameters.Credential]).Code, ((TokenResponse)badRequest.Value).Code);
        _passkeyService.Verify(x => x.AuthenticateAsync(It.IsAny<string>()), Times.Never);
        VerifySignInAttempt(SignInFailureReason.MissingParameter);
    }

    [Theory]
    [InlineData(PasskeyOutcome.InvalidPasskey, ModuleConstants.Security.FailureReason.InvalidPasskey)]
    [InlineData(PasskeyOutcome.PasskeyDisabled, SignInFailureReason.NotAllowed)]
    public async Task HandleAsync_Should_RecordTheReasonAndReturnTheGenericError_When_VerificationFails(PasskeyOutcome outcome, string failureReason)
    {
        // Arrange
        _passkeyService
            .Setup(x => x.AuthenticateAsync(_credential))
            .ReturnsAsync(PasskeyResult.Fail(outcome));

        var context = CreateContext(_credential);
        var handler = CreateHandler();

        // Act
        var result = await handler.HandleAsync(context);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(SecurityErrorDescriber.LoginFailed().Code, ((TokenResponse)badRequest.Value).Code);
        VerifySignInAttempt(failureReason);
    }

    [Fact]
    public async Task HandleAsync_Should_RecordTheReasonAndReturnTheGenericError_When_AuthenticationThrows()
    {
        // Arrange
        _passkeyService
            .Setup(x => x.AuthenticateAsync(_credential))
            .ThrowsAsync(new Exception());

        var context = CreateContext(_credential);
        var handler = CreateHandler();

        // Act
        var result = await handler.HandleAsync(context);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(SecurityErrorDescriber.LoginFailed().Code, ((TokenResponse)badRequest.Value).Code);
        VerifySignInAttempt(ModuleConstants.Security.FailureReason.InvalidPasskey);
    }

    [Theory]
    [InlineData(true, "user_is_temporary_locked_out")]
    [InlineData(false, "login_failed")]
    public async Task HandleAsync_Should_RecordLockout_When_UserIsLockedOut(bool detailedErrors, string expectedErrorCode)
    {
        // Arrange
        var user = new ApplicationUser { Id = "user-id", UserName = "buyer" };

        _passkeyService
            .Setup(x => x.AuthenticateAsync(_credential))
            .ReturnsAsync(PasskeyResult.Succeed(user));

        _userManager
            .Setup(x => x.IsLockedOutAsync(user))
            .ReturnsAsync(true);

        var context = CreateContext(_credential);
        context.DetailedErrors = detailedErrors;

        var handler = CreateHandler();

        // Act
        var result = await handler.HandleAsync(context);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(expectedErrorCode, ((TokenResponse)badRequest.Value).Code);
        VerifySignInAttempt(SignInFailureReason.LockedOut);
    }

    private PasskeyGrantTypeHandler CreateHandler()
    {
        var signInManager = new Mock<SignInManager<ApplicationUser>>(
            _userManager.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
            null, null, null, null);

        signInManager
            .Setup(x => x.CanSignInAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(true);

        return new PasskeyGrantTypeHandler(
            signInManager.Object,
            Options.Create(new IdentityOptions()),
            requestValidators: [],
            claimProviders: [],
            requestHandlers: [],
            _eventPublisher.Object,
            Mock.Of<ITwoFactorSignInService>(),
            _passkeyService.Object,
            NullLogger<PasskeyGrantTypeHandler>.Instance);
    }

    private static TokenRequestContext CreateContext(string credential)
    {
        var request = new OpenIddictRequest { GrantType = ModuleConstants.Security.GrantType };

        if (credential != null)
        {
            request.SetParameter(ModuleConstants.Security.Parameters.Credential, credential);
        }

        return new TokenRequestContext
        {
            AuthenticationScheme = IdentityConstants.ApplicationScheme,
            Request = request,
        };
    }

    private void VerifySignInAttempt(string failureReason)
    {
        _eventPublisher.Verify(x => x.Publish(It.Is<UserSignInAttemptEvent>(e =>
            !e.Succeeded &&
            e.FailureReason == failureReason &&
            e.SignInType == ModuleConstants.Security.SignInType)), Times.Once);
    }
}
