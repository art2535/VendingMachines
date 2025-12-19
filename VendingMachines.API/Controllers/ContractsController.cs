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
    [SwaggerTag("Контроллер для работы с договорами")]
    public class ContractsController : ControllerBase
    {
        private readonly VendingMachinesContext _context;

        public ContractsController(VendingMachinesContext context)
        {
            _context = context;
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Получение списка договоров",
            Description = "Возвращает договоры с информацией о компаниях. Поддерживает фильтрацию по компании и пагинацию")]
        [SwaggerResponse(StatusCodes.Status200OK, "Список договоров получен", typeof(List<Contract>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        public async Task<IActionResult> GetContractsAsync(
            [FromQuery][SwaggerParameter(Description = "ID компании для фильтрации (опционально)")] int? companyId = null,
            [FromQuery][SwaggerParameter(Description = "Количество договоров на странице (по умолчанию 10)")] int limit = 10,
            [FromQuery][SwaggerParameter(Description = "Смещение для пагинации (по умолчанию 0)")] int offset = 0)
        {
            var query = _context.Contracts
                .Include(contract => contract.Company)
                .AsQueryable();

            if (companyId.HasValue)
            {
                query = query.Where(c => c.CompanyId == companyId);
            }

            var contracts = await query.OrderBy(c => c.Id)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            return Ok(contracts);
        }
    }
}
