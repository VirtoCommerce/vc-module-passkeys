using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using VirtoCommerce.Passkeys.Xapi.Schemas;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.ProfileExperienceApiModule.Data.Schemas;
using VirtoCommerce.XFrontend.Data.Commands;

namespace VirtoCommerce.Passkeys.Xapi.Commands;

public class CreatePasskeyCommandBuilder(
    IAuthorizationService authorizationService,
    Func<UserManager<ApplicationUser>> userManagerFactory)
    : SignInKeyCommandBuilder<CreatePasskeyCommand, IdentityResult, InputCreatePasskeyType, IdentityResultType>
        (authorizationService, userManagerFactory)
{
    protected override string Name => "createPasskey";
}
