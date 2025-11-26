using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendingMachines.Infrastructure.Data;
using Swashbuckle.AspNetCore.Annotations;
using VendingMachines.Core.Models;

namespace VendingMachines.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [SwaggerTag("Контроллер для работы с продажами")]
    public class SalesController : ControllerBase
    {
        private readonly VendingMachinesContext _context;

        public SalesController(VendingMachinesContext context)
        {
            _context = context;
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Получение списка продаж",
            Description = "Поддерживает фильтрацию по устройству и пагинацию.")]
        [ProducesResponseType(typeof(List<Sale>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
