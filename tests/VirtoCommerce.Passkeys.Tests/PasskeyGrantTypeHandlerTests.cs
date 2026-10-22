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
using VirtoCommerce.Platform.Security.OpenIddict;
using Xunit;

namespace VirtoCommerce.Passkeys.Tests;

[Trait("Category", "Unit")]
public class PasskeyGrantTypeHandlerTests
{
    private const string _credential = "credential";

    private readonly Mock<IPasskeyService> _passkeyService = new();
    private readonly Mock<IEventPublisher> _eventPublisher = new();

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
    [InlineData(PasskeyOutcome.AccountLocked, SignInFailureReason.LockedOut)]
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

    [Fact]
    public async Task HandleAsync_Should_ReturnTheLockoutError_When_TheAccountIsLockedAndDetailedErrorsAreOn()
    {
        // Arrange
        var user = new ApplicationUser { Id = "user-id", UserName = "buyer" };

        _passkeyService
            .Setup(x => x.AuthenticateAsync(_credential))
            .ReturnsAsync(PasskeyResult.Fail(PasskeyOutcome.AccountLocked, user));

        var context = CreateContext(_credential);
        context.DetailedErrors = true;

        var handler = CreateHandler();

        // Act
        var result = await handler.HandleAsync(context);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(SecurityErrorDescriber.GetLockoutError(user).Code, ((TokenResponse)badRequest.Value).Code);
        VerifySignInAttempt(SignInFailureReason.LockedOut);
    }

    private PasskeyGrantTypeHandler CreateHandler()
    {
        var userManager = new Mock<UserManager<ApplicationUser>>(Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

        var signInManager = new Mock<SignInManager<ApplicationUser>>(
            userManager.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
            null, null, null, null);

        return new PasskeyGrantTypeHandler(
            signInManager.Object,
            Options.Create(new IdentityOptions()),
            requestValidators: [],
            claimProviders: [],
            requestHandlers: [],
            _eventPublisher.Object,
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
