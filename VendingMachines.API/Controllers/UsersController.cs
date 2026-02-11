using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using VendingMachines.API.DTOs;
using VendingMachines.API.DTOs.Account;
using VendingMachines.API.DTOs.Company;
using VendingMachines.API.DTOs.Role;
using VendingMachines.Infrastructure.Data;

namespace VendingMachines.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Consumes("application/json")]
    [SwaggerTag("Контроллер личного кабинета пользователя")]
    public class UsersController : ControllerBase
    {
        private readonly VendingMachinesContext _context;

        public UsersController(VendingMachinesContext context)
        {
            _context = context;
        }

        [HttpGet("info")]
        [SwaggerOperation(
            Summary = "Информация о текущем пользователе",
            Description = "Возвращает данные текущего авторизованного пользователя, роль и JWT токен из cookies")]
        [SwaggerResponse(StatusCodes.Status200OK, "Информация о пользователе получена", typeof(UserResponse))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Пользователь не найден")]
        public async Task<IActionResult> GetUserAsync()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                var user = await _context.Users
                    .Include(u => u.Role)
                    .Include(u => u.Company)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                    return NotFound("Пользователь не найден");

                var token = Request.Cookies[$"jwt_token_user{userId}"];

                var userResponse = new UserResponse
                {
                    LastName = user.LastName,
                    FirstName = user.FirstName,
                    MiddleName = user.MiddleName ?? "не задан",
                    Email = user.Email,
                    Password = user.HashedPassword,
                    Phone = user.Phone ?? "не задан",
                    Language = user.Language ?? "не задан",
                    Role = new RoleResponse
                    {
                        Name = user?.Role != null ? user.Role.Name : "не задан",
                        Description = user?.Role?.Description ?? "не задан"
                    },
                    Company = new CompanyResponse
                    {
                        Name = user?.Company != null ? user.Company.Name : "не задан",
                        ContactEmail = user?.Company != null ? user?.Company.ContactEmail : "не задан",
                        ContactPhone = user?.Company != null ? user?.Company.ContactPhone : "не задан",
                        Address = user?.Company != null ? user.Company.Address : "не задан"
                    },
                    Token = !string.IsNullOrEmpty(token)
                        ? "JWT-токен хранится в Cookies на сервере. Пользователю он недоступен"
                        : "JWT-токена на сервере нет"
                };

                return Ok(userResponse);
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
