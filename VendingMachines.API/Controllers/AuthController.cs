using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
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

        [Authorize]
        [HttpGet("info")]
        [SwaggerOperation(
            Summary = "Информация о текущем пользователе",
            Description = "Возвращает данные текущего авторизованного пользователя, роль и JWT токен из cookies")]
        [SwaggerResponse(StatusCodes.Status200OK, "Информация о пользователе получена", typeof(UserRequest))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Пользователь не найден")]
        public async Task<IActionResult> GetUserAsync()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound("Пользователь не найден");

            var token = Request.Cookies["jwt_token"];
            var userResponse = new UserRequest
            {
                Email = user.Email,
                Password = user.HashedPassword,
                FirstName = user.FirstName,
                MiddleName = user.MiddleName,
                LastName = user.LastName,
                RoleName = user.Role?.Name,
                Token = token
            };

            return Ok(userResponse);
        }

        [HttpPost("register")]
        [SwaggerOperation(
            Summary = "Регистрация нового пользователя",
            Description = "Создает нового пользователя с указанием ФИО, email, телефона, роли, компании и языка")]
        [SwaggerResponse(StatusCodes.Status200OK, "Пользователь успешно зарегистрирован", typeof(User))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Ошибка валидации данных")]
        public async Task<IActionResult> RegisterAsync(
            [FromBody][SwaggerParameter(Description = "Данные для регистрации нового пользователя")] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errorMessage = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .FirstOrDefault()?.ErrorMessage ?? "Ошибка валидации";

                return BadRequest(errorMessage);
            }

            var user = new User
            {
                LastName = request.LastName,
                FirstName = request.FirstName,
                MiddleName = request.MiddleName,
                Email = request.Email,
                Phone = request.Phone,
                HashedPassword = request.Password,
                RoleId = request.RoleId,
                CompanyId = request.CompanyId,
                Language = request.Language
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(user);
        }

        [HttpPost("login")]
        [SwaggerOperation(
            Summary = "Вход в систему",
            Description = "Аутентифицирует пользователя по email и паролю. Возвращает JWT-токен и данные пользователя. Токен сохраняется в cookie.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Авторизация успешна", typeof(UserRequest))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Неверные учетные данные")]
        public async Task<IActionResult> LoginAsync(
            [FromBody][SwaggerParameter(Description = "Учетные данные для входа (email и пароль)")] LoginRequest loginRequest)
        {
            var user = await _context.Users
                .Include(user => user.Role)
                .FirstOrDefaultAsync(u => u.Email == loginRequest.Email && u.HashedPassword == loginRequest.Password);

            if (user == null)
            {
                return Unauthorized("Пользователь не авторизован");
            }

            var token = JwtTokenService.GenerateJwtToken(user, _configuration);

            var userResponse = new UserRequest
            {
                Email = user.Email,
                Password = user.HashedPassword,
                FirstName = user.FirstName,
                MiddleName = user.MiddleName,
                LastName = user.LastName,
                RoleName = user.Role.Name,
                Token = token
            };

            Response.Cookies.Append("jwt_token", token);

            return Ok(userResponse);
        }

        [Authorize]
        [HttpPost("refresh-token")]
        [SwaggerOperation(
            Summary = "Обновление JWT-токена",
            Description = "Генерирует новый JWT токен для текущего авторизованного пользователя")]
        [SwaggerResponse(StatusCodes.Status200OK, "Новый токен сгенерирован", typeof(string))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        public IActionResult RefreshToken()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = _context.Users.FirstOrDefault(u => u.Id.ToString() == userId);

            var token = JwtTokenService.GenerateJwtToken(user, _configuration);

            return Ok(token);
        }

        [Authorize]
        [HttpPost("logout")]
        [SwaggerOperation(
            Summary = "Выход из системы",
            Description = "Удаляет JWT токен из cookies клиента и завершает сессию")]
        [SwaggerResponse(StatusCodes.Status200OK, "Выход выполнен успешно")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        public IActionResult LogoutAsync()
        {
            Response.Cookies.Delete("jwt_token");
            return Ok("Вы вышли из системы");
        }
    }
}
