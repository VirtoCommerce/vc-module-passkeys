using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Identity;
using VirtoCommerce.Passkeys.Core.Services;

namespace VirtoCommerce.Passkeys.Xapi.Commands;

public class CreatePasskeyCommandHandler(IPasskeyService passkeyService)
    : IRequestHandler<CreatePasskeyCommand, IdentityResult>
{
    public virtual Task<IdentityResult> Handle(CreatePasskeyCommand request, CancellationToken cancellationToken)
    {
        return passkeyService.CreateAsync(request.User, request.Credential, request.Name);
    }
}
