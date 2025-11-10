using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VendingMachines.API.DTOs.Account; 
using VendingMachines.API.DTOs.Auth;
using VendingMachines.Core.Models;
using VendingMachines.Infrastructure.Data;
using VendingMachines.Infrastructure.Services;

namespace VendingMachines.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        public async Task<IActionResult> GetUserAsync()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound("Пользователь не найден");

            var userResponse = new UserRequest
            {
                Email = user.Email,
                Password = user.HashedPassword,
                FirstName = user.FirstName,
                MiddleName = user.MiddleName,
                LastName = user.LastName,
                RoleName = user.Role?.Name
            };

            return Ok(userResponse);
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request)
        {
            if (request.Password != request.RepeatPassword)
            {
                return BadRequest("Пароли не совпадают");
            }

            var user = new User
            {
                Email = request.Email,
                HashedPassword = request.Password
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("login")]
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
        public IActionResult RefreshToken()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = _context.Users.FirstOrDefault(u => u.Id.ToString() == userId);

            var token = JwtTokenService.GenerateJwtToken(user, _configuration);

            return Ok(token);
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult LogoutAsync()
        {
            Response.Cookies.Delete("jwt_token");
            return Ok("Вы вышли из системы");
        }
    }
}
