using System;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Types;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Security.Extensions;
using VirtoCommerce.Xapi.Core.Extensions;
using VirtoCommerce.Xapi.Core.Security.Authorization;
using VirtoCommerce.XFrontend.Data.Queries;

namespace VirtoCommerce.Passkeys.Xapi.Queries;

public class PasskeyCreationOptionsQueryBuilder(
    IAuthorizationService authorizationService,
    Func<UserManager<ApplicationUser>> userManagerFactory)
    : SignInQueryBuilder<PasskeyCreationOptionsQuery, string, StringGraphType>
        (authorizationService, userManagerFactory)
{
    protected override string Name => "passkeyCreationOptions";

    protected override async Task BeforeMediatorSend(IResolveFieldContext<object> context, PasskeyCreationOptionsQuery request)
    {
        if (context.GetCurrentPrincipal().IsImpersonated())
        {
            throw AuthorizationError.Forbidden();
        }

        await base.BeforeMediatorSend(context, request);
    }
}
