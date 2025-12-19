using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VendingMachines.Core.Models;
using VendingMachines.Infrastructure.Data;

namespace VendingMachines.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [SwaggerTag("Контроллер для управления компаниями")]
    public class CompaniesController : ControllerBase
    {
        private readonly VendingMachinesContext _context;

        public CompaniesController(VendingMachinesContext context)
        {
            _context = context;
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Получение списка компаний",
            Description = "Возвращает компании с пагинацией и фильтром по части названия")]
        [SwaggerResponse(StatusCodes.Status200OK, "Список компаний получен", typeof(List<Company>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        public async Task<IActionResult> GetCompaniesAsync(
            [FromQuery][SwaggerParameter(Description = "Количество компаний на странице (по умолчанию 10)")] int limit = 10,
            [FromQuery][SwaggerParameter(Description = "Смещение для пагинации (по умолчанию 0)")] int offset = 0,
            [FromQuery][SwaggerParameter(Description = "Фильтр по части названия компании")] string nameFilter = "")
        {
            var query = _context.Companies.AsQueryable();

            if (!string.IsNullOrEmpty(nameFilter))
            {
                query = query.Where(company => company.Name.Contains(nameFilter));
            }

            var companies = await query.OrderBy(company => company.Id)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            return Ok(companies);
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Создание новой компании")]
        [SwaggerResponse(StatusCodes.Status201Created, "Компания успешно создана", typeof(Company))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Некорректные данные компании")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Ошибка сервера при создании")]
        public async Task<IActionResult> CreateCompanyAsync(
            [FromBody][SwaggerParameter(Description = "Данные новой компании")] Company company)
        {
            if (company == null)
            {
                return BadRequest();
            }

            company.CreatedAt = DateTime.UtcNow;
            company.UpdatedAt = DateTime.UtcNow;

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            var createdCompany = await _context.Companies
                .FirstOrDefaultAsync(c => c.Id == company.Id);

            if (createdCompany == null)
            {
                return StatusCode(500, "Ошибка при создании компании: компания не найдена после сохранения.");
            }

            return Created($"api/companies/{createdCompany.Id}", createdCompany);
        }

        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Обновление компании")]
        [SwaggerResponse(StatusCodes.Status200OK, "Компания успешно обновлена", typeof(Company))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "ID в пути не совпадает с ID в теле")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Компания не найдена")]
        public async Task<IActionResult> UpdateCompanyAsync(
            [FromRoute][SwaggerParameter(Description = "ID компании для обновления")] int id,
            [FromBody][SwaggerParameter(Description = "Обновленные данные компании")] Company company)
        {
            if (company.Id != id)
            {
                return BadRequest();
            }

            var existingCompany = await _context.Companies.FindAsync(id);
            if (existingCompany == null)
            {
                return NotFound();
            }

            existingCompany.UpdatedAt = DateTime.UtcNow;

            _context.Entry(existingCompany).CurrentValues.SetValues(company);
            await _context.SaveChangesAsync();

            return Ok(existingCompany);
        }

        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Удаление компании")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Компания успешно удалена")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Компания не найдена")]
        public async Task<IActionResult> DeleteCompanyAsync(
            [FromRoute][SwaggerParameter(Description = "ID компании для удаления")] int id)
        {
            var deletedCompany = await _context.Companies.FindAsync(id);

            if (deletedCompany == null)
            {
                return NotFound();
            }

            _context.Companies.Remove(deletedCompany);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
