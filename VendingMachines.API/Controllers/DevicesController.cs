using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VendingMachines.API.DTOs;
using VendingMachines.API.DTOs.Company;
using VendingMachines.API.DTOs.Devices;
using VendingMachines.Core.Models;
using VendingMachines.Infrastructure.Data;

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
        [SwaggerResponse(StatusCodes.Status200OK, "Список аппаратов с пагинацией", typeof(DeviceResponse))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        public async Task<IActionResult> GetDevicesAsync(
            [FromQuery][SwaggerParameter(Description = "Фильтр поиска по названию модели, типа или компании")] string nameFilter = "")
        {
            try
            {
                var query = _context.Devices
                    .AsNoTracking()
                    .Include(d => d.DeviceModel)
                        .ThenInclude(m => m.DeviceType)
                    .Include(d => d.DeviceStatus)
                    .Include(d => d.Company)
                    .Include(d => d.Modem)
                    .Include(d => d.Location)
                    .Where(d =>
                        string.IsNullOrEmpty(nameFilter) ||
                        d.DeviceModel.Name.ToLower().Contains(nameFilter.ToLower()) ||
                        d.DeviceModel.DeviceType.Name.ToLower().Contains(nameFilter.ToLower()) ||
                        (d.Company != null && d.Company.Name.ToLower().Contains(nameFilter.ToLower())))
                    .OrderBy(d => d.Id)
                    .Select(d => new DeviceResponse
                    {
                        Id = d.Id,
                        Model = d.DeviceModel != null ? new ModelResponse
                        {
                            Id = d.DeviceModel.Id,
                            Name = d.DeviceModel.Name,
                            Description = d.DeviceModel.Description ?? "",
                            DeviceType = d.DeviceModel.DeviceType != null ? new DeviceTypeResponse
                            {
                                Id = d.DeviceModel.DeviceType.Id,
                                Name = d.DeviceModel.DeviceType.Name,
                                Description = d.DeviceModel.DeviceType.Description ?? ""
                            } : new DeviceTypeResponse()
                        } : new ModelResponse(),
                        Location = d.Location != null ? new LocationResponse
                        {
                            Id = d.Location.Id,
                            InstallationAddress = d.Location.InstallationAddress,
                            PlaceDescription = d.Location.PlaceDescription
                        } : new LocationResponse(),
                        Modem = d.Modem != null ? new ModemResponse
                        {
                            Id = d.Modem.Id,
                            Brand = d.Modem.Brand ?? "",
                            SerialNumber = d.Modem.SerialNumber ?? "",
                            Provider = d.Modem.Provider ?? "",
                            Balance = d.Modem.Balance
                        } : new ModemResponse(),
                        DeviceStatus = d.DeviceStatus != null ? new DeviceStatusResponse
                        {
                            Id = d.DeviceStatus.Id,
                            Name = d.DeviceStatus.Name,
                            ColorCode = d.DeviceStatus.ColorCode ?? ""
                        } : new DeviceStatusResponse(),
                        Company = d.Company != null ? new CompanyResponse
                        {
                            Id = d.Company.Id,
                            Name = d.Company.Name,
                            ContactEmail = d.Company.ContactEmail ?? "не задан",
                            ContactPhone = d.Company.ContactPhone ?? "не задан",
                            Address = d.Company.Address ?? "не задан"
                        } : new CompanyResponse(),
                        InstallationDate = d.InstallationDate,
                        LastServiceDate = d.LastServiceDate
                    });

                return Ok(await query.ToListAsync());
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

        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Получение аппарата по ID")]
        [SwaggerResponse(StatusCodes.Status200OK, "Аппарат найден", typeof(DeviceResponse))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Аппарат не найден")]
        public async Task<IActionResult> GetDeviceByIdAsync([FromRoute][SwaggerParameter(Description = "ID аппарата")] int id)
        {
            try
            {
                var query = _context.Devices
                    .AsNoTracking()
                    .Include(d => d.DeviceModel)
                        .ThenInclude(m => m.DeviceType)
                    .Include(d => d.DeviceStatus)
                    .Include(d => d.Company)
                    .Include(d => d.Modem)
                    .Include(d => d.Location)
                    .Where(d => d.Id == id)
                    .OrderBy(d => d.Id)
                    .Select(d => new DeviceResponse
                    {
                        Id = d.Id,
                        Model = d.DeviceModel != null ? new ModelResponse
                        {
                            Id = d.DeviceModel.Id,
                            Name = d.DeviceModel.Name,
                            Description = d.DeviceModel.Description ?? "",
                            DeviceType = d.DeviceModel.DeviceType != null ? new DeviceTypeResponse
                            {
                                Id = d.DeviceModel.DeviceType.Id,
                                Name = d.DeviceModel.DeviceType.Name,
                                Description = d.DeviceModel.DeviceType.Description ?? ""
                            } : new DeviceTypeResponse()
                        } : new ModelResponse(),
                        Location = d.Location != null ? new LocationResponse
                        {
                            Id = d.Location.Id,
                            InstallationAddress = d.Location.InstallationAddress,
                            PlaceDescription = d.Location.PlaceDescription
                        } : new LocationResponse(),
                        Modem = d.Modem != null ? new ModemResponse
                        {
                            Id = d.Modem.Id,
                            Brand = d.Modem.Brand ?? "",
                            SerialNumber = d.Modem.SerialNumber ?? "",
                            Provider = d.Modem.Provider ?? "",
                            Balance = d.Modem.Balance
                        } : new ModemResponse(),
                        DeviceStatus = d.DeviceStatus != null ? new DeviceStatusResponse
                        {
                            Id = d.DeviceStatus.Id,
                            Name = d.DeviceStatus.Name,
                            ColorCode = d.DeviceStatus.ColorCode ?? ""
                        } : new DeviceStatusResponse(),
                        Company = d.Company != null ? new CompanyResponse
                        {
                            Id = d.Company.Id,
                            Name = d.Company.Name,
                            ContactEmail = d.Company.ContactEmail ?? "не задан",
                            ContactPhone = d.Company.ContactPhone ?? "не задан",
                            Address = d.Company.Address ?? "не задан"
                        } : new CompanyResponse(),
                        InstallationDate = d.InstallationDate,
                        LastServiceDate = d.LastServiceDate
                    });

                var result = await query.FirstOrDefaultAsync();

                if (result == null)
                    return NotFound($"Аппарат с ID = {id} не найден");

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
        [SwaggerOperation(Summary = "Создание нового аппарата")]
        [SwaggerResponse(StatusCodes.Status201Created, "Аппарат успешно создан", typeof(DeviceResponse))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Некорректные данные")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Ошибка сервера")]
        public async Task<IActionResult> CreateNewDeviceAsync(
            [FromBody][SwaggerParameter(Description = "Данные нового аппарата")] DeviceRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Устройство не может быть null.");
                }

                var device = new Device
                {
                    Id = await _context.Devices.MaxAsync(d => d.Id) + 1,
                    DeviceModelId = request.DeviceModelId,
                    LocationId = request.LocationId,
                    ModemId = request.ModemId,
                    DeviceStatusId = request.DeviceStatusId,
                    CompanyId = request.CompanyId,
                    InstallationDate = request.InstallationDate,
                    LastServiceDate = request.LastServiceDate,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Devices.Add(device);
                await _context.SaveChangesAsync();

                var query = _context.Devices
                    .AsNoTracking()
                    .Include(d => d.DeviceModel)
                        .ThenInclude(m => m.DeviceType)
                    .Include(d => d.DeviceStatus)
                    .Include(d => d.Company)
                    .Include(d => d.Modem)
                    .Include(d => d.Location)
                    .Where(d => d.Id == device.Id)
                    .OrderBy(d => d.Id)
                    .Select(d => new DeviceResponse
                    {
                        Id = d.Id,
                        Model = d.DeviceModel != null ? new ModelResponse
                        {
                            Id = d.DeviceModel.Id,
                            Name = d.DeviceModel.Name,
                            Description = d.DeviceModel.Description ?? "",
                            DeviceType = d.DeviceModel.DeviceType != null ? new DeviceTypeResponse
                            {
                                Id = d.DeviceModel.DeviceType.Id,
                                Name = d.DeviceModel.DeviceType.Name,
                                Description = d.DeviceModel.DeviceType.Description ?? ""
                            } : new DeviceTypeResponse()
                        } : new ModelResponse(),
                        Location = d.Location != null ? new LocationResponse
                        {
                            Id = d.Location.Id,
                            InstallationAddress = d.Location.InstallationAddress,
                            PlaceDescription = d.Location.PlaceDescription
                        } : new LocationResponse(),
                        Modem = d.Modem != null ? new ModemResponse
                        {
                            Id = d.Modem.Id,
                            Brand = d.Modem.Brand ?? "",
                            SerialNumber = d.Modem.SerialNumber ?? "",
                            Provider = d.Modem.Provider ?? "",
                            Balance = d.Modem.Balance
                        } : new ModemResponse(),
                        DeviceStatus = d.DeviceStatus != null ? new DeviceStatusResponse
                        {
                            Id = d.DeviceStatus.Id,
                            Name = d.DeviceStatus.Name,
                            ColorCode = d.DeviceStatus.ColorCode ?? ""
                        } : new DeviceStatusResponse(),
                        Company = d.Company != null ? new CompanyResponse
                        {
                            Id = d.Company.Id,
                            Name = d.Company.Name,
                            ContactEmail = d.Company.ContactEmail ?? "не задан",
                            ContactPhone = d.Company.ContactPhone ?? "не задан",
                            Address = d.Company.Address ?? "не задан"
                        } : new CompanyResponse(),
                        InstallationDate = d.InstallationDate,
                        LastServiceDate = d.LastServiceDate
                    });

                var createdDevice = await query.FirstOrDefaultAsync();

                if (createdDevice == null)
                {
                    return NotFound("Ошибка при создании устройства: устройство не найдено после сохранения");
                }

                return Created($"api/devices/{device.Id}", createdDevice);
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

        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Полное обновление аппарата")]
        [SwaggerResponse(StatusCodes.Status200OK, "Аппарат успешно обновлен", typeof(DeviceResponse))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Аппарат не найден")]
        public async Task<IActionResult> UpdateDeviceAsync(
            [FromRoute][SwaggerParameter(Description = "ID аппарата для обновления")] int id,
            [FromBody][SwaggerParameter(Description = "Обновленные данные аппарата")] DeviceUpdateResponse request)
        {
            try
            {
                var existingDevice = await _context.Devices
                    .Include(d => d.Location)
                    .AsTracking()
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (existingDevice == null)
                    return NotFound($"Аппарат с ID = {id} не найден");

                existingDevice.UpdatedAt = DateTime.UtcNow;
                existingDevice.DeviceModelId = request.DeviceModelId;
                existingDevice.LocationId = request.LocationId;
                existingDevice.ModemId = request.ModemId;
                existingDevice.CompanyId = request.CompanyId;
                existingDevice.DeviceStatusId = request.DeviceStatusId;
                existingDevice.InstallationDate = request.InstallationDate != default ? request.InstallationDate : existingDevice.InstallationDate;
                existingDevice.LastServiceDate = request.LastServiceDate ?? existingDevice.LastServiceDate;

                _context.Devices.Update(existingDevice);
                await _context.SaveChangesAsync();

                var updated = await _context.Devices
                    .AsNoTracking()
                    .Include(d => d.DeviceModel)
                        .ThenInclude(m => m.DeviceType)
                    .Include(d => d.DeviceStatus)
                    .Include(d => d.Company)
                    .Include(d => d.Modem)
                    .Include(d => d.Location)
                    .Where(d => d.Id == id)
                    .Select(d => new DeviceResponse
                    {
                        Id = d.Id,
                        Model = d.DeviceModel != null ? new ModelResponse
                        {
                            Id = d.DeviceModel.Id,
                            Name = d.DeviceModel.Name,
                            Description = d.DeviceModel.Description ?? "",
                            DeviceType = d.DeviceModel.DeviceType != null ? new DeviceTypeResponse
                            {
                                Id = d.DeviceModel.DeviceType.Id,
                                Name = d.DeviceModel.DeviceType.Name,
                                Description = d.DeviceModel.DeviceType.Description ?? ""
                            } : new DeviceTypeResponse()
                        } : new ModelResponse(),
                        Location = d.Location != null ? new LocationResponse
                        {
                            Id = d.Location.Id,
                            InstallationAddress = d.Location.InstallationAddress,
                            PlaceDescription = d.Location.PlaceDescription
                        } : new LocationResponse(),
                        Modem = d.Modem != null ? new ModemResponse
                        {
                            Id = d.Modem.Id,
                            Brand = d.Modem.Brand ?? "",
                            SerialNumber = d.Modem.SerialNumber ?? "",
                            Provider = d.Modem.Provider ?? "",
                            Balance = d.Modem.Balance
                        } : new ModemResponse(),
                        DeviceStatus = d.DeviceStatus != null ? new DeviceStatusResponse
                        {
                            Id = d.DeviceStatus.Id,
                            Name = d.DeviceStatus.Name,
                            ColorCode = d.DeviceStatus.ColorCode ?? ""
                        } : new DeviceStatusResponse(),
                        Company = d.Company != null ? new CompanyResponse
                        {
                            Id = d.Company.Id,
                            Name = d.Company.Name,
                            ContactEmail = d.Company.ContactEmail ?? "не задан",
                            ContactPhone = d.Company.ContactPhone ?? "не задан",
                            Address = d.Company.Address ?? "не задан"
                        } : new CompanyResponse(),
                        InstallationDate = d.InstallationDate,
                        LastServiceDate = d.LastServiceDate
                    })
                    .FirstOrDefaultAsync();

                if (updated == null)
                {
                    return NotFound($"Обновленный аппарат с ID = {id} не найден");
                }

                return Ok(updated);
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

        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Удаление аппарата")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Аппарат успешно удален")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Аппарат не найден")]
        public async Task<IActionResult> DeleteDeviceAsync(
            [FromRoute][SwaggerParameter(Description = "ID аппарата для удаления")] int id)
        {
            try
            {
                var device = await _context.Devices.FindAsync(id);
                if (device == null)
                {
                    return NotFound($"Аппарат с ID = {id} не найден");
                }

                _context.Devices.Remove(device);
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

        [HttpPatch("{id}/detach-modem")]
        [SwaggerOperation(Summary = "Отвязка модема от аппарата")]
        [SwaggerResponse(StatusCodes.Status200OK, "Модем отвязан", typeof(DeviceResponse))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Аппарат не найден")]
        public async Task<IActionResult> DetachModemAsync(
            [FromRoute][SwaggerParameter(Description = "ID аппарата")] int id)
        {
            try
            {
                var device = await _context.Devices.FindAsync(id);
                if (device == null)
                {
                    return NotFound($"Аппарат с ID = {id} не найден");
                }

                device.ModemId = null;

                _context.Devices.Update(device);
                await _context.SaveChangesAsync();

                var updated = await _context.Devices
                    .AsNoTracking()
                    .Include(d => d.DeviceModel)
                        .ThenInclude(m => m.DeviceType)
                    .Include(d => d.DeviceStatus)
                    .Include(d => d.Company)
                    .Include(d => d.Modem)
                    .Include(d => d.Location)
                    .Where(d => d.Id == id)
                    .Select(d => new DeviceResponse
                    {
                        Id = d.Id,
                        Model = d.DeviceModel != null ? new ModelResponse
                        {
                            Id = d.DeviceModel.Id,
                            Name = d.DeviceModel.Name,
                            Description = d.DeviceModel.Description ?? "",
                            DeviceType = d.DeviceModel.DeviceType != null ? new DeviceTypeResponse
                            {
                                Id = d.DeviceModel.DeviceType.Id,
                                Name = d.DeviceModel.DeviceType.Name,
                                Description = d.DeviceModel.DeviceType.Description ?? ""
                            } : new DeviceTypeResponse()
                        } : new ModelResponse(),
                        Location = d.Location != null ? new LocationResponse
                        {
                            Id = d.Location.Id,
                            InstallationAddress = d.Location.InstallationAddress,
                            PlaceDescription = d.Location.PlaceDescription
                        } : new LocationResponse(),
                        Modem = d.Modem != null ? new ModemResponse
                        {
                            Id = d.Modem.Id,
                            Brand = d.Modem.Brand ?? "",
                            SerialNumber = d.Modem.SerialNumber ?? "",
                            Provider = d.Modem.Provider ?? "",
                            Balance = d.Modem.Balance
                        } : new ModemResponse(),
                        DeviceStatus = d.DeviceStatus != null ? new DeviceStatusResponse
                        {
                            Id = d.DeviceStatus.Id,
                            Name = d.DeviceStatus.Name,
                            ColorCode = d.DeviceStatus.ColorCode ?? ""
                        } : new DeviceStatusResponse(),
                        Company = d.Company != null ? new CompanyResponse
                        {
                            Id = d.Company.Id,
                            Name = d.Company.Name,
                            ContactEmail = d.Company.ContactEmail ?? "не задан",
                            ContactPhone = d.Company.ContactPhone ?? "не задан",
                            Address = d.Company.Address ?? "не задан"
                        } : new CompanyResponse(),
                        InstallationDate = d.InstallationDate,
                        LastServiceDate = d.LastServiceDate
                    })
                    .FirstOrDefaultAsync();

                if (updated == null)
                {
                    return NotFound($"Обновленный аппарат с ID = {id} не найден");
                }

                return Ok(updated);
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

        [HttpGet("device-models")]
        [SwaggerOperation(Summary = "Список моделей аппаратов")]
        [SwaggerResponse(StatusCodes.Status200OK, "Список моделей", typeof(List<ModelResponse>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        public async Task<IActionResult> GetDeviceModelsAsync()
        {
            try
            {
                var models = await _context.DeviceModels
                    .Include(dm => dm.DeviceType)
                    .Select(dm => new ModelResponse
                    {
                        Id = dm.Id,
                        Name = dm.Name,
                        DeviceType = dm.DeviceType != null ? new DeviceTypeResponse
                        {
                            Id = dm.DeviceType.Id,
                            Name = dm.DeviceType.Name,
                            Description = dm.DeviceType.Description ?? "нет"
                        } : new DeviceTypeResponse(),
                        Description = dm.Description ?? "нет"
                    })
                    .ToListAsync();

                return Ok(models);
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

        [HttpGet("modems")]
        [SwaggerOperation(Summary = "Список модемов")]
        [SwaggerResponse(StatusCodes.Status200OK, "Список модемов", typeof(List<ModemResponse>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        public async Task<IActionResult> GetModemsAsync()
        {
            try
            {
                var modems = await _context.Modems
                    .Select(m => new ModemResponse
                    {
                        Id = m.Id,
                        Brand = m.Brand,
                        SerialNumber = m.SerialNumber,
                        Provider = m.Provider ?? "нет",
                        Balance = m.Balance ?? 0
                    })
                    .ToListAsync();

                return Ok(modems);
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

        [HttpGet("payment-methods")]
        [SwaggerOperation(Summary = "Способы оплаты")]
        [SwaggerResponse(StatusCodes.Status200OK, "Список способов оплаты", typeof(List<PaymentMethodResponse>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        public async Task<IActionResult> GetPaymentMethodsAsync()
        {
            try
            {
                var paymentMethods = await _context.PaymentMethods
                    .Select(pm => new PaymentMethodResponse
                    {
                        Id = pm.Id,
                        Name = pm.Name
                    })
                    .ToListAsync();

                return Ok(paymentMethods);
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

        [HttpGet("locations")]
        [SwaggerOperation(Summary = "Список локаций")]
        [SwaggerResponse(StatusCodes.Status200OK, "Список локаций", typeof(List<LocationResponse>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        public async Task<IActionResult> GetLocationsAsync()
        {
            try
            {
                var locations = await _context.Locations
                    .Select(l => new LocationResponse
                    {
                        Id = l.Id,
                        InstallationAddress = l.InstallationAddress ?? "не задан",
                        PlaceDescription = l.PlaceDescription ?? "не задан",
                    })
                    .ToListAsync();
                return Ok(locations);
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

        [HttpGet("device-status")]
        [SwaggerOperation(Summary = "Список статусов работы аппаратов")]
        [SwaggerResponse(StatusCodes.Status200OK, "Список статусов работы аппаратов", typeof(List<DeviceStatusResponse>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        public async Task<IActionResult> GetDeviceStatusesAsync()
        {
            try
            {
                var statuses = await _context.DeviceStatuses
                    .Select(ds => new DeviceStatusResponse
                    {
                        Id = ds.Id,
                        Name = ds.Name,
                        ColorCode = ds.ColorCode ?? "не задан"
                    })
                    .ToListAsync();

                return Ok(statuses);
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
