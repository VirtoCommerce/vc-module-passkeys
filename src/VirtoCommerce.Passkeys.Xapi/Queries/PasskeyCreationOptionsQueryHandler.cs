using System.Threading;
using System.Threading.Tasks;
using GraphQL;
using VirtoCommerce.Passkeys.Core.Services;
using VirtoCommerce.Passkeys.Data.Services;
using VirtoCommerce.Xapi.Core.Infrastructure;

namespace VirtoCommerce.Passkeys.Xapi.Queries;

public class PasskeyCreationOptionsQueryHandler(
    IPasskeyService passkeyService)
    : IQueryHandler<PasskeyCreationOptionsQuery, string>
{
    public virtual async Task<string> Handle(PasskeyCreationOptionsQuery request, CancellationToken cancellationToken)
    {
        if (!await passkeyService.IsPasskeyEnabledAsync(request.StoreId))
        {
            var error = PasskeyErrorDescriber.PasskeysDisabled();
            throw new ExecutionError(error.Description) { Code = error.Code };
        }

        return await passkeyService.MakeCreationOptionsAsync(request.User);
    }
}
