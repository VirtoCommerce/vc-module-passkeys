using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VirtoCommerce.Passkeys.Core;
using VirtoCommerce.Passkeys.Core.Models;
using VirtoCommerce.Passkeys.Core.Services;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Security.SignInLog;
using VirtoCommerce.Platform.Security;
using VirtoCommerce.Platform.Security.OpenIddict;

namespace VirtoCommerce.Passkeys.Data.Services;

public class PasskeyGrantTypeHandler(
    SignInManager<ApplicationUser> signInManager,
    IOptions<IdentityOptions> identityOptions,
    IEnumerable<ITokenRequestValidator> requestValidators,
    IEnumerable<ITokenClaimProvider> claimProviders,
    IEnumerable<ITokenRequestHandler> requestHandlers,
    IEventPublisher eventPublisher,
    ITwoFactorSignInService twoFactorSignInService,
    IPasskeyService passkeyService,
    ILogger<PasskeyGrantTypeHandler> logger)
    : GrantTypeHandlerBase(signInManager, identityOptions, requestValidators, claimProviders, requestHandlers, eventPublisher, twoFactorSignInService)
{
    public override string GrantType => ModuleConstants.Security.GrantType;

    protected override string SignInType => ModuleConstants.Security.SignInType;

    protected override async Task<GrantValidationResult> ValidateGrantAsync(TokenRequestContext context)
    {
        var missingParameters = new List<string>();
        var credential = GetRequiredParameter(context.Request, ModuleConstants.Security.Parameters.Credential, missingParameters);

        if (missingParameters.Count > 0)
        {
            return GrantValidationResult.Fail(BuildErrorResponse(context, missingParameters));
        }

        var storeId = (string)context.Request.GetParameter(ModuleConstants.Security.Parameters.StoreId);
        var result = await AuthenticateAsync(storeId, credential);

        context.SignInResult = result.ToSignInResult();

        return result.Outcome == PasskeyOutcome.Success
            ? GrantValidationResult.Succeed(result.User)
            : GrantValidationResult.Fail(BuildErrorResponse(context, result), result.User);
    }

    protected virtual async Task<PasskeyResult> AuthenticateAsync(string storeId, string credential)
    {
        if (!string.IsNullOrEmpty(storeId) && !await passkeyService.IsPasskeyEnabledAsync(storeId))
        {
            return PasskeyResult.Fail(PasskeyOutcome.PasskeyDisabled);
        }

        try
        {
            return await passkeyService.AuthenticateAsync(credential);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Passkey authentication failed");
            return PasskeyResult.Fail(PasskeyOutcome.InvalidPasskey);
        }
    }

    protected override Task<bool> IsTwoFactorRequiredAsync(TokenRequestContext context)
    {
        // Passkey sign-in skips two-factor, as SignInManager.PasskeySignInAsync does
        return Task.FromResult(false);
    }

    protected virtual TokenResponse BuildErrorResponse(TokenRequestContext context, PasskeyResult result)
    {
        // The sign-in log always gets the real reason
        context.FailureReason = result.Outcome switch
        {
            PasskeyOutcome.PasskeyDisabled => SignInFailureReason.NotAllowed,
            _ => ModuleConstants.Security.FailureReason.InvalidPasskey,
        };

        return SecurityErrorDescriber.LoginFailed();
    }
}
