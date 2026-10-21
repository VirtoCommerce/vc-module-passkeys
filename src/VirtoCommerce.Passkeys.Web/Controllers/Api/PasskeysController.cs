using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Permissions = VirtoCommerce.Passkeys.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.Passkeys.Web.Controllers.Api;

[Authorize]
[Route("api/passkeys")]
public class PasskeysController : Controller
{
    // GET: api/passkeys
    /// <summary>
    /// Get message
    /// </summary>
    /// <remarks>Return "Hello world!" message</remarks>
    [HttpGet]
    [Route("")]
    [Authorize(Permissions.Read)]
    public ActionResult<string> Get()
    {
        return Ok(new { result = "Hello world!" });
    }
}
