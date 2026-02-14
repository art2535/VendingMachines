using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VendingMachines.API.DTOs;
using VendingMachines.API.DTOs.Products;
using VendingMachines.Infrastructure.Data;

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
        [SwaggerResponse(StatusCodes.Status200OK, "Список товаров получен", typeof(List<ProductResponse>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        public async Task<IActionResult> GetProductsAsync()
        {
            try
            {
                var products = await _context.Products
                    .Include(p => p.Inventories)
                    .Include(p => p.Sales)
                    .OrderBy(p => p.Id)
                    .Select(p => new ProductResponse
                    {
                        Name = p.Name,
                        Description = p.Description ?? "нет",
                        Price = p.Price,
                        SalesPopularity = p.SalesPopularity ?? 0,
                        CreatedAt = p.CreatedAt ?? DateTime.UtcNow,
                    })
                    .ToListAsync();

                return Ok(products);
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
