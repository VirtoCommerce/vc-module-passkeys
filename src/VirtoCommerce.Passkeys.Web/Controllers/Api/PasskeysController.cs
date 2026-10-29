using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.Passkeys.Core.Models;
using VirtoCommerce.Passkeys.Core.Services;
using VirtoCommerce.Platform.Core.Extensions;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Security.Extensions;

namespace VirtoCommerce.Passkeys.Web.Controllers.Api;

[Authorize]
[Route("api/passkeys")]
public class PasskeysController(
    UserManager<ApplicationUser> userManager,
    IPasskeyService passkeyService)
    : Controller
{
    /// <summary>
    /// Get WebAuthn options for signing in with a passkey
    /// </summary>
    [HttpPost]
    [Route("request-options")]
    [AllowAnonymous]
    public async Task<ActionResult> GetRequestOptions()
    {
        var options = await passkeyService.MakeRequestOptionsAsync();
        if (options is null)
        {
            return BadRequest();
        }

        return Content(options, "application/json");
    }

    /// <summary>
    /// Get WebAuthn options for creating a passkey for the current user
    /// </summary>
    [HttpPost]
    [Route("creation-options")]
    public async Task<ActionResult> GetCreationOptions()
    {
        if (User.IsImpersonated())
        {
            return Forbid();
        }

        var user = await GetCurrentUserAsync();
        if (user is null)
        {
            return Unauthorized();
        }

        var options = await passkeyService.MakeCreationOptionsAsync(user);
        if (options is null)
        {
            return BadRequest();
        }

        return Content(options, "application/json");
    }

    /// <summary>
    /// Create a passkey for the current user
    /// </summary>
    [HttpPost]
    [Route("")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    public async Task<ActionResult> CreatePasskey([FromBody] PasskeyCreateRequest request)
    {
        if (User.IsImpersonated())
        {
            return Forbid();
        }

        var user = await GetCurrentUserAsync();
        if (user is null)
        {
            return Unauthorized();
        }

        if (string.IsNullOrEmpty(request?.Credential))
        {
            return BadRequest();
        }

        var result = await passkeyService.CreateAsync(user, request.Credential, request.Name);
        if (!result.Succeeded)
        {
            return BadRequest(result.ToSecurityResult());
        }

        return NoContent();
    }

    private Task<ApplicationUser> GetCurrentUserAsync()
    {
        if (string.IsNullOrEmpty(User.Identity?.Name) || !User.Identity.IsAuthenticated)
        {
            return Task.FromResult<ApplicationUser>(null);
        }

        return userManager.FindByNameAsync(User.Identity.Name);
    }
}
