using System;
using GraphQL.Types;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.XFrontend.Data.Queries;

namespace VirtoCommerce.Passkeys.Xapi.Queries;

public class PasskeyCreationOptionsQueryBuilder(
    IAuthorizationService authorizationService,
    Func<UserManager<ApplicationUser>> userManagerFactory)
    : SignInQueryBuilder<PasskeyCreationOptionsQuery, string, StringGraphType>
        (authorizationService, userManagerFactory)
{
    protected override string Name => "passkeyCreationOptions";
}
