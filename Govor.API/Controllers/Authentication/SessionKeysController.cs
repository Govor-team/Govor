using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Govor.API.Controllers.Authentication;

[RequireHttps]
[ApiController]
[Route("api/session")]
[Authorize(Roles = "Admin, User")]
public class SessionKeysController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}