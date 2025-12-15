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
        [SwaggerOperation(Summary = "Информация о текущем пользователе")]
        [ProducesResponseType(typeof(UserRequest), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
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
        [SwaggerOperation(Summary = "Регистрация нового пользователя")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request)
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
            Description = "Возвращает JWT-токен и данные пользователя. Токен также сохраняется в cookie.")]
        [ProducesResponseType(typeof(UserRequest), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest loginRequest)
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
        [SwaggerOperation(Summary = "Обновление JWT-токена")]
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
            Description = "Удаляет JWT из cookies")]
        public IActionResult LogoutAsync()
        {
            Response.Cookies.Delete("jwt_token");
            return Ok("Вы вышли из системы");
        }
    }
}
