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
            Description = "Поддерживает фильтрацию по компании и пагинацию.")]
        [ProducesResponseType(typeof(List<Contract>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetContractsAsync([FromQuery] int? companyId,
            [FromQuery] int limit = 10, [FromQuery] int offset = 0)
        {
            var query = _context.Contracts
                .Include(contract => contract.Company)
                .AsQueryable();

            if (companyId.HasValue)
            {
                query = query.Where(c => c.Id == companyId);
            }

            var contracts = await query.OrderBy(c => c.Id)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            return Ok(contracts);
        }
    }
}
