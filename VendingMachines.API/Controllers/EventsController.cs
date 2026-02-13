using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VendingMachines.API.DTOs;
using VendingMachines.API.DTOs.Company;
using VendingMachines.API.DTOs.Devices;
using VendingMachines.API.DTOs.Events;
using VendingMachines.API.DTOs.Events.Enums;
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
    [SwaggerTag("Контроллер для работы с событиями и заметками по аппаратам")]
    public class EventsController : ControllerBase
    {
        private VendingMachinesContext _context;

        public EventsController(VendingMachinesContext context)
        {
            _context = context;
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Получение архива событий с фильтрацией и сортировкой",
            Description = "Поддерживает поиск по тексту, фильтр по типу события, дате и сортировку")]
        [SwaggerResponse(StatusCodes.Status200OK, "Список событий получен", typeof(List<NotesResponse>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        public async Task<IActionResult> GetAllNotesAsync(
            [FromQuery][SwaggerParameter(Description = "Поиск по типу события, описанию, адресу, модели или компании")] string? search = null,
            [FromQuery][SwaggerParameter(Description = "Фильтр по типу события")] EventTypeEnum eventType = EventTypeEnum.Enabling,
            [FromQuery][SwaggerParameter(Description = "Фильтр по дате события (вся дата)")] DateTime? date = null,
            [FromQuery][SwaggerParameter(Description = "Поле сортировки: 'date' или 'type' (по умолчанию 'date')")] string? sortBy = "date",
            [FromQuery][SwaggerParameter(Description = "Направление сортировки: 'asc' или 'desc' (по умолчанию 'desc')")] string? sortOrder = "desc")
        {
            try
            {
                var query = _context.Events
                    .Include(e => e.Device)
                        .ThenInclude(d => d.DeviceModel)
                    .Include(e => e.Device)
                        .ThenInclude(d => d.Modem)
                    .Include(e => e.Device)
                        .ThenInclude(d => d.Location)
                    .Include(e => e.Device)
                        .ThenInclude(d => d.DeviceStatus)
                    .Include(e => e.Device)
                        .ThenInclude(d => d.Company)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var s = search.Trim().ToLowerInvariant();
                    query = query.Where(e =>
                        e.EventType.ToLower().Contains(s) ||
                        e.Description.ToLower().Contains(s) ||
                        e.Device.Location.InstallationAddress.ToLower().Contains(s) ||
                        e.Device.DeviceModel.Name.ToLower().Contains(s) ||
                        e.Device.Company.Name.ToLower().Contains(s)
                    );
                }

                if (!string.IsNullOrWhiteSpace(eventType.ToRussianDb()))
                {
                    query = query.Where(e => e.EventType == eventType.ToRussianDb().Trim());
                }

                if (date.HasValue)
                {
                    var startOfDay = DateTime.SpecifyKind(date.Value.Date, DateTimeKind.Utc);
                    var endOfDay = startOfDay.AddDays(1).AddTicks(-1);
                    query = query.Where(e => e.DateTime >= startOfDay && e.DateTime <= endOfDay);
                }

                query = sortBy?.ToLower() switch
                {
                    "type" => sortOrder?.ToLower() == "asc"
                        ? query.OrderBy(e => e.EventType).ThenByDescending(e => e.DateTime)
                        : query.OrderByDescending(e => e.EventType).ThenByDescending(e => e.DateTime),
                    _ => sortOrder?.ToLower() == "asc"
                        ? query.OrderBy(e => e.DateTime)
                        : query.OrderByDescending(e => e.DateTime)
                };

                var result = query.Select(e => new NotesResponse
                {
                    Id = e.Id,
                    EventType = e.EventType ?? "не задан",
                    Description = e.Description ?? "не задан",
                    EventDate = e.DateTime,
                    Device = e.Device != null ? new DeviceResponse
                    {
                        Id = e.Id,
                        Model = e.Device.DeviceModel != null ? new ModelResponse
                        {
                            Id = e.Device.DeviceModel.Id,
                            Name = e.Device.DeviceModel.Name,
                            Description = e.Device.DeviceModel.Description ?? "не задан",
                            DeviceType = e.Device.DeviceModel.DeviceType != null ? new DeviceTypeResponse
                            {
                                Id = e.Device.DeviceModel.DeviceType.Id,
                                Name = e.Device.DeviceModel.DeviceType.Name,
                                Description = e.Device.DeviceModel.DeviceType.Description ?? "не задан"
                            } : new DeviceTypeResponse()
                        } : new ModelResponse(),
                        Location = e.Device.Location != null ? new LocationResponse
                        {
                            Id = e.Device.Location.Id,
                            InstallationAddress = e.Device.Location.InstallationAddress,
                            PlaceDescription = e.Device.Location.PlaceDescription
                        } : new LocationResponse(),
                        Modem = e.Device.Modem != null ? new ModemResponse
                        {
                            Id = e.Device.Modem.Id,
                            Brand = e.Device.Modem.Brand ?? "не задан",
                            SerialNumber = e.Device.Modem.SerialNumber ?? "не задан",
                            Provider = e.Device.Modem.Provider ?? "не задан",
                            Balance = e.Device.Modem.Balance
                        } : new ModemResponse(),
                        DeviceStatus = e.Device.DeviceStatus != null ? new DeviceStatusResponse
                        {
                            Id = e.Device.DeviceStatus.Id,
                            Name = e.Device.DeviceStatus.Name,
                            ColorCode = e.Device.DeviceStatus.ColorCode ?? "не задан"
                        } : new DeviceStatusResponse(),
                        Company = e.Device.Company != null ? new CompanyResponse
                        {
                            Id = e.Device.Company.Id,
                            Name = e.Device.Company.Name,
                            ContactEmail = e.Device.Company.ContactEmail ?? "не задан",
                            ContactPhone = e.Device.Company.ContactPhone ?? "не задан",
                            Address = e.Device.Company.Address ?? "не задан"
                        } : new CompanyResponse(),
                        InstallationDate = e.Device.InstallationDate,
                        LastServiceDate = e.Device.LastServiceDate
                    } : new DeviceResponse()
                });

                return Ok(await result.ToListAsync());
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

        [HttpGet("{id:int}")]
        [SwaggerOperation(Summary = "Получение события по ID")]
        [SwaggerResponse(StatusCodes.Status200OK, "Событие найдено", typeof(NotesResponse))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Событие не найдено")]
        public async Task<IActionResult> GetNoteByIdAsync(
            [FromRoute][SwaggerParameter(Description = "ID события")] int id)
        {
            try
            {
                var eventEntity = await _context.Events
                    .Include(e => e.Device)
                        .ThenInclude(d => d.DeviceModel)
                    .Include(e => e.Device)
                        .ThenInclude(d => d.Modem)
                    .Include(e => e.Device)
                        .ThenInclude(d => d.Location)
                    .Include(e => e.Device)
                        .ThenInclude(d => d.DeviceStatus)
                    .Include(e => e.Device)
                        .ThenInclude(d => d.Company)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (eventEntity == null)
                {
                    return NotFound($"Событие с ID = {id} не найдено");
                }

                var result = new NotesResponse
                {
                    Id = eventEntity.Id,
                    EventType = eventEntity.EventType ?? "не задан",
                    Description = eventEntity.Description ?? "не задан",
                    EventDate = eventEntity.DateTime,
                    Device = eventEntity.Device != null ? new DeviceResponse
                    {
                        Id = eventEntity.Id,
                        Model = eventEntity.Device.DeviceModel != null ? new ModelResponse
                        {
                            Id = eventEntity.Device.DeviceModel.Id,
                            Name = eventEntity.Device.DeviceModel.Name,
                            Description = eventEntity.Device.DeviceModel.Description ?? "не задан",
                            DeviceType = eventEntity.Device.DeviceModel.DeviceType != null ? new DeviceTypeResponse
                            {
                                Id = eventEntity.Device.DeviceModel.DeviceType.Id,
                                Name = eventEntity.Device.DeviceModel.DeviceType.Name,
                                Description = eventEntity.Device.DeviceModel.DeviceType.Description ?? "не задан"
                            } : new DeviceTypeResponse()
                        } : new ModelResponse(),
                        Location = eventEntity.Device.Location != null ? new LocationResponse
                        {
                            Id = eventEntity.Device.Location.Id,
                            InstallationAddress = eventEntity.Device.Location.InstallationAddress,
                            PlaceDescription = eventEntity.Device.Location.PlaceDescription
                        } : new LocationResponse(),
                        Modem = eventEntity.Device.Modem != null ? new ModemResponse
                        {
                            Id = eventEntity.Device.Modem.Id,
                            Brand = eventEntity.Device.Modem.Brand ?? "не задан",
                            SerialNumber = eventEntity.Device.Modem.SerialNumber ?? "не задан",
                            Provider = eventEntity.Device.Modem.Provider ?? "не задан",
                            Balance = eventEntity.Device.Modem.Balance
                        } : new ModemResponse(),
                        DeviceStatus = eventEntity.Device.DeviceStatus != null ? new DeviceStatusResponse
                        {
                            Id = eventEntity.Device.DeviceStatus.Id,
                            Name = eventEntity.Device.DeviceStatus.Name,
                            ColorCode = eventEntity.Device.DeviceStatus.ColorCode ?? "не задан"
                        } : new DeviceStatusResponse(),
                        Company = eventEntity.Device.Company != null ? new CompanyResponse
                        {
                            Id = eventEntity.Device.Company.Id,
                            Name = eventEntity.Device.Company.Name,
                            ContactEmail = eventEntity.Device.Company.ContactEmail ?? "не задан",
                            ContactPhone = eventEntity.Device.Company.ContactPhone ?? "не задан",
                            Address = eventEntity.Device.Company.Address ?? "не задан"
                        } : new CompanyResponse(),
                        InstallationDate = eventEntity.Device.InstallationDate,
                        LastServiceDate = eventEntity.Device.LastServiceDate
                    } : new DeviceResponse()
                };

                return Ok(result);
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
        [SwaggerOperation(Summary = "Создание новой заметки/события")]
        [SwaggerResponse(StatusCodes.Status201Created, "Событие создано", typeof(NotesResponse))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Некорректные данные или аппарат не найден")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        public async Task<IActionResult> CreateNoteAsync(
            [FromBody][SwaggerParameter(Description = "Данные нового события/заметки")] NotesRequest request)
        {
            try
            {
                var newEvent = new Event
                {
                    Id = await _context.Events.MaxAsync(e => e.Id) + 1,
                    DeviceId = request.DeviceId,
                    EventType = request.EventType.ToRussianDb(),
                    Description = request.Description,
                    DateTime = request.EventDate ?? DateTime.UtcNow
                };

                _context.Events.Add(newEvent);
                await _context.SaveChangesAsync();

                var createdEvent = await _context.Events
                    .Include(e => e.Device)
                        .ThenInclude(d => d.DeviceModel)
                    .Include(e => e.Device)
                        .ThenInclude(d => d.Modem)
                    .Include(e => e.Device)
                        .ThenInclude(d => d.Location)
                    .Include(e => e.Device)
                        .ThenInclude(d => d.DeviceStatus)
                    .Include(e => e.Device)
                        .ThenInclude(d => d.Company)
                    .Select(e => new NotesResponse
                    {
                        Id = e.Id,
                        EventType = e.EventType ?? "не задан",
                        Description = e.Description ?? "не задан",
                        EventDate = e.DateTime,
                        Device = e.Device != null ? new DeviceResponse
                        {
                            Id = e.Id,
                            Model = e.Device.DeviceModel != null ? new ModelResponse
                            {
                                Id = e.Device.DeviceModel.Id,
                                Name = e.Device.DeviceModel.Name,
                                Description = e.Device.DeviceModel.Description ?? "не задан",
                                DeviceType = e.Device.DeviceModel.DeviceType != null ? new DeviceTypeResponse
                                {
                                    Id = e.Device.DeviceModel.DeviceType.Id,
                                    Name = e.Device.DeviceModel.DeviceType.Name,
                                    Description = e.Device.DeviceModel.DeviceType.Description ?? "не задан"
                                } : new DeviceTypeResponse()
                            } : new ModelResponse(),
                            Location = e.Device.Location != null ? new LocationResponse
                            {
                                Id = e.Device.Location.Id,
                                InstallationAddress = e.Device.Location.InstallationAddress,
                                PlaceDescription = e.Device.Location.PlaceDescription
                            } : new LocationResponse(),
                            Modem = e.Device.Modem != null ? new ModemResponse
                            {
                                Id = e.Device.Modem.Id,
                                Brand = e.Device.Modem.Brand ?? "не задан",
                                SerialNumber = e.Device.Modem.SerialNumber ?? "не задан",
                                Provider = e.Device.Modem.Provider ?? "не задан",
                                Balance = e.Device.Modem.Balance
                            } : new ModemResponse(),
                            DeviceStatus = e.Device.DeviceStatus != null ? new DeviceStatusResponse
                            {
                                Id = e.Device.DeviceStatus.Id,
                                Name = e.Device.DeviceStatus.Name,
                                ColorCode = e.Device.DeviceStatus.ColorCode ?? "не задан"
                            } : new DeviceStatusResponse(),
                            Company = e.Device.Company != null ? new CompanyResponse
                            {
                                Id = e.Device.Company.Id,
                                Name = e.Device.Company.Name,
                                ContactEmail = e.Device.Company.ContactEmail ?? "не задан",
                                ContactPhone = e.Device.Company.ContactPhone ?? "не задан",
                                Address = e.Device.Company.Address ?? "не задан"
                            } : new CompanyResponse(),
                            InstallationDate = e.Device.InstallationDate,
                            LastServiceDate = e.Device.LastServiceDate
                        } : new DeviceResponse()
                    })
                    .FirstOrDefaultAsync(e => e.Id == newEvent.Id);

                return Created($"api/events/{newEvent.Id}", createdEvent);
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

        [HttpPut("{id:int}")]
        [SwaggerOperation(Summary = "Обновление события")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Событие обновлено")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Некорректные данные или аппарат не найден")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Событие не найдено")]
        public async Task<IActionResult> UpdateNoteAsync(
            [FromRoute][SwaggerParameter(Description = "ID события для обновления")] int id,
            [FromBody][SwaggerParameter(Description = "Обновленные данные события")] NotesRequest request)
        {
            try
            {
                var existingEvent = await _context.Events.FindAsync(id);
                if (existingEvent == null)
                {
                    return NotFound($"Событие с ID = {id} не найдено");
                }

                existingEvent.EventType = request.EventType.ToRussianDb() ?? existingEvent.EventType;
                existingEvent.Description = request.Description ?? existingEvent.Description;
                existingEvent.DateTime = request.EventDate;

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

        [HttpDelete("{id:int}")]
        [SwaggerOperation(Summary = "Удаление события")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Событие удалено")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Событие не найдено")]
        public async Task<IActionResult> DeleteNoteAsync(
            [FromRoute][SwaggerParameter(Description = "ID события для удаления")] int id)
        {
            try
            {
                var deletedNote = await _context.Events
                    .FirstOrDefaultAsync(note => note.Id == id);
                if (deletedNote == null)
                {
                    return NotFound($"Событие с ID = {id} не найдено");
                }

                _context.Events.Remove(deletedNote);
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
