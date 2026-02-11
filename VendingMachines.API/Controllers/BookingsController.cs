using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VendingMachines.API.DTOs;
using VendingMachines.API.DTOs.Bookings;
using VendingMachines.API.DTOs.Bookings.Enums;
using VendingMachines.API.DTOs.Company;
using VendingMachines.API.DTOs.Devices;
using VendingMachines.API.Extensions;
using VendingMachines.Core.Models;
using VendingMachines.Infrastructure.Data;

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
        [SwaggerResponse(StatusCodes.Status200OK, "Список бронирований получен", typeof(IEnumerable<BookingsResponse>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        public async Task<ActionResult<IEnumerable<BookingsResponse>>> GetBookingsAsync()
        {
            try
            {
                var bookings = await _context.Bookings
                    .Include(b => b.Device)
                    .Include(b => b.Company)
                    .Select(b => new BookingsResponse
                    {
                        Device = new DeviceResponse
                        {
                            Model = new ModelResponse
                            {
                                Name = b.Device.DeviceModel.Name,
                                DeviceType = new DeviceTypeResponse
                                {
                                    Name = b.Device.DeviceModel.DeviceType.Name,
                                    Description = b.Device.DeviceModel.DeviceType.Description
                                },
                                Description = b.Device.DeviceModel.Description
                            },
                            Location = new LocationResponse
                            {
                                InstallationAddress = b.Device.Location.InstallationAddress,
                                PlaceDescription = b.Device.Location.PlaceDescription
                            },
                            Modem = new ModemResponse
                            {
                                Brand = b.Device.Modem.Brand,
                                SerialNumber = b.Device.Modem.SerialNumber,
                                Provider = b.Device.Modem.Provider,
                                Balance = b.Device.Modem.Balance ?? 0
                            },
                            DeviceStatus = new DeviceStatusResponse
                            {
                                Name = b.Device.DeviceStatus.Name,
                                ColorCode = b.Device.DeviceStatus.ColorCode
                            },
                            InstallationDate = b.Device.InstallationDate,
                            LastServiceDate = b.Device.LastServiceDate
                        },
                        Company = new CompanyResponse
                        {
                            Name = b.Company.Name,
                            ContactEmail = b.Company.ContactEmail,
                            ContactPhone = b.Company.ContactPhone,
                            Address = b.Company.Address
                        },
                        StartDate = b.StartDate,
                        EndDate = b.EndDate,
                        OwnershipType = b.OwnershipType,
                        Insurance = b.Insurance,
                        MonthlyCost = b.MonthlyCost,
                        AnnualCost = b.AnnualCost,
                        PaybackPeriod = b.PaybackPeriod,
                        Status = b.Status ?? "не задан"
                    })
                    .ToListAsync();

                return Ok(bookings);
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
        [SwaggerOperation(
            Summary = "Создание бронирования аппарата",
            Description = "Создает новое бронирование. Нельзя забронировать уже забронированный аппарат со статусом 'подтверждено'")]
        [SwaggerResponse(StatusCodes.Status201Created, "Бронирование успешно создано", typeof(BookingsResponse))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Аппарат уже забронирован или ошибка в данных")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        public async Task<IActionResult> CreateBookingAsync(
            [FromBody][SwaggerParameter(Description = "Данные для создания бронирования аппарата")] BookingsRequest request)
        {
            try
            {
                var existingBooking = await _context.Bookings
                    .AnyAsync(b =>
                        b.DeviceId == request.DeviceId &&
                        b.Status == BookingStatusEnum.Confirmed.ToRussianDb().ToLower());

                if (existingBooking)
                {
                    return BadRequest("Устройство уже забронировано");
                }

                var booking = new Booking
                {
                    Id = request.Id,
                    DeviceId = request.DeviceId,
                    CompanyId = request.CompanyId,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    OwnershipType = request.OwnershipType.ToRussianDb(),
                    Insurance = request.Insurance,
                    MonthlyCost = request.MonthlyCost,
                    AnnualCost = request.AnnualCost,
                    PaybackPeriod = request.PaybackPeriod,
                    Status = request.Status.ToRussianDb()
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                return Created($"api/bookings/{booking.Id}", booking);
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
