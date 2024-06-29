using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Antiforgery.Controllers;
[ApiController]
[Route("[controller]")]
public class ApiController : ControllerBase
{

    [HttpGet]
    public IActionResult Get()
    {
        return Ok();
    }

    [HttpPost("Authenticated")]
    public IActionResult AuthenticatedPost(string name)
    {
        return Ok(name);
    }

// TODO Make test for these endpoints
// Get to the point where these work then add in antiforgery stuff
    [AllowAnonymous]
    [HttpPost("Anonymous")]
    public IActionResult AnonymousPost(string name)
    {
        return Ok(name);
    }
}