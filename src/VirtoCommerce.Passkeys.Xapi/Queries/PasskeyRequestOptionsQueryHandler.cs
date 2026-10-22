using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.Passkeys.Core.Services;
using VirtoCommerce.Xapi.Core.Infrastructure;

namespace VirtoCommerce.Passkeys.Xapi.Queries;

public class PasskeyRequestOptionsQueryHandler(IPasskeyService passkeyService)
    : IQueryHandler<PasskeyRequestOptionsQuery, string>
{
    public virtual Task<string> Handle(PasskeyRequestOptionsQuery request, CancellationToken cancellationToken)
    {
        return passkeyService.MakeRequestOptionsAsync();
    }
}
