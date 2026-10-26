using Microsoft.AspNetCore.Identity;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.Passkeys.Core.Models;

public class PasskeyResult
{
    public PasskeyOutcome Outcome { get; set; }

    public ApplicationUser User { get; set; }

    public static PasskeyResult Succeed(ApplicationUser user)
    {
        return new PasskeyResult
        {
            Outcome = PasskeyOutcome.Success,
            User = user,
        };
    }

    public static PasskeyResult Fail(PasskeyOutcome outcome, ApplicationUser user = null)
    {
        return new PasskeyResult
        {
            Outcome = outcome,
            User = user,
        };
    }

    public SignInResult ToSignInResult()
    {
        return Outcome switch
        {
            PasskeyOutcome.Success => SignInResult.Success,
            PasskeyOutcome.PasskeyDisabled => SignInResult.NotAllowed,
            _ => SignInResult.Failed,
        };
    }
}
