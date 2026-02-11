using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VendingMachines.API.DTOs;
using VendingMachines.API.DTOs.Company;
using VendingMachines.API.DTOs.Contracts;
using VendingMachines.Infrastructure.Data;

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
        [SwaggerResponse(StatusCodes.Status200OK, "Список договоров получен", typeof(List<ContractResponse>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        public async Task<IActionResult> GetContractsAsync(
            [FromQuery][SwaggerParameter(Description = "ID компании для фильтрации (опционально)")] int? companyId = null,
            [FromQuery][SwaggerParameter(Description = "Количество договоров на странице (по умолчанию 10)")] int limit = 10,
            [FromQuery][SwaggerParameter(Description = "Смещение для пагинации (по умолчанию 0)")] int offset = 0)
        {
            try
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
                    .Select(contract => new ContractResponse
                    {
                        Company = new CompanyResponse
                        {
                            Id = contract.Id,
                            Name = contract.Company.Name,
                            ContactEmail = contract.Company.ContactEmail,
                            ContactPhone = contract.Company.ContactPhone,
                            Address = contract.Company.Address
                        },
                        ContractNumber = contract.ContractNumber,
                        SigningDate = contract.SigningDate,
                        EndDate = contract.EndDate,
                        ContactStatus = contract.Status,
                        SignatureData = contract.SignatureData
                    })
                    .ToListAsync();

                return Ok(contracts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse
                {
                    StatusCode = 500,
                    Message = ex.Message,
                });
            }
        }
    }
}
