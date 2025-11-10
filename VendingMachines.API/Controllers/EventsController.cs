using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendingMachines.Core.Models;
using VendingMachines.DTOs.Events;
using VendingMachines.Infrastructure.Data;

namespace VendingMachines.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private VendingMachinesContext _context;
    
    public EventsController(VendingMachinesContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllNotesAsync()
    {
        var notes = from e in _context.Events
            join d in _context.Devices on e.DeviceId equals d.Id
            join dm in _context.DeviceModels on d.DeviceModelId equals dm.Id
            join m in _context.Modems on d.ModemId equals m.Id
            join l in _context.Locations on d.LocationId equals l.Id
            join ds in _context.DeviceStatuses on d.DeviceStatusId equals ds.Id
            join c in _context.Companies on d.CompanyId equals c.Id
            select new NotesRequest
            {
                Id = e.Id,
                EventType = e.EventType,
                Description = e.Description,
                PhotoUrl = e.MediaPath,
                EventDate = e.DateTime,
                Device = new DeviceRequest
                {
                    Id = d.Id,
                    DeviceModel = dm.Name,
                    Location = l.InstallationAddress,
                    Modem = m.Brand,
                    DeviceStatus = ds.Name,
                    Company = c.Name,
                    InstallationDate = d.InstallationDate,
                    LastServiceDate = d.LastServiceDate,
                    CreatedAt = d.CreatedAt,
                    UpdatedAt = d.UpdatedAt
                }
            };
        
        return Ok(await notes.ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetNoteByIdAsync(int id)
    {
        var note = from e in _context.Events
            join d in _context.Devices on e.DeviceId equals d.Id
            join dm in _context.DeviceModels on d.DeviceModelId equals dm.Id
            join m in _context.Modems on d.ModemId equals m.Id
            join l in _context.Locations on d.LocationId equals l.Id
            join ds in _context.DeviceStatuses on d.DeviceStatusId equals ds.Id
            join c in _context.Companies on d.CompanyId equals c.Id
            select new NotesRequest
            {
                Id = e.Id,
                EventType = e.EventType,
                Description = e.Description,
                PhotoUrl = e.MediaPath,
                EventDate = e.DateTime,
                Device = new DeviceRequest
                {
                    Id = d.Id,
                    DeviceModel = dm.Name,
                    Location = l.InstallationAddress,
                    Modem = m.Brand,
                    DeviceStatus = ds.Name,
                    Company = c.Name,
                    InstallationDate = d.InstallationDate,
                    LastServiceDate = d.LastServiceDate,
                    CreatedAt = d.CreatedAt,
                    UpdatedAt = d.UpdatedAt
                }
            };
        
        return Ok(await note.FirstOrDefaultAsync(d => d.Id == id));
    }

    [HttpPost]
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
    public async Task<IActionResult> UpdateNoteAsync(int id, [FromBody] NotesRequest request)
    {
        var existingEvent = await _context.Events
            .Include(e => e.Device)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (existingEvent == null)
        {
            return NotFound();
        }

        if (request.Id != existingEvent.Id)
        {
            return BadRequest("ID события в теле запроса и URL должны совпадать");
        }
        
        existingEvent.EventType = request.EventType;
        existingEvent.Description = request.Description;
        existingEvent.MediaPath = request.PhotoUrl;
        existingEvent.DateTime = request.EventDate;

        if (request.Device != null)
        {
            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.Id == request.Device.Id);
            if (device != null)
            {
                existingEvent.DeviceId = device.Id;
            }
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
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