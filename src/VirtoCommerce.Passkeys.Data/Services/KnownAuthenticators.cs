using System;
using System.Collections.Generic;

namespace VirtoCommerce.Passkeys.Data.Services;

public static class KnownAuthenticators
{
    // AAGUID names come from https://github.com/passkeydeveloper/passkey-authenticator-aaguids
    private static readonly Dictionary<Guid, string> _names = new()
    {
        [new("bada5566-a7aa-401f-bd96-45619a55120d")] = "1Password",
        [new("fbfc3007-154e-4ecc-8c0b-6e020557d7bd")] = "Apple Passwords",
        [new("d548826e-79b4-db40-a3d8-11116f7e8349")] = "Bitwarden",
        [new("adce0002-35bc-c60a-648b-0b25f1f05503")] = "Chrome on Mac",
        [new("531126d6-e717-415c-9320-3d9aa6981239")] = "Dashlane",
        [new("771b48fd-d3d4-4f74-9232-fc157ab0507a")] = "Edge on Mac",
        [new("f3809540-7f14-49c1-a8b3-8f813b225541")] = "Enpass",
        [new("ea9b8d66-4d01-1d21-3ce4-b6b48cb575d4")] = "Google Password Manager",
        [new("dd4ec289-e01d-41c9-bb89-70fa845d4bf2")] = "iCloud Keychain (Managed)",
        [new("fdb141b2-5d84-443e-8a35-4698c205a502")] = "KeePassXC",
        [new("0ea242b4-43c4-4a1b-8b17-dd6d0b6baec6")] = "Keeper",
        [new("b78a0a55-6ef8-d246-a042-ba0f6d55050c")] = "LastPass",
        [new("d3452668-01fd-4c12-926c-83a4204853aa")] = "Microsoft Password Manager",
        [new("b84e4048-15dc-4dd0-8640-f4f60813c8af")] = "NordPass",
        [new("50726f74-6f6e-5061-7373-50726f746f6e")] = "Proton Pass",
        [new("53414d53-554e-4700-0000-000000000000")] = "Samsung Pass",
        [new("08987058-cadc-4b81-b6e1-30de50dcbe96")] = "Windows Hello",
        [new("6028b017-b1d4-4c02-b4b3-afcdafc96bb2")] = "Windows Hello",
        [new("9ddd1817-af5a-4672-a2b9-3e3dd95000a9")] = "Windows Hello",
    };

    public static string GetName(Guid? aaguid)
    {
        return aaguid.HasValue && _names.TryGetValue(aaguid.Value, out var name)
            ? name
            : null;
    }
}
