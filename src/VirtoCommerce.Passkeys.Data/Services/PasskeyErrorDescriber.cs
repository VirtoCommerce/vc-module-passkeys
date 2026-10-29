using Microsoft.AspNetCore.Identity;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Passkeys.Data.Services;

public static class PasskeyErrorDescriber
{
    public static IdentityError PasskeysDisabled()
    {
        return new IdentityError
        {
            Code = nameof(PasskeysDisabled).ToSnakeCase(),
            Description = "Passkeys are disabled.",
        };
    }

    public static IdentityError InvalidPasskeyAttestation(string message)
    {
        return new IdentityError
        {
            Code = nameof(InvalidPasskeyAttestation).ToSnakeCase(),
            Description = message,
        };
    }

    public static IdentityError PasskeyForAnotherUser()
    {
        return new IdentityError
        {
            Code = nameof(PasskeyForAnotherUser).ToSnakeCase(),
            Description = "The passkey was created for another user.",
        };
    }
}
