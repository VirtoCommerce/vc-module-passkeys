using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using VirtoCommerce.Passkeys.Core.Models;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.Passkeys.Core.Services;

public interface IPasskeyService
{
    Task<string> MakeRequestOptionsAsync();

    Task<PasskeyResult> AuthenticateAsync(string credential);

    Task<string> MakeCreationOptionsAsync(ApplicationUser user);

    Task<IdentityResult> CreateAsync(ApplicationUser user, string credential, string name);
}
