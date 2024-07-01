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

    [HttpPost("Authenticated/{name}")]
    public IActionResult AuthenticatedPost(string name)
    {
        return Ok(name);
    }

    [AllowAnonymous]
    [HttpPost("Anonymous/{name}")]
    public IActionResult AnonymousPost(string name)
    {
        return Ok(name);
    }

// TODO: Create test for this method
    [ValidateAntiForgeryToken]
    [HttpPost("Authenticated/Antiforgery/{name}")]
    public IActionResult AuthenticatedAntiforgeryPost(string name)
    {
        return Ok(name);
    }

    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [HttpPost("Anonymous/Antiforgery/{name}")]
    public IActionResult AnonymousAntiforgeryPost(string name)
    {
        return Ok(name);
    }
}