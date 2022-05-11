using Microsoft.AspNetCore.Mvc;

namespace Order.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create()
    {
        return Ok();
    }
}