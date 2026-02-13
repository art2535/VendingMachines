using DocumentFormat.OpenXml.ExtendedProperties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VendingMachines.API.DTOs;
using VendingMachines.API.DTOs.Company;
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
        [SwaggerResponse(StatusCodes.Status200OK, "Список компаний получен", typeof(List<CompanyResponse>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        public async Task<IActionResult> GetCompaniesAsync(
            [FromQuery][SwaggerParameter(Description = "Фильтр по части названия компании")] string nameFilter = "")
        {
            try
            {
                var query = _context.Companies.AsQueryable();

                if (!string.IsNullOrEmpty(nameFilter))
                {
                    query = query.Where(company => company.Name.Contains(nameFilter));
                }

                var companies = await query.OrderBy(company => company.Id)
                    .Select(company => new CompanyResponse
                    {
                        Id = company.Id,
                        Name = company.Name,
                        ContactEmail = company.ContactEmail ?? "не задан",
                        ContactPhone = company.ContactPhone ?? "не задан",
                        Address = company.Address ?? "не задан"
                    })
                    .ToListAsync();

                return Ok(companies);
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

        [HttpPost]
        [SwaggerOperation(Summary = "Создание новой компании")]
        [SwaggerResponse(StatusCodes.Status201Created, "Компания успешно создана", typeof(CompanyResponse))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Некорректные данные компании")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Ошибка сервера при создании")]
        public async Task<IActionResult> CreateCompanyAsync(
            [FromBody][SwaggerParameter(Description = "Данные новой компании")] CompanyRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Пустое тело JSON!");
                }

                var company = new Company
                {
                    Id = await _context.Companies.MaxAsync(c => c.Id) + 1,
                    Name = request.Name,
                    ContactEmail = request.ContactEmail,
                    ContactPhone = request.ContactPhone,
                    Address = request.Address,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Companies.Add(company);
                await _context.SaveChangesAsync();

                var createdCompany = new CompanyResponse
                {
                    Id = company.Id,
                    Name = company.Name,
                    ContactEmail = company.ContactEmail ?? "не задан",
                    ContactPhone = company.ContactPhone ?? "не задан",
                    Address = company.Address ?? "не задан"
                };

                return Created($"api/companies/{company.Id}", createdCompany);
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

        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Обновление компании")]
        [SwaggerResponse(StatusCodes.Status200OK, "Компания успешно обновлена", typeof(CompanyResponse))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "ID в пути не совпадает с ID в теле")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Компания не найдена")]
        public async Task<IActionResult> UpdateCompanyAsync(
            [FromRoute][SwaggerParameter(Description = "ID компании для обновления")] int id,
            [FromBody][SwaggerParameter(Description = "Обновленные данные компании")] CompanyRequest request)
        {
            try
            {
                var existingCompany = await _context.Companies.FindAsync(id);
                if (existingCompany == null)
                {
                    return NotFound("Комания не найдена");
                }

                existingCompany.UpdatedAt = DateTime.UtcNow;

                _context.Entry(existingCompany).CurrentValues.SetValues(request);
                await _context.SaveChangesAsync();

                var updatedCompany = new CompanyResponse
                {
                    Id = existingCompany.Id,
                    Name = existingCompany.Name,
                    ContactEmail = existingCompany.ContactEmail ?? "не задан",
                    ContactPhone = existingCompany.ContactPhone ?? "не задан",
                    Address = existingCompany.Address ?? "не задан"
                };

                return Ok(updatedCompany);
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

        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Удаление компании")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Компания успешно удалена")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Компания не найдена")]
        public async Task<IActionResult> DeleteCompanyAsync(
            [FromRoute][SwaggerParameter(Description = "ID компании для удаления")] int id)
        {
            try
            {
                var deletedCompany = await _context.Companies.FindAsync(id);

                if (deletedCompany == null)
                {
                    return NotFound("Комания не найдена");
                }

                _context.Companies.Remove(deletedCompany);
                await _context.SaveChangesAsync();

                return NoContent();
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
