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
    [Produces("application/json")]
    [Consumes("application/json")]
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
            Description = "Возвращает продажи с информацией об аппаратах. Поддерживает фильтрацию по устройству и пагинацию")]
        [SwaggerResponse(StatusCodes.Status200OK, "Список продаж получен", typeof(List<Sale>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        public async Task<IActionResult> GetSalesAsync(
            [FromQuery][SwaggerParameter(Description = "ID аппарата для фильтрации (опционально)")] int? deviceId = null,
            [FromQuery][SwaggerParameter(Description = "Количество продаж на странице (по умолчанию 10)")] int limit = 10,
            [FromQuery][SwaggerParameter(Description = "Смещение для пагинации (по умолчанию 0)")] int offset = 0)
        {
            var query = _context.Sales
                .Include(sale => sale.Device)
                .AsQueryable();

            if (deviceId.HasValue)
            {
                query = query.Where(s => s.DeviceId == deviceId);
            }

            var sales = await query
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            return Ok(sales);
        }
    }
}
