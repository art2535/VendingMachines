using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendingMachines.Infrastructure.Data;

namespace VendingMachines.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ContractsController : ControllerBase
    {
        private readonly VendingMachinesContext _context;

        public ContractsController(VendingMachinesContext context)
        {
            _context = context;
        }

        [HttpGet]
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
