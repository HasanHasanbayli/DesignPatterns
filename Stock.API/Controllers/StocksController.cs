using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stock.API.Context;

namespace Stock.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StocksController : ControllerBase
{
    private readonly ApplicationDbContext _applicationDbContext;

    public StocksController(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return Ok(await _applicationDbContext.Stocks.ToListAsync());
    }
}