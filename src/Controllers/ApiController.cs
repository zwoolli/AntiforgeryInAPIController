using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AntiforgeryInAPIController.Controllers;
[ApiController]
[Route("[controller]")]
public class ApiController : ControllerBase
{

    [HttpGet]
    public IActionResult Get()
    {
        return Ok();
    }

    [HttpPost]
    public void Post(string name)
    {
        Debug.WriteLine(name);
    }
}