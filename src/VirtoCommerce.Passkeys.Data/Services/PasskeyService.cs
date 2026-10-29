using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using VirtoCommerce.Passkeys.Core.Models;
using VirtoCommerce.Passkeys.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Security;

namespace VirtoCommerce.Passkeys.Data.Services;

public class PasskeyService(
    SignInManager<ApplicationUser> signInManager,
    IOptions<PasskeyOptions> passkeyOptions,
    IOptions<AuthorizationOptions> authorizationOptions)
    : IPasskeyService
{
    private readonly UserManager<ApplicationUser> _userManager = signInManager.UserManager;

    public virtual Task<string> MakeRequestOptionsAsync()
    {
        return passkeyOptions.Value.Enabled
            ? signInManager.MakePasskeyRequestOptionsAsync(user: null)
            : Task.FromResult<string>(null);
    }

    public virtual async Task<PasskeyResult> AuthenticateAsync(string credential)
    {
        if (!passkeyOptions.Value.Enabled)
        {
            return PasskeyResult.Fail(PasskeyOutcome.PasskeyDisabled);
        }

        var assertionResult = await signInManager.PerformPasskeyAssertionAsync(credential);
        if (!assertionResult.Succeeded)
        {
            return PasskeyResult.Fail(PasskeyOutcome.InvalidPasskey);
        }

        var user = assertionResult.User;

        // Like SignInManager.PasskeySignInAsync, update the sign count and authenticator data
        var updateResult = await _userManager.AddOrUpdatePasskeyAsync(user, assertionResult.Passkey);
        if (!updateResult.Succeeded)
        {
            return PasskeyResult.Fail(PasskeyOutcome.InvalidPasskey, user);
        }

        return PasskeyResult.Succeed(user);
    }

    public virtual Task<string> MakeCreationOptionsAsync(ApplicationUser user)
    {
        if (!passkeyOptions.Value.Enabled)
        {
            return Task.FromResult<string>(null);
        }

        var userEntity = new PasskeyUserEntity
        {
            Id = user.Id,
            Name = user.UserName,
            DisplayName = user.UserName,
        };

        return signInManager.MakePasskeyCreationOptionsAsync(userEntity);
    }

    public virtual async Task<IdentityResult> CreateAsync(ApplicationUser user, string credential, string name)
    {
        if (!passkeyOptions.Value.Enabled)
        {
            return IdentityResult.Failed(PasskeyErrorDescriber.PasskeysDisabled());
        }

        if (authorizationOptions.Value.NonEditableUsers?.Any(x => x.EqualsIgnoreCase(user.UserName)) == true)
        {
            return IdentityResult.Failed(CommonErrorDescriber.UserNotEditable());
        }

        var attestationResult = await signInManager.PerformPasskeyAttestationAsync(credential);
        if (!attestationResult.Succeeded)
        {
            return IdentityResult.Failed(PasskeyErrorDescriber.InvalidPasskeyAttestation(attestationResult.Failure.Message));
        }

        // The attestation state names the account, so it must be the given user
        if (attestationResult.UserEntity.Id != user.Id)
        {
            return IdentityResult.Failed(PasskeyErrorDescriber.PasskeyForAnotherUser());
        }

        var passkey = attestationResult.Passkey;
        passkey.Name = name.WhiteSpaceToNull();

        return await _userManager.AddOrUpdatePasskeyAsync(user, passkey);
    }
}
