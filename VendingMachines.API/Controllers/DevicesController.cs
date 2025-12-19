using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendingMachines.API.DTOs.Devices;
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
    [SwaggerTag("Контроллер для управления вендинговыми аппаратами")]
    public class DevicesController : ControllerBase
    {
        private readonly VendingMachinesContext _context;

        public DevicesController(VendingMachinesContext context)
        {
            _context = context;
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Список аппаратов с фильтрацией и пагинацией",
            Description = "Возвращает аппараты с информацией о моделях, компаниях, модемах и локациях. Поддерживает поиск по названию")]
        [SwaggerResponse(StatusCodes.Status200OK, "Список аппаратов с пагинацией", typeof(object))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        public async Task<IActionResult> GetDevicesAsync(
            [FromQuery][SwaggerParameter(Description = "Количество аппаратов на странице (по умолчанию 10)")] int limit = 10,
            [FromQuery][SwaggerParameter(Description = "Смещение для пагинации (по умолчанию 0)")] int offset = 0,
            [FromQuery][SwaggerParameter(Description = "Фильтр поиска по названию модели, типа или компании")] string nameFilter = "")
        {
            var query = from device in _context.Devices
                        join model in _context.DeviceModels on device.DeviceModelId equals model.Id
                        join type in _context.DeviceTypes on model.DeviceTypeId equals type.Id
                        join company in _context.Companies on device.CompanyId equals company.Id into compJoin
                        from company in compJoin.DefaultIfEmpty()
                        join modem in _context.Modems on device.ModemId equals modem.Id into modemJoin
                        from modem in modemJoin.DefaultIfEmpty()
                        join location in _context.Locations on device.LocationId equals location.Id into locJoin
                        from location in locJoin.DefaultIfEmpty()
                        where string.IsNullOrEmpty(nameFilter)
                            || model.Name.ToLower().Contains(nameFilter.ToLower())
                            || type.Name.ToLower().Contains(nameFilter.ToLower())
                            || (company != null && company.Name.ToLower().Contains(nameFilter.ToLower()))
                        orderby device.Id ascending
                        select new DeviceListItem
                        {
                            Id = device.Id,
                            Name = model.Name,
                            Model = type.Name,
                            Company = company != null ? company.Name : "—",
                            ModemId = modem != null ? modem.Id : 0,
                            Address = location != null ? location.InstallationAddress : "—",
                            InstallationDate = device.InstallationDate
                        };

            int totalCount = await query.CountAsync();
            var result = await query
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            return Ok(new
            {
                totalCount,
                items = result
            });
        }

        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Получение аппарата по ID")]
        [SwaggerResponse(StatusCodes.Status200OK, "Аппарат найден", typeof(DeviceListItem))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Аппарат не найден")]
        public async Task<IActionResult> GetDeviceByIdAsync(
            [FromRoute][SwaggerParameter(Description = "ID аппарата")] int id)
        {
            var query = from device in _context.Devices
                        join model in _context.DeviceModels on device.DeviceModelId equals model.Id
                        join type in _context.DeviceTypes on model.DeviceTypeId equals type.Id
                        join company in _context.Companies on device.CompanyId equals company.Id into compJoin
                        from company in compJoin.DefaultIfEmpty()
                        join modem in _context.Modems on device.ModemId equals modem.Id into modemJoin
                        from modem in modemJoin.DefaultIfEmpty()
                        join location in _context.Locations on device.LocationId equals location.Id into locJoin
                        from location in locJoin.DefaultIfEmpty()
                        where device.Id == id
                        select new DeviceListItem
                        {
                            Id = device.Id,
                            Name = model.Name,
                            Model = type.Name,
                            Company = company != null ? company.Name : "—",
                            ModemId = modem != null ? modem.Id : 0,
                            Address = location != null ? location.InstallationAddress : "—",
                            Place = location != null ? location.PlaceDescription : "—",
                            InstallationDate = device.InstallationDate
                        };

            var result = await query.FirstOrDefaultAsync();

            return Ok(result);
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Создание нового аппарата")]
        [SwaggerResponse(StatusCodes.Status201Created, "Аппарат успешно создан", typeof(Device))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Некорректные данные")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Ошибка сервера")]
        public async Task<IActionResult> CreateNewDeviceAsync(
            [FromBody][SwaggerParameter(Description = "Данные нового аппарата")] Device device)
        {
            if (device == null)
            {
                return BadRequest("Устройство не может быть null.");
            }

            device.CreatedAt = DateTime.UtcNow;
            device.UpdatedAt = DateTime.UtcNow;

            _context.Devices.Add(device);
            await _context.SaveChangesAsync();

            var createdDevice = await _context.Devices
                .Include(d => d.DeviceModel)
                .Include(d => d.Location)
                .Include(d => d.Modem)
                .Include(d => d.DeviceStatus)
                .Include(d => d.Company)
                .Where(d => d.Id == device.Id)
                .FirstOrDefaultAsync();

            if (createdDevice == null)
            {
                return StatusCode(500, "Ошибка при создании устройства: устройство не найдено после сохранения.");
            }

            return Created($"api/devices/{createdDevice.Id}", createdDevice);
        }

        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Полное обновление аппарата")]
        [SwaggerResponse(StatusCodes.Status200OK, "Аппарат успешно обновлен", typeof(Device))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Аппарат не найден")]
        public async Task<IActionResult> UpdateDeviceAsync(
            [FromRoute][SwaggerParameter(Description = "ID аппарата для обновления")] int id,
            [FromBody][SwaggerParameter(Description = "Обновленные данные аппарата")] DeviceUpdateDto dto)
        {
            var existingDevice = await _context.Devices
                .Include(d => d.Location)
                .AsTracking()
                .FirstOrDefaultAsync(d => d.Id == id);

            if (existingDevice == null)
                return NotFound("Устройство с указанным ID не найдено");

            existingDevice.UpdatedAt = DateTime.UtcNow;
            existingDevice.DeviceModelId = dto.DeviceModelId;
            existingDevice.ModemId = dto.ModemId;
            existingDevice.CompanyId = dto.CompanyId;
            existingDevice.DeviceStatusId = dto.DeviceStatusId;
            existingDevice.CreatedAt = dto.CreatedAt ?? existingDevice.CreatedAt;
            existingDevice.InstallationDate = dto.InstallationDate != default ? dto.InstallationDate : existingDevice.InstallationDate;
            existingDevice.LastServiceDate = dto.LastServiceDate ?? existingDevice.LastServiceDate;

            if (dto.Location != null)
            {
                if (existingDevice.Location == null || !existingDevice.LocationId.HasValue)
                {
                    var newLocation = new Location
                    {
                        InstallationAddress = dto.Location.InstallationAddress ?? string.Empty,
                        PlaceDescription = dto.Location.PlaceDescription ?? string.Empty
                    };
                    _context.Locations.Add(newLocation);
                    await _context.SaveChangesAsync();

                    existingDevice.LocationId = newLocation.Id;
                }
                else
                {
                    var existingLocation = await _context.Locations
                        .FirstOrDefaultAsync(l => l.Id == existingDevice.LocationId);
                    if (existingLocation != null)
                    {
                        existingLocation.InstallationAddress = dto.Location.InstallationAddress
                            ?? existingLocation.InstallationAddress;
                        existingLocation.PlaceDescription = dto.Location.PlaceDescription ?? existingLocation.PlaceDescription;
                        _context.Entry(existingLocation).State = EntityState.Modified;
                    }
                }
            }

            _context.Entry(existingDevice).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            var updated = await _context.Devices
                .Include(d => d.DeviceModel)
                .Include(d => d.Location)
                .Include(d => d.Company)
                .Include(d => d.Modem)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id);

            return Ok(updated);
        }

        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Удаление аппарата")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Аппарат успешно удален")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Аппарат не найден")]
        public async Task<IActionResult> DeleteDeviceAsync(
            [FromRoute][SwaggerParameter(Description = "ID аппарата для удаления")] int id)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null)
            {
                return NotFound();
            }

            _context.Devices.Remove(device);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("{id}/detach-modem")]
        [SwaggerOperation(Summary = "Отвязка модема от аппарата")]
        [SwaggerResponse(StatusCodes.Status200OK, "Модем отвязан", typeof(Device))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Аппарат не найден")]
        public async Task<IActionResult> DetachModemAsync(
            [FromRoute][SwaggerParameter(Description = "ID аппарата")] int id)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null)
            {
                return NotFound();
            }

            device.ModemId = null;

            _context.Devices.Update(device);
            await _context.SaveChangesAsync();

            return Ok(device);
        }

        [HttpGet("companies")]
        [SwaggerOperation(Summary = "Список компаний (для выпадающих списков)")]
        [SwaggerResponse(StatusCodes.Status200OK, "Список компаний", typeof(List<object>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        public async Task<IActionResult> GetCompaniesAsync()
        {
            var companies = await _context.Companies
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();
            return Ok(companies);
        }

        [HttpGet("devicemodels")]
        [SwaggerOperation(Summary = "Список моделей аппаратов")]
        [SwaggerResponse(StatusCodes.Status200OK, "Список моделей", typeof(List<object>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        public async Task<IActionResult> GetDeviceModelsAsync()
        {
            var models = await _context.DeviceModels
                .Select(m => new { m.Id, m.Name })
                .ToListAsync();
            return Ok(models);
        }

        [HttpGet("modems")]
        [SwaggerOperation(Summary = "Список модемов")]
        [SwaggerResponse(StatusCodes.Status200OK, "Список модемов", typeof(List<object>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        public async Task<IActionResult> GetModemsAsync()
        {
            var modems = await _context.Modems
                .Select(m => new { m.Id, m.SerialNumber })
                .ToListAsync();
            return Ok(modems);
        }

        [HttpGet("paymentmethods")]
        [SwaggerOperation(Summary = "Способы оплаты")]
        [SwaggerResponse(StatusCodes.Status200OK, "Список способов оплаты", typeof(List<object>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        public async Task<IActionResult> GetPaymentMethodsAsync()
        {
            var paymentMethods = await _context.PaymentMethods
                .Select(pm => new { pm.Id, pm.Name })
                .ToListAsync();
            return Ok(paymentMethods);
        }

        [HttpGet("locations")]
        [SwaggerOperation(Summary = "Список локаций")]
        [SwaggerResponse(StatusCodes.Status200OK, "Список локаций", typeof(List<object>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        public async Task<IActionResult> GetLocationsAsync()
        {
            var locations = await _context.Locations
                .Select(l => new { l.Id, l.PlaceDescription })
                .ToListAsync();
            return Ok(locations);
        }
    }
}
