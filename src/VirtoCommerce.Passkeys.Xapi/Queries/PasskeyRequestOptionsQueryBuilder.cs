using GraphQL.Types;
using Microsoft.AspNetCore.Authorization;
using VirtoCommerce.Xapi.Core.BaseQueries;

namespace VirtoCommerce.Passkeys.Xapi.Queries;

public class PasskeyRequestOptionsQueryBuilder(IAuthorizationService authorizationService)
    : QueryBuilder<PasskeyRequestOptionsQuery, string, StringGraphType>
        (authorizationService)
{
    protected override string Name => "passkeyRequestOptions";
}
