using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendingMachines.Core.Models;
using VendingMachines.Infrastructure.Data;

namespace VendingMachines.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly VendingMachinesContext _context;

        public BookingsController(VendingMachinesContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBookingAsync([FromBody] Booking booking)
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
