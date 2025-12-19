using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendingMachines.Core.Models;
using VendingMachines.Infrastructure.Data;
using Swashbuckle.AspNetCore.Annotations;

namespace VendingMachines.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [SwaggerTag("Контроллер бронирования аппаратов")]
    public class BookingsController : ControllerBase
    {
        private readonly VendingMachinesContext _context;

        public BookingsController(VendingMachinesContext context)
        {
            _context = context;
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Список бронирований аппаратов",
            Description = "Возвращает все бронирования с информацией об аппаратах и компаниях")]
        [SwaggerResponse(StatusCodes.Status200OK, "Список бронирований получен", typeof(IEnumerable<Booking>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        public async Task<ActionResult<IEnumerable<Booking>>> GetBookingsAsync()
        {
            return await _context.Bookings
                .Include(b => b.Device)
                .Include(b => b.Company)
                .ToListAsync();
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Создание бронирования аппарата",
            Description = "Создает новое бронирование. Нельзя забронировать уже забронированный аппарат со статусом 'confirmed'")]
        [SwaggerResponse(StatusCodes.Status201Created, "Бронирование успешно создано", typeof(Booking))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Аппарат уже забронирован или ошибка в данных")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        public async Task<IActionResult> CreateBookingAsync(
            [FromBody][SwaggerParameter(Description = "Данные для создания бронирования аппарата")] Booking booking)
        {
            var existingBooking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.DeviceId == booking.DeviceId && b.Status == "confirmed");

            if (existingBooking != null)
            {
                return BadRequest("Устройство уже забронировано");
            }

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return Created($"api/bookings/{booking.Id}", booking);
        }
    }
}
