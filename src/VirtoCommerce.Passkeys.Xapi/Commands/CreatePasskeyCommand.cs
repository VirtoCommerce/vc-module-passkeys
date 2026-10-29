using Microsoft.AspNetCore.Identity;
using VirtoCommerce.XFrontend.Core.Commands;

namespace VirtoCommerce.Passkeys.Xapi.Commands;

public class CreatePasskeyCommand : SignInKeyCommand<IdentityResult>
{
    public string Credential { get; set; }

    public string Name { get; set; }
}
