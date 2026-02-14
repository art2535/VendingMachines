using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VendingMachines.API.DTOs;
using VendingMachines.API.DTOs.Company;
using VendingMachines.API.DTOs.Devices;
using VendingMachines.API.DTOs.Products;
using VendingMachines.API.DTOs.Sales;
using VendingMachines.Infrastructure.Data;

namespace VendingMachines.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [SwaggerTag("Контроллер для работы с продажами")]
    public class SalesController : ControllerBase
    {
        private readonly VendingMachinesContext _context;

        public SalesController(VendingMachinesContext context)
        {
            _context = context;
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Получение списка продаж",
            Description = "Возвращает продажи с информацией об аппаратах. Поддерживает фильтрацию по устройству и пагинацию")]
        [SwaggerResponse(StatusCodes.Status200OK, "Список продаж получен", typeof(List<SalesResponse>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        public async Task<IActionResult> GetSalesAsync(
            [FromQuery][SwaggerParameter(Description = "ID аппарата для фильтрации (опционально)")] int? deviceId = null)
        {
            try
            {
                var query = _context.Sales
                    .Include(s => s.Device)
                        .ThenInclude(d => d.DeviceModel)
                            .ThenInclude(m => m.DeviceType)
                    .Include(s => s.Device)
                        .ThenInclude(d => d.DeviceStatus)
                    .Include(s => s.Device)
                        .ThenInclude(d => d.Company)
                    .Include(s => s.Device)
                        .ThenInclude(d => d.Modem)
                    .Include(s => s.Device)
                        .ThenInclude(d => d.Location)
                    .Include(s => s.Product)
                    .Include(s => s.PaymentMethod)
                    .AsQueryable();

                if (deviceId.HasValue)
                {
                    query = query.Where(s => s.DeviceId == deviceId);
                }

                var sales = await query
                    .Select(s => new SalesResponse
                    {
                        Device = s.Device != null ? new DeviceResponse
                        {
                            Id = s.Device.Id,
                            Model = s.Device.DeviceModel != null ? new ModelResponse
                            {
                                Id = s.Device.DeviceModel.Id,
                                Name = s.Device.DeviceModel.Name,
                                Description = s.Device.DeviceModel.Description ?? "не задан",
                                DeviceType = s.Device.DeviceModel.DeviceType != null ? new DeviceTypeResponse
                                {
                                    Id = s.Device.DeviceModel.DeviceType.Id,
                                    Name = s.Device.DeviceModel.DeviceType.Name,
                                    Description = s.Device.DeviceModel.DeviceType.Description ?? "не задан"
                                } : new DeviceTypeResponse()
                            } : new ModelResponse(),
                            Location = s.Device.Location != null ? new LocationResponse
                            {
                                Id = s.Device.Location.Id,
                                InstallationAddress = s.Device.Location.InstallationAddress,
                                PlaceDescription = s.Device.Location.PlaceDescription
                            } : new LocationResponse(),
                            Modem = s.Device.Modem != null ? new ModemResponse
                            {
                                Id = s.Device.Modem.Id,
                                Brand = s.Device.Modem.Brand ?? "не задан",
                                SerialNumber = s.Device.Modem.SerialNumber ?? "не задан",
                                Provider = s.Device.Modem.Provider ?? "не задан",
                                Balance = s.Device.Modem.Balance
                            } : new ModemResponse(),
                            DeviceStatus = s.Device.DeviceStatus != null ? new DeviceStatusResponse
                            {
                                Id = s.Device.DeviceStatus.Id,
                                Name = s.Device.DeviceStatus.Name,
                                ColorCode = s.Device.DeviceStatus.ColorCode ?? "не задан"
                            } : new DeviceStatusResponse(),
                            Company = s.Device.Company != null ? new CompanyResponse
                            {
                                Id = s.Device.Company.Id,
                                Name = s.Device.Company.Name,
                                ContactEmail = s.Device.Company.ContactEmail ?? "не задан",
                                ContactPhone = s.Device.Company.ContactPhone ?? "не задан",
                                Address = s.Device.Company.Address ?? "не задан"
                            } : new CompanyResponse(),
                            InstallationDate = s.Device.InstallationDate,
                            LastServiceDate = s.Device.LastServiceDate
                        } : new DeviceResponse(),
                        Product = s.Product != null ? new ProductResponse
                        {
                            Name = s.Product.Name,
                            Description = s.Product.Description ?? "не задан",
                            Price = s.Product.Price,
                            SalesPopularity = s.Product.SalesPopularity ?? 0,
                            CreatedAt = s.Product.CreatedAt ?? DateTime.UtcNow,
                        } : new ProductResponse(),
                        SaleDateTime = s.SaleDateTime ?? DateTime.UtcNow,
                        PaymentMethod = s.PaymentMethod != null ? new PaymentMethodResponse
                        {
                            Id = s.PaymentMethod.Id,
                            Name = s.PaymentMethod.Name,
                        } : new PaymentMethodResponse()
                    })
                    .ToListAsync();

                return Ok(sales);
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
