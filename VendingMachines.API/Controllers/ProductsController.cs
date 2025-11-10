using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendingMachines.Infrastructure.Data;

namespace VendingMachines.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly VendingMachinesContext _context;

        public ProductsController(VendingMachinesContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetProductsAsync([FromQuery] int limit = 10, [FromQuery] int offset = 0)
        {
            var products = await _context.Products
                .OrderBy(product => product.Id)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            return Ok(products);
        }
    }
}
