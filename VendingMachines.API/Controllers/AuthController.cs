using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VendingMachines.API.DTOs;
using VendingMachines.API.DTOs.Company;
using VendingMachines.API.DTOs.Role;
using VendingMachines.API.DTOs.Account;
using VendingMachines.API.DTOs.Auth;
using VendingMachines.Core.Models;
using VendingMachines.Infrastructure.Data;
using VendingMachines.Infrastructure.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace VendingMachines.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [SwaggerTag("Контроллер авторизации и аутентификации")]
    public class AuthController : ControllerBase
    {
        private readonly VendingMachinesContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(VendingMachinesContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        [SwaggerOperation(
            Summary = "Регистрация нового пользователя",
            Description = "Создает нового пользователя с указанием ФИО, email, телефона, роли, компании и языка")]
        [SwaggerResponse(StatusCodes.Status200OK, "Пользователь успешно зарегистрирован", typeof(UserResponse))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Ошибка валидации данных")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера", typeof(ErrorResponse))]
        public async Task<IActionResult> RegisterAsync(
            [FromBody][SwaggerParameter(Description = "Данные для регистрации нового пользователя")] RegisterRequest request)
        {
            try
            {
                //if (!ModelState.IsValid)
                //{
                //    var errorMessage = ModelState.Values
                //        .SelectMany(v => v.Errors)
                //        .FirstOrDefault()?.ErrorMessage ?? "Ошибка валидации";

                //    return BadRequest(errorMessage);
                //}

                var role = await _context.Roles
                        .FirstOrDefaultAsync(r => r.Name == "Пользователь");

                if (role == null)
                {
                    return NotFound("Роль для регистрации пользователя не найдена");
                }

                int roleId = role.Id;

                var company = await _context.Companies
                        .OrderBy(c => EF.Functions.Random())
                        .FirstOrDefaultAsync();

                if (company == null)
                {
                    return NotFound("Компания для регистрации пользователя не найдена");
                }

                int companyId = company.Id;

                var user = new User
                {
                    Id = await _context.Users.MaxAsync(u => u.Id) + 1,
                    LastName = request.LastName,
                    FirstName = request.FirstName,
                    MiddleName = request.MiddleName,
                    Email = request.Email,
                    Phone = request.Phone,
                    HashedPassword = PasswordHasher.HashPassword(request.Password),
                    RoleId = roleId,
                    CompanyId = companyId,
                    Language = request.Language
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var registeredUser = await _context.Users
                    .Include(u => u.Role)
                    .Include(u => u.Company)
                    .FirstOrDefaultAsync(u => u.Email == user.Email && u.HashedPassword == user.HashedPassword);

                if (registeredUser == null)
                {
                    return NotFound($"Пользователь с регистрационными данными {registeredUser?.Email} не найден");
                }

                return Ok(new UserResponse
                {
                    LastName = user.LastName,
                    FirstName = user.FirstName,
                    MiddleName = user.MiddleName ?? "не задан",
                    Email = user.Email,
                    Password = user.HashedPassword,
                    Phone = user.Phone ?? "не задан",
                    Language = user.Language ?? "не задан",
                    Role = user.Role != null ? new RoleResponse
                    {
                        Name = user.Role.Name ?? "не задан",
                        Description = user.Role.Description ?? "не задан"
                    } : new RoleResponse(),
                    Company = user.Company != null ? new CompanyResponse
                    {
                        Id = user.Company.Id,
                        Name = user.Company.Name ?? "не задан",
                        ContactEmail = user.Company.ContactEmail ?? "не задан",
                        ContactPhone = user.Company.ContactPhone ?? "не задан",
                        Address = user.Company.Address ?? "не задан"
                    } : new CompanyResponse(),
                    Token = "При регистрации JWT-токен не выдается"
                });
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

        [HttpPost("login")]
        [SwaggerOperation(
            Summary = "Вход в систему",
            Description = "Аутентифицирует пользователя по email и паролю. Возвращает JWT-токен и данные пользователя. Токен сохраняется в cookie.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Авторизация успешна", typeof(UserResponse))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Неверные учетные данные")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера", typeof(ErrorResponse))]
        public async Task<IActionResult> LoginAsync(
            [FromBody][SwaggerParameter(Description = "Учетные данные для входа (email и пароль)")] LoginRequest loginRequest)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Role)
                    .Include(u => u.Company)
                    .FirstOrDefaultAsync(u => u.Email == loginRequest.Email);

                if (user == null || !PasswordHasher.VerifyPassword(loginRequest.Password, user.HashedPassword))
                {
                    return Unauthorized("Неверный email или пароль");
                }

                var token = JwtTokenService.GenerateJwtToken(user, _configuration);

                var userResponse = new UserResponse
                {
                    LastName = user.LastName,
                    FirstName = user.FirstName,
                    MiddleName = user.MiddleName ?? "не задан",
                    Email = user.Email,
                    Password = user.HashedPassword,
                    Phone = user.Phone ?? "не задан",
                    Language = user.Language ?? "не задан",
                    Role = user.Role != null ? new RoleResponse
                    {
                        Name = user.Role.Name ?? "не задан",
                        Description = user.Role.Description ?? "не задан"
                    } : new RoleResponse(),
                    Company = user?.Company != null ? new CompanyResponse
                    {
                        Id = user.Company.Id,
                        Name = user.Company.Name ?? "не задан",
                        ContactEmail = user.Company.ContactEmail ?? "не задан",
                        ContactPhone = user.Company.ContactPhone ?? "не задан",
                        Address = user.Company.Address ?? "не задан"
                    } : new CompanyResponse(),
                    Token = !string.IsNullOrEmpty(token)
                        ? "JWT-токен хранится в Cookies на сервере. Пользователю он недоступен"
                        : "JWT-токена на сервере нет"
                };

                Response.Cookies.Append("jwt_token", token);

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

        [Authorize]
        [HttpPost("refresh-token")]
        [SwaggerOperation(
            Summary = "Обновление JWT-токена",
            Description = "Генерирует новый JWT токен для текущего авторизованного пользователя")]
        [SwaggerResponse(StatusCodes.Status200OK, "Новый токен сгенерирован", typeof(string))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера", typeof(ErrorResponse))]
        public async Task<IActionResult> RefreshTokenAsync()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id.ToString() == userId);

                if (user == null)
                {
                    return NotFound("Пользователь не найден");
                }

                var token = JwtTokenService.GenerateJwtToken(user, _configuration);

                Response.Cookies.Append("jwt_token", token);

                return Ok("JWT-токен успешно обновлен");
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

        [Authorize]
        [HttpPost("logout")]
        [SwaggerOperation(
            Summary = "Выход из системы",
            Description = "Удаляет JWT токен из cookies клиента и завершает сессию")]
        [SwaggerResponse(StatusCodes.Status200OK, "Выход выполнен успешно")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера", typeof(ErrorResponse))]
        public IActionResult LogoutAsync()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = _context.Users.FirstOrDefault(u => u.Id.ToString() == userId);

                if (user == null)
                {
                    return NotFound("Пользователь не найден");
                }

                Response.Cookies.Delete("jwt_token");
                return Ok("Вы вышли из системы");
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
