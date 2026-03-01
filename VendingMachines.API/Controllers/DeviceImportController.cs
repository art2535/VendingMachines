using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Globalization;
using VendingMachines.API.DTOs.DeviceImport;
using VendingMachines.Core.Models;
using VendingMachines.Infrastructure.Data;

namespace VendingMachines.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[SwaggerTag("Импорт торговых аппаратов из файлов")]
public class DeviceImportController : ControllerBase
{
    private readonly VendingMachinesContext _context;

    public DeviceImportController(VendingMachinesContext context)
    {
        _context = context;
    }

    [HttpPost("upload")]
    [SwaggerOperation(
        Summary = "Импорт торговых аппаратов из файла",
        Description = "Поддерживаются .xlsx и .csv. Обязательные поля: ModelName, " +
                      "CompanyName, Address, InstallationDate (ГГГГ-ММ-ДД). " +
                      "Опционально: StatusName (по умолчанию 'Активен'), ModemSerial (если есть модем).")]
    [SwaggerResponse(StatusCodes.Status200OK, "Импорт успешен", typeof(ImportResult))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Ошибки валидации или формата")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new ImportResult
            {
                Success = false,
                Errors = ["Файл не загружен."]
            });
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".xlsx" && extension != ".csv")
        {
            return BadRequest(new ImportResult
            {
                Success = false,
                Errors = ["Поддерживаются только .xlsx и .csv"]
            });
        }

        var records = new List<DeviceImportDto>();
        var errors = new List<string>();

        try
        {
            using var stream = file.OpenReadStream();

            if (extension == ".xlsx")
            {
                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RowsUsed().Skip(1);
                int rowNum = 2;

                foreach (var row in rows)
                {
                    try
                    {
                        records.Add(new DeviceImportDto
                        {
                            ModelName = row.Cell(1).GetValue<string>()?.Trim(),
                            ModelDescription = row.Cell(2).GetValue<string>()?.Trim(),
                            DeviceTypeName = row.Cell(3).GetValue<string>()?.Trim(),
                            DeviceTypeDescription = row.Cell(4).GetValue<string>()?.Trim(),
                            CompanyName = row.Cell(5).GetValue<string>()?.Trim(),
                            CompanyContactEmail = row.Cell(6).GetValue<string>()?.Trim(),
                            CompanyContactPhone = row.Cell(7).GetValue<string>()?.Trim(),
                            CompanyAddress = row.Cell(8).GetValue<string>()?.Trim(),
                            Address = row.Cell(9).GetValue<string>()?.Trim(),
                            PlaceDescription = row.Cell(10).GetValue<string>()?.Trim(),
                            InstallationDate = row.Cell(11).GetValue<string>()?.Trim(),
                            LastServiceDate = row.Cell(12).GetValue<string>()?.Trim(),
                            StatusName = row.Cell(13).GetValue<string>()?.Trim(),
                            ModemSerial = row.Cell(14).GetValue<string>()?.Trim(),
                        });
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Строка {rowNum}: ошибка чтения — {ex.Message}");
                    }
                    rowNum++;
                }
            }
            else
            {
                using var reader = new StreamReader(stream);
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    MissingFieldFound = null,
                    BadDataFound = null,
                    TrimOptions = TrimOptions.Trim,
                });

                records = csv.GetRecords<DeviceImportDto>().ToList();
            }
        }
        catch (Exception ex)
        {
            return BadRequest(new ImportResult 
            { 
                Success = false, 
                Errors = [$"Ошибка чтения файла: {ex.Message}"] 
            });
        }

        for (int i = 0; i < records.Count; i++)
        {
            int rowNum = i + 2;
            var r = records[i];

            if (string.IsNullOrWhiteSpace(r.ModelName)) errors.Add($"Строка {rowNum}: ModelName обязательно");
            if (string.IsNullOrWhiteSpace(r.CompanyName)) errors.Add($"Строка {rowNum}: CompanyName обязательно");
            if (string.IsNullOrWhiteSpace(r.Address)) errors.Add($"Строка {rowNum}: Address обязательно");
            if (string.IsNullOrWhiteSpace(r.InstallationDate) || !DateOnly.TryParse(r.InstallationDate, out _))
                errors.Add($"Строка {rowNum}: InstallationDate в формате ГГГГ-ММ-ДД");
        }

        if (errors.Count > 0)
        {
            return BadRequest(new ImportResult
            {
                Success = false,
                Errors = errors
            });
        }

        int imported = 0;

        foreach (var r in records)
        {
            try
            {
                int? deviceTypeId = null;
                if (!string.IsNullOrWhiteSpace(r.DeviceTypeName))
                {
                    var deviceType = await _context.DeviceTypes
                        .FirstOrDefaultAsync(t => t.Name == r.DeviceTypeName.Trim());

                    if (deviceType == null)
                    {
                        deviceType = new DeviceType
                        {
                            Name = r.DeviceTypeName.Trim(),
                            Description = r.DeviceTypeDescription?.Trim()
                        };
                        _context.DeviceTypes.Add(deviceType);
                        await _context.SaveChangesAsync();
                    }

                    deviceTypeId = deviceType.Id;
                }

                var deviceModel = await _context.DeviceModels
                    .FirstOrDefaultAsync(m => m.Name == r.ModelName.Trim());

                if (deviceModel == null)
                {
                    deviceModel = new DeviceModel
                    {
                        Name = r.ModelName.Trim(),
                        Description = r.ModelDescription?.Trim(),
                        DeviceTypeId = deviceTypeId
                    };
                    _context.DeviceModels.Add(deviceModel);
                    await _context.SaveChangesAsync();
                }

                var company = await _context.Companies
                    .FirstOrDefaultAsync(c => c.Name == r.CompanyName.Trim());

                if (company == null)
                {
                    company = new Company
                    {
                        Name = r.CompanyName.Trim(),
                        ContactEmail = r.CompanyContactEmail?.Trim(),
                        ContactPhone = r.CompanyContactPhone?.Trim(),
                        Address = r.CompanyAddress?.Trim(),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.Companies.Add(company);
                    await _context.SaveChangesAsync();
                }

                var location = await _context.Locations
                    .FirstOrDefaultAsync(l => l.InstallationAddress == r.Address.Trim());

                if (location == null)
                {
                    location = new Location
                    {
                        InstallationAddress = r.Address.Trim(),
                        PlaceDescription = r.PlaceDescription?.Trim() ?? string.Empty
                    };
                    _context.Locations.Add(location);
                    await _context.SaveChangesAsync();
                }

                int? statusId = null;
                if (!string.IsNullOrWhiteSpace(r.StatusName))
                {
                    var status = await _context.DeviceStatuses
                        .FirstOrDefaultAsync(s => s.Name == r.StatusName.Trim());
                    statusId = status?.Id;
                }
                else
                {
                    var defaultStatus = await _context.DeviceStatuses
                        .FirstOrDefaultAsync(s => s.Name == "Активен");
                    statusId = defaultStatus?.Id;
                }

                int? modemId = null;
                if (!string.IsNullOrWhiteSpace(r.ModemSerial))
                {
                    var modem = await _context.Modems
                        .FirstOrDefaultAsync(m => m.SerialNumber == r.ModemSerial.Trim());
                    modemId = modem?.Id;
                }

                var device = new Device
                {
                    DeviceModelId = deviceModel.Id,
                    CompanyId = company.Id,
                    LocationId = location.Id,
                    ModemId = modemId,
                    DeviceStatusId = statusId,

                    InstallationDate = DateOnly.Parse(r.InstallationDate),

                    LastServiceDate = !string.IsNullOrWhiteSpace(r.LastServiceDate) &&
                                      DateOnly.TryParse(r.LastServiceDate, out var lastSvc)
                        ? lastSvc : null,

                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Devices.Add(device);
                imported++;
            }
            catch (Exception ex)
            {
                errors.Add($"Ошибка импорта строки (модель: {r.ModelName}, адрес: {r.Address}): {ex.Message}");
            }
        }

        if (errors.Count > 0)
        {
            return BadRequest(new ImportResult
            {
                Success = false,
                Errors = errors,
                ImportedCount = imported
            });
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ImportResult
            {
                Success = false,
                Errors = [$"Ошибка финального сохранения: {ex.Message}"]
            });
        }

        return Ok(new ImportResult
        {
            Success = true,
            ImportedCount = imported,
            Message = $"Успешно импортировано {imported} торговых аппаратов."
        });
    }
}