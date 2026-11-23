using System.Threading;
using System.Threading.Tasks;
using GraphQL;
using VirtoCommerce.Passkeys.Core.Services;
using VirtoCommerce.Passkeys.Data.Services;
using VirtoCommerce.Xapi.Core.Infrastructure;

namespace VirtoCommerce.Passkeys.Xapi.Queries;

public class PasskeyRequestOptionsQueryHandler(IPasskeyService passkeyService)
    : IQueryHandler<PasskeyRequestOptionsQuery, string>
{
    public virtual async Task<string> Handle(PasskeyRequestOptionsQuery request, CancellationToken cancellationToken)
    {
        if (!await passkeyService.IsPasskeyEnabledAsync(request.StoreId))
        {
            var error = PasskeyErrorDescriber.PasskeysDisabled();
            throw new ExecutionError(error.Description) { Code = error.Code };
        }

        return await passkeyService.MakeRequestOptionsAsync();
    }
}
