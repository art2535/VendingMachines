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
[SwaggerTag("Контроллер для работы с событиями и заметками по аппаратам")]
public class EventsController : ControllerBase
{
    private VendingMachinesContext _context;
    
    public EventsController(VendingMachinesContext context)
    {
        _context = context;
    }
    
    [HttpGet]
    [SwaggerOperation(Summary = "Получение архива событий с фильтрацией и сортировкой")]
    public async Task<IActionResult> GetAllNotesAsync([FromQuery] string? search = null, [FromQuery] string? eventType = null,
        [FromQuery] DateTime? date = null, [FromQuery] string? sortBy = "date", [FromQuery] string? sortOrder = "desc")
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
    public async Task<IActionResult> GetNoteByIdAsync(int id)
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
    [ProducesResponseType(typeof(Event), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddNoteAsync([FromBody] NotesRequest request)
    {
        var device = await _context.Devices
            .FirstOrDefaultAsync(d => d.Id == request.Device.Id);
        if (device == null)
        {
            return BadRequest($"Аппарат с ID {request.Device.Id} не найден");
        }

        var newEvent = new Event
        {
            DeviceId = device.Id,
            EventType = request.EventType,
            Description = request.Description,
            MediaPath = request.PhotoUrl,
            DateTime = request.EventDate
        };

        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync();
        
        return Created($"api/events/{newEvent.Id}", newEvent);
    }

    [HttpPut("{id:int}")]
    [SwaggerOperation(Summary = "Обновление события")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateNoteAsync(int id, [FromBody] NotesRequest request)
    {
        var existingEvent = await _context.Events.FindAsync(id);
        if (existingEvent == null)
            return NotFound();

        existingEvent.EventType = request.EventType ?? existingEvent.EventType;
        existingEvent.Description = request.Description ?? existingEvent.Description;
        existingEvent.DateTime = request.EventDate;
        existingEvent.MediaPath = request.PhotoUrl;

        if (request.Device?.Id != null && request.Device.Id != existingEvent.DeviceId)
        {
            var device = await _context.Devices.FindAsync(request.Device.Id);
            if (device == null) 
                return BadRequest("Аппарат не найден");
            existingEvent.DeviceId = device.Id;
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [SwaggerOperation(Summary = "Удаление события")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteNoteAsync(int id)
    {
        var deletedNote = await _context.Events
            .FirstOrDefaultAsync(note => note.Id == id);
        if (deletedNote == null)
        {
            return NotFound();
        }
        
        _context.Events.Remove(deletedNote);
        return NoContent();
    }
}