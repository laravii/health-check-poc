using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class RadomController : ControllerBase
{
    private readonly ILogger<RadomController> _logger;

    public RadomController(ILogger<RadomController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "Healthly")]
    public IActionResult Get()
    {
        var random = new Random();
        var result = random.Next(0,2);

        return result > 0 ? Ok() : StatusCode(500);
    }
}
