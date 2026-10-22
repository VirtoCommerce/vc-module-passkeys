using System.Collections.Generic;
using GraphQL;
using GraphQL.Types;
using VirtoCommerce.Xapi.Core.BaseQueries;

namespace VirtoCommerce.Passkeys.Xapi.Queries;

public class PasskeyRequestOptionsQuery : Query<string>
{
    public override IEnumerable<QueryArgument> GetArguments()
    {
        yield break;
    }

    public override void Map(IResolveFieldContext context)
    {
    }
}
