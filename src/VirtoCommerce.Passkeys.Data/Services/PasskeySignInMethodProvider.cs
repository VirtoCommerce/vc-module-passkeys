using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Formats.Cbor;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using VirtoCommerce.Passkeys.Core.Models;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Security.SignInMethods;
using static VirtoCommerce.Passkeys.Core.ModuleConstants.Security;

namespace VirtoCommerce.Passkeys.Data.Services;

public class PasskeySignInMethodProvider(
    IOptions<PasskeyOptions> passkeyOptions,
    UserManager<ApplicationUser> userManager)
    : ISignInMethodProvider
{
    public virtual async Task<IList<SignInMethod>> GetSignInMethodsAsync(ApplicationUser user = null)
    {
        if (!passkeyOptions.Value.Enabled)
        {
            return [];
        }

        var signInMethod = AbstractTypeFactory<SignInMethod>.TryCreateInstance();

        signInMethod.Kind = SignInType;
        signInMethod.Name = SignInType;
        signInMethod.DisplayName = SignInType;
        signInMethod.CanCreateKey = true;
        signInMethod.CanRenameKey = true;
        signInMethod.CanDeleteKey = true;

        if (user != null)
        {
            var passkeys = await userManager.GetPasskeysAsync(user);
            signInMethod.Keys = passkeys.Select(BuildSignInKey).ToList();
        }

        return [signInMethod];
    }

    public virtual async Task<IdentityResult> RenameKeyAsync(ApplicationUser user, string signInMethodName, string keyId, string name)
    {
        var passkey = await userManager.GetPasskeyAsync(user, Base64Url.DecodeFromChars(keyId));
        passkey.Name = name.WhiteSpaceToNull();

        return await userManager.AddOrUpdatePasskeyAsync(user, passkey);
    }

    public virtual Task<IdentityResult> DeleteKeyAsync(ApplicationUser user, string signInMethodName, string keyId)
    {
        return userManager.RemovePasskeyAsync(user, Base64Url.DecodeFromChars(keyId));
    }

    protected virtual SignInKey BuildSignInKey(UserPasskeyInfo passkey)
    {
        var key = AbstractTypeFactory<SignInKey>.TryCreateInstance();

        key.Id = Base64Url.EncodeToString(passkey.CredentialId);
        key.Name = GetKeyName(passkey);
        key.CreatedDate = passkey.CreatedAt.UtcDateTime;
        key.State = GetKeyState(passkey);

        return key;
    }

    protected virtual string GetKeyName(UserPasskeyInfo passkey)
    {
        if (!string.IsNullOrWhiteSpace(passkey.Name))
        {
            return passkey.Name;
        }

        var aaguid = ReadAaguid(passkey.AttestationObject);

        return KnownAuthenticators.GetName(aaguid);
    }

    private static Guid? ReadAaguid(byte[] attestationObject)
    {
        if (attestationObject is null)
        {
            return null;
        }

        // rpIdHash + flags + signCount precede the AAGUID in authenticator data
        const int aaguidOffset = 32 + 1 + 4;
        const int aaguidLength = 16;

        var reader = new CborReader(attestationObject, CborConformanceMode.Lax);
        reader.ReadStartMap();

        while (reader.PeekState() != CborReaderState.EndMap)
        {
            if (reader.ReadTextString() != "authData")
            {
                reader.SkipValue();
                continue;
            }

            var authData = reader.ReadByteString();

            return authData.Length >= aaguidOffset + aaguidLength
                ? new Guid(authData.AsSpan(aaguidOffset, aaguidLength), bigEndian: true)
                : null;
        }

        return null;
    }

    protected virtual string GetKeyState(UserPasskeyInfo passkey)
    {
        if (!passkey.IsBackupEligible)
        {
            return KeyState.DeviceOnly;
        }

        return passkey.IsBackedUp
            ? KeyState.BackedUp
            : KeyState.NotBackedUp;
    }
}
