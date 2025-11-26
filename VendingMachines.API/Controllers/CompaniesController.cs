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
            Description = "Поддерживает фильтр по части названия.")]
        [ProducesResponseType(typeof(List<Company>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCompaniesAsync([FromQuery] int limit = 10,
            [FromQuery] int offset = 0, [FromQuery] string nameFilter = "")
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
        [ProducesResponseType(typeof(Company), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCompanyAsync([FromBody] Company company)
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
        [ProducesResponseType(typeof(Company), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCompanyAsync([FromRoute] int id, [FromBody] Company company)
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
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCompanyAsync([FromRoute] int id)
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
