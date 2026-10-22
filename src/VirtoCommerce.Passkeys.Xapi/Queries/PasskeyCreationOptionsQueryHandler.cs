using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.Passkeys.Core.Services;
using VirtoCommerce.Xapi.Core.Infrastructure;

namespace VirtoCommerce.Passkeys.Xapi.Queries;

public class PasskeyCreationOptionsQueryHandler(
    IPasskeyService passkeyService)
    : IQueryHandler<PasskeyCreationOptionsQuery, string>
{
    public virtual async Task<string> Handle(PasskeyCreationOptionsQuery request, CancellationToken cancellationToken)
    {
        return await passkeyService.MakeCreationOptionsAsync(request.User);
    }
}
