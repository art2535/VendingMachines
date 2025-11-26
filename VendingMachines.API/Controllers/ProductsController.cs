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
    [SwaggerTag("Контроллер для работы с товарами/напитками")]
    public class ProductsController : ControllerBase
    {
        private readonly VendingMachinesContext _context;

        public ProductsController(VendingMachinesContext context)
        {
            _context = context;
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Получение списка товаров",
            Description = "Пагинируемый список всех товаров в системе.")]
        [ProducesResponseType(typeof(List<Product>), StatusCodes.Status200OK)]
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
