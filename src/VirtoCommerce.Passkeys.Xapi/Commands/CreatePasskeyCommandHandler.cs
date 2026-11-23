using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Identity;
using VirtoCommerce.Passkeys.Core.Services;
using VirtoCommerce.Passkeys.Data.Services;

namespace VirtoCommerce.Passkeys.Xapi.Commands;

public class CreatePasskeyCommandHandler(IPasskeyService passkeyService)
    : IRequestHandler<CreatePasskeyCommand, IdentityResult>
{
    public virtual async Task<IdentityResult> Handle(CreatePasskeyCommand request, CancellationToken cancellationToken)
    {
        if (!await passkeyService.IsPasskeyEnabledAsync(request.StoreId))
        {
            return IdentityResult.Failed(PasskeyErrorDescriber.PasskeysDisabled());
        }

        return await passkeyService.CreateAsync(request.User, request.Credential, request.Name);
    }
}
