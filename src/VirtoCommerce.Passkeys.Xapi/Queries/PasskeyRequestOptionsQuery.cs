using System.Collections.Generic;
using GraphQL;
using GraphQL.Types;
using VirtoCommerce.Xapi.Core.BaseQueries;

namespace VirtoCommerce.Passkeys.Xapi.Queries;

public class PasskeyRequestOptionsQuery : Query<string>
{
    public string StoreId { get; set; }

    public override IEnumerable<QueryArgument> GetArguments()
    {
        yield return Argument<NonNullGraphType<StringGraphType>>(nameof(StoreId));
    }

    public override void Map(IResolveFieldContext context)
    {
        StoreId = context.GetArgument<string>(nameof(StoreId));
    }
}
