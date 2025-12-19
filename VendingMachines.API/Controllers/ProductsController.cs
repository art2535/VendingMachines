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
            Description = "Пагинируемый список всех товаров в системе с сортировкой по ID")]
        [SwaggerResponse(StatusCodes.Status200OK, "Список товаров получен", typeof(List<Product>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        public async Task<IActionResult> GetProductsAsync(
            [FromQuery][SwaggerParameter(Description = "Количество товаров на странице (по умолчанию 10)")] int limit = 10,
            [FromQuery][SwaggerParameter(Description = "Смещение для пагинации (по умолчанию 0)")] int offset = 0)
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
