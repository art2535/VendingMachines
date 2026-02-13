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
                        Id = b.Id,
                        Device = b.Device != null ? new DeviceResponse
                        {
                            Id = b.Device.Id,
                            Model = b.Device.DeviceModel != null ? new ModelResponse
                            {
                                Id = b.Id,
                                Name = b.Device.DeviceModel.Name ?? "не задан",
                                DeviceType = b.Device.DeviceModel.DeviceType != null ? new DeviceTypeResponse
                                {
                                    Id = b.Device.DeviceModel.DeviceType.Id,
                                    Name = b.Device.DeviceModel.DeviceType.Name,
                                    Description = b.Device.DeviceModel.DeviceType.Description ?? "не задан"
                                } : new DeviceTypeResponse(),
                                Description = b.Device.DeviceModel.Description ?? "не задан"
                            } : new ModelResponse(),
                            Location = b.Device.Location != null ? new LocationResponse
                            {
                                Id = b.Device.Location.Id,
                                InstallationAddress = b.Device.Location.InstallationAddress ?? "не задан",
                                PlaceDescription = b.Device.Location.PlaceDescription
                            } : new LocationResponse(),
                            Modem = b.Device.Modem != null ? new ModemResponse
                            {
                                Id = b.Device.Modem.Id,
                                Brand = b.Device.Modem.Brand ?? "не задан",
                                SerialNumber = b.Device.Modem.SerialNumber,
                                Provider = b.Device.Modem.Provider ?? "не задан",
                                Balance = b.Device.Modem.Balance ?? 0
                            } : new ModemResponse(),
                            DeviceStatus = b.Device.DeviceStatus != null ? new DeviceStatusResponse
                            {
                                Id = b.Device.DeviceStatus.Id,
                                Name = b.Device.DeviceStatus.Name ?? "не задан",
                                ColorCode = b.Device.DeviceStatus.ColorCode ?? "не задан"
                            } : new DeviceStatusResponse(),
                            Company = b.Device.Company != null ? new CompanyResponse
                            {
                                Id = b.Device.Company.Id,
                                Name = b.Device.Company.Name ?? "не задан",
                                ContactEmail = b.Device.Company.ContactEmail ?? "не задан",
                                ContactPhone = b.Device.Company.ContactPhone ?? "не задан",
                                Address = b.Device.Company.Address ?? "не задан"
                            } : new CompanyResponse(),
                            InstallationDate = b.Device.InstallationDate,
                            LastServiceDate = b.Device.LastServiceDate
                        } : new DeviceResponse(),
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
                    Id = await _context.Bookings.MaxAsync(b => b.Id) + 1,
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

                var createdBooking = await _context.Bookings
                    .Include(b => b.Device)
                    .Include(b => b.Company)
                    .Select(b => new BookingsResponse
                    {
                        Id = b.Id,
                        Device = b.Device != null ? new DeviceResponse
                        {
                            Id = b.Device.Id,
                            Model = b.Device.DeviceModel != null ? new ModelResponse
                            {
                                Id = b.Id,
                                Name = b.Device.DeviceModel.Name ?? "не задан",
                                DeviceType = b.Device.DeviceModel.DeviceType != null ? new DeviceTypeResponse
                                {
                                    Id = b.Device.DeviceModel.DeviceType.Id,
                                    Name = b.Device.DeviceModel.DeviceType.Name,
                                    Description = b.Device.DeviceModel.DeviceType.Description ?? "не задан"
                                } : new DeviceTypeResponse(),
                                Description = b.Device.DeviceModel.Description ?? "не задан"
                            } : new ModelResponse(),
                            Location = b.Device.Location != null ? new LocationResponse
                            {
                                Id = b.Device.Location.Id,
                                InstallationAddress = b.Device.Location.InstallationAddress ?? "не задан",
                                PlaceDescription = b.Device.Location.PlaceDescription
                            } : new LocationResponse(),
                            Modem = b.Device.Modem != null ? new ModemResponse
                            {
                                Id = b.Device.Modem.Id,
                                Brand = b.Device.Modem.Brand ?? "не задан",
                                SerialNumber = b.Device.Modem.SerialNumber,
                                Provider = b.Device.Modem.Provider ?? "не задан",
                                Balance = b.Device.Modem.Balance ?? 0
                            } : new ModemResponse(),
                            DeviceStatus = b.Device.DeviceStatus != null ? new DeviceStatusResponse
                            {
                                Id = b.Device.DeviceStatus.Id,
                                Name = b.Device.DeviceStatus.Name ?? "не задан",
                                ColorCode = b.Device.DeviceStatus.ColorCode ?? "не задан"
                            } : new DeviceStatusResponse(),
                            Company = b.Device.Company != null ? new CompanyResponse
                            {
                                Id = b.Device.Company.Id,
                                Name = b.Device.Company.Name ?? "не задан",
                                ContactEmail = b.Device.Company.ContactEmail ?? "не задан",
                                ContactPhone = b.Device.Company.ContactPhone ?? "не задан",
                                Address = b.Device.Company.Address ?? "не задан"
                            } : new CompanyResponse(),
                            InstallationDate = b.Device.InstallationDate,
                            LastServiceDate = b.Device.LastServiceDate
                        } : new DeviceResponse(),
                        StartDate = b.StartDate,
                        EndDate = b.EndDate,
                        OwnershipType = b.OwnershipType,
                        Insurance = b.Insurance,
                        MonthlyCost = b.MonthlyCost,
                        AnnualCost = b.AnnualCost,
                        PaybackPeriod = b.PaybackPeriod,
                        Status = b.Status ?? "не задан"
                    })
                    .FirstOrDefaultAsync();

                return Created($"api/bookings/{booking.Id}", createdBooking);
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
