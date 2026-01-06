using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VendingMachines.Core.Models;
using VendingMachines.Infrastructure.Data;
using ClosedXML.Excel;
using CsvHelper;
using System.Globalization;
using VendingMachines.API.DTOs.DeviceImport;

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
                Errors = new()
                {
                    "Файл не загружен."
                }
            });
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".xlsx" && extension != ".csv")
        {
            return BadRequest(new ImportResult
            {
                Success = false,
                Errors = new()
                {
                    "Поддерживаются только .xlsx и .csv"
                }
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
                            CompanyName = row.Cell(2).GetValue<string>()?.Trim(),
                            Address = row.Cell(3).GetValue<string>()?.Trim(),
                            InstallationDateStr = row.Cell(4).GetValue<string>()?.Trim(),
                            StatusName = row.Cell(5).GetValue<string>()?.Trim(),
                            ModemSerial = row.Cell(6).GetValue<string>()?.Trim()
                        });
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Строка {rowNum}: Некорректные данные — {ex.Message}");
                    }
                    rowNum++;
                }
            }
            else
            {
                using var reader = new StreamReader(stream);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                records = csv.GetRecords<DeviceImportDto>().ToList();
            }
        }
        catch (Exception ex)
        {
            return BadRequest(new ImportResult
            {
                Success = false, 
                Errors = new()
                {
                    $"Ошибка чтения файла: {ex.Message}"
                }
            });
        }

        for (int i = 0; i < records.Count; i++)
        {
            int rowNum = i + 2;
            var r = records[i];

            if (string.IsNullOrWhiteSpace(r.ModelName))
                errors.Add($"Строка {rowNum}: Название модели обязательно.");

            if (string.IsNullOrWhiteSpace(r.CompanyName))
                errors.Add($"Строка {rowNum}: Название компании обязательно.");

            if (string.IsNullOrWhiteSpace(r.Address))
                errors.Add($"Строка {rowNum}: Адрес обязателен.");

            if (string.IsNullOrWhiteSpace(r.InstallationDateStr) || !DateOnly.TryParse(r.InstallationDateStr, out _))
                errors.Add($"Строка {rowNum}: Дата установки должна быть в формате ГГГГ-ММ-ДД.");
        }

        if (errors.Any())
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
                var deviceModel = await _context.DeviceModels
                    .FirstOrDefaultAsync(m => m.Name == r.ModelName!.Trim());

                if (deviceModel == null)
                {
                    deviceModel = new DeviceModel { Name = r.ModelName!.Trim() };
                    _context.DeviceModels.Add(deviceModel);
                    await _context.SaveChangesAsync();
                }

                var company = await _context.Companies
                    .FirstOrDefaultAsync(c => c.Name == r.CompanyName!.Trim());

                if (company == null)
                {
                    company = new Company { Name = r.CompanyName!.Trim() };
                    _context.Companies.Add(company);
                    await _context.SaveChangesAsync();
                }

                var location = await _context.Locations
                    .FirstOrDefaultAsync(l => l.InstallationAddress == r.Address!.Trim());

                if (location == null)
                {
                    location = new Location { InstallationAddress = r.Address!.Trim() };
                    _context.Locations.Add(location);
                    await _context.SaveChangesAsync();
                }

                int? statusId = null;
                if (!string.IsNullOrWhiteSpace(r.StatusName))
                {
                    var status = await _context.DeviceStatuses
                        .FirstOrDefaultAsync(s => s.Name == r.StatusName.Trim());

                    if (status != null)
                        statusId = status.Id;
                }

                int? modemId = null;
                if (!string.IsNullOrWhiteSpace(r.ModemSerial))
                {
                    var modem = await _context.Modems
                        .FirstOrDefaultAsync(m => m.SerialNumber == r.ModemSerial.Trim());

                    if (modem != null)
                        modemId = modem.Id;
                }

                var device = new Device
                {
                    DeviceModelId = deviceModel.Id,
                    LocationId = location.Id,
                    CompanyId = company.Id,
                    InstallationDate = DateOnly.Parse(r.InstallationDateStr!),
                    DeviceStatusId = statusId,
                    ModemId = modemId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Devices.Add(device);
                imported++;
            }
            catch (Exception ex)
            {
                errors.Add($"Ошибка при импорте строки: {ex.Message}");
            }
        }

        if (errors.Any())
        {
            return BadRequest(new ImportResult
            {
                Success = false, 
                Errors = errors
            });
        }

        await _context.SaveChangesAsync();

        return Ok(new ImportResult
        {
            Success = true,
            ImportedCount = imported,
            Message = $"Успешно импортировано {imported} торговых аппаратов."
        });
    }
}