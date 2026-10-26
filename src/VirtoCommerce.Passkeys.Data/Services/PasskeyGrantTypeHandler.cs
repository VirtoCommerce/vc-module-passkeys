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

        PasskeyResult result;

        try
        {
            result = await passkeyService.AuthenticateAsync(credential);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Passkey authentication failed");
            result = PasskeyResult.Fail(PasskeyOutcome.InvalidPasskey);
        }

        context.SignInResult = result.ToSignInResult();

        if (result.Outcome != PasskeyOutcome.Success)
        {
            return GrantValidationResult.Fail(BuildErrorResponse(context, result), result.User);
        }

        return GrantValidationResult.Succeed(result.User);
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
