using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendingMachines.Infrastructure.Data;

namespace VendingMachines.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SalesController : ControllerBase
    {
        private readonly VendingMachinesContext _context;

        public SalesController(VendingMachinesContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetSalesAsync([FromQuery] int? deviceId,
            [FromQuery] int limit = 10, [FromQuery] int offset = 0)
        {
            var query = _context.Sales
                .Include(sale => sale.Device)
                .AsQueryable();

            if (deviceId.HasValue)
            {
                query = query.Where(device => device.Id == deviceId);
            }

            var sales = await query
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            return Ok(sales);
        }
    }
}
