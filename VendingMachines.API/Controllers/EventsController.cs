using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendingMachines.Core.Models;
using VendingMachines.DTOs.Events;
using VendingMachines.Infrastructure.Data;
using Swashbuckle.AspNetCore.Annotations;

namespace VendingMachines.API.Controllers;

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
    [SwaggerResponse(StatusCodes.Status200OK, "Список событий получен", typeof(List<NotesRequest>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
    public async Task<IActionResult> GetAllNotesAsync(
        [FromQuery][SwaggerParameter(Description = "Поиск по типу события, описанию, адресу, модели или компании")] string? search = null,
        [FromQuery][SwaggerParameter(Description = "Фильтр по типу события (например: 'maintenance', 'error')")] string? eventType = null,
        [FromQuery][SwaggerParameter(Description = "Фильтр по дате события (вся дата)")] DateTime? date = null,
        [FromQuery][SwaggerParameter(Description = "Поле сортировки: 'date' или 'type' (по умолчанию 'date')")] string? sortBy = "date",
        [FromQuery][SwaggerParameter(Description = "Направление сортировки: 'asc' или 'desc' (по умолчанию 'desc')")] string? sortOrder = "desc")
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

        if (!string.IsNullOrWhiteSpace(eventType))
        {
            query = query.Where(e => e.EventType == eventType.Trim());
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

        var result = query.Select(e => new NotesRequest
        {
            Id = e.Id,
            EventType = e.EventType ?? "",
            Description = e.Description ?? "",
            PhotoUrl = e.MediaPath,
            EventDate = e.DateTime,
            Device = e.Device != null ? new DeviceRequest
            {
                Id = e.Device.Id,
                DeviceModel = e.Device.DeviceModel != null ? e.Device.DeviceModel.Name : "Неизвестная модель",
                Location = e.Device.Location != null ? e.Device.Location.InstallationAddress : "Адрес не указан",
                Modem = e.Device.Modem != null ? e.Device.Modem.Brand : "Модем не указан",
                DeviceStatus = e.Device.DeviceStatus != null ? e.Device.DeviceStatus.Name : "Статус неизвестен",
                Company = e.Device.Company != null ? e.Device.Company.Name : "Компания не указана",
                InstallationDate = e.Device.InstallationDate,
                LastServiceDate = e.Device.LastServiceDate,
                CreatedAt = e.Device.CreatedAt,
                UpdatedAt = e.Device.UpdatedAt
            } : null
        });

        return Ok(await result.ToListAsync());
    }

    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "Получение события по ID")]
    [SwaggerResponse(StatusCodes.Status200OK, "Событие найдено", typeof(NotesRequest))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Событие не найдено")]
    public async Task<IActionResult> GetNoteByIdAsync(
        [FromRoute][SwaggerParameter(Description = "ID события")] int id)
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
            return NotFound();

        var result = new NotesRequest
        {
            Id = eventEntity.Id,
            EventType = eventEntity.EventType ?? "",
            Description = eventEntity.Description ?? "",
            PhotoUrl = eventEntity.MediaPath,
            EventDate = eventEntity.DateTime,
            Device = eventEntity.Device != null ? new DeviceRequest
            {
                Id = eventEntity.Device.Id,
                DeviceModel = eventEntity.Device.DeviceModel?.Name ?? "Неизвестная модель",
                Location = eventEntity.Device.Location?.InstallationAddress ?? "Адрес не указан",
                Modem = eventEntity.Device.Modem?.Brand ?? "Модем не указан",
                DeviceStatus = eventEntity.Device.DeviceStatus?.Name ?? "Статус неизвестен",
                Company = eventEntity.Device.Company?.Name ?? "Компания не указана",
                InstallationDate = eventEntity.Device.InstallationDate,
                LastServiceDate = eventEntity.Device.LastServiceDate,
                CreatedAt = eventEntity.Device.CreatedAt,
                UpdatedAt = eventEntity.Device.UpdatedAt
            } : null
        };

        return Ok(result);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Создание новой заметки/события")]
    [SwaggerResponse(StatusCodes.Status201Created, "Событие создано", typeof(Event))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Некорректные данные или аппарат не найден")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
    public async Task<IActionResult> AddNoteAsync(
        [FromBody][SwaggerParameter(Description = "Данные нового события/заметки")] NotesRequest request)
    {
        var deviceId = request.Device?.Id ?? request.DeviceId;
        if (deviceId == null || deviceId <= 0)
            return BadRequest("Не указан ID аппарата");

        var device = await _context.Devices.FindAsync(deviceId);
        if (device == null)
            return BadRequest($"Аппарат с ID {deviceId} не найден");

        var newEvent = new Event
        {
            DeviceId = device.Id,
            EventType = request.EventType,
            Description = request.Description,
            MediaPath = request.PhotoUrl,
            DateTime = request.EventDate ?? DateTime.UtcNow
        };

        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync();

        return Created($"api/events/{newEvent.Id}", newEvent);
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
        var existingEvent = await _context.Events.FindAsync(id);
        if (existingEvent == null)
            return NotFound();

        var targetDeviceId = request.Device?.Id ?? request.DeviceId ?? existingEvent.DeviceId;
        if (targetDeviceId != existingEvent.DeviceId)
        {
            var device = await _context.Devices.FindAsync(targetDeviceId);
            if (device == null)
                return BadRequest("Аппарат не найден");
            existingEvent.DeviceId = targetDeviceId;
        }

        existingEvent.EventType = request.EventType ?? existingEvent.EventType;
        existingEvent.Description = request.Description ?? existingEvent.Description;
        existingEvent.DateTime = request.EventDate;
        existingEvent.MediaPath = request.PhotoUrl;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [SwaggerOperation(Summary = "Удаление события")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Событие удалено")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Событие не найдено")]
    public async Task<IActionResult> DeleteNoteAsync(
        [FromRoute][SwaggerParameter(Description = "ID события для удаления")] int id)
    {
        var deletedNote = await _context.Events
            .FirstOrDefaultAsync(note => note.Id == id);
        if (deletedNote == null)
        {
            return NotFound();
        }

        _context.Events.Remove(deletedNote);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
