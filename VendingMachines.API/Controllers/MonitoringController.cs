using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendingMachines.API.DTOs.Monitoring;
using VendingMachines.Infrastructure.Data;
using Swashbuckle.AspNetCore.Annotations;

namespace VendingMachines.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [SwaggerTag("Контроллер мониторинга и аналитики")]
    public class MonitoringController : ControllerBase
    {
        private readonly VendingMachinesContext _context;

        public MonitoringController(VendingMachinesContext context)
        {
            _context = context;
        }

        [HttpGet("network-status")]
        [SwaggerOperation(
            Summary = "Сетевой статус аппаратов",
            Description = "Фильтрация по статусу и типу подключения + статистика эффективности и выручки")]
        [SwaggerResponse(StatusCodes.Status200OK, "Сетевой статус получен", typeof(object))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        public async Task<IActionResult> GetNetworkStatusAsync(
            [FromQuery][SwaggerParameter(Description = "Фильтр по статусу аппарата")] string status = "",
            [FromQuery][SwaggerParameter(Description = "Фильтр по типу подключения")] string connectionType = "")
        {
            var query = _context.Devices
                .Include(d => d.DeviceStatus)
                .Include(d => d.Modem)
                .Include(d => d.DeviceModel)
                    .ThenInclude(dm => dm.DeviceType)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(d => d.DeviceStatus != null &&
                                         d.DeviceStatus.Name.Contains(status, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(connectionType))
            {
                query = query.Where(d => d.DeviceModel != null &&
                                         d.DeviceModel.DeviceType != null &&
                                         d.DeviceModel.DeviceType.Name.Contains(connectionType,
                                             StringComparison.OrdinalIgnoreCase));
            }

            var devices = await query.OrderBy(d => d.Id).ToListAsync();

            var activeCount = devices.Count(d =>
                d.DeviceStatus != null &&
                d.DeviceStatus.Name.Equals("Активен", StringComparison.OrdinalIgnoreCase));

            var inactiveCount = devices.Count - activeCount;

            var efficiency = devices.Count != 0 ? (double)activeCount / devices.Count * 100 : 0;

            var money = await _context.Sales
                .Where(s => s.DeviceId.HasValue &&
                            devices
                                .Select(d => d.Id)
                                .Contains(s.DeviceId.Value))
                .Join(_context.Products, s => s.ProductId, p => p.Id, (s, p) => p.Price)
                .SumAsync();

            return Ok(new
            {
                Active = activeCount,
                Inactive = inactiveCount,
                Efficiency = Math.Round(efficiency, 2),
                Devices = devices,
                TotalMoney = money
            });
        }

        [HttpGet("summary")]
        [SwaggerOperation(
            Summary = "Сводка за сегодня/вчера",
            Description = "Выручка, инкассация, обслуживание, деньги в ТА и т.д.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Сводка получена", typeof(object))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        public async Task<IActionResult> GetSummaryAsync(
            [FromQuery][SwaggerParameter(Description = "Дата для сводки (по умолчанию сегодня)")] DateTime? date = null)
        {
            var targetDate = date?.Date ?? DateTime.UtcNow.Date;
            var yesterday = targetDate.AddDays(-1);

            var sales = await _context.Sales
                .Include(s => s.Product)
                .Where(s => s.SaleDateTime == targetDate || s.SaleDateTime == yesterday)
                .ToListAsync();

            var revenueToday = sales
                .Where(s => s.SaleDateTime == targetDate && s.Product != null)
                .Sum(s => s.Product.Price);
            var revenueYesterday = sales
                .Where(s => s.SaleDateTime == yesterday && s.Product != null)
                .Sum(s => s.Product.Price);

            var collectedToday = revenueToday;
            var collectedYesterday = revenueYesterday;

            var services = await _context.Services
                .Where(s => s.ServiceDate == DateOnly.FromDateTime(targetDate)
                || s.ServiceDate == DateOnly.FromDateTime(yesterday))
                .ToListAsync();

            var servicedToday = services.Count(s => s.ServiceDate == DateOnly.FromDateTime(targetDate));
            var servicedYesterday = services.Count(s => s.ServiceDate == DateOnly.FromDateTime(yesterday));

            var inventory = await _context.Inventories
                .Include(i => i.Product)
                .ToListAsync();

            var moneyInTA = inventory.Sum(i => i.Quantity * i.Product.Price);
            var changeInTA = inventory.Sum(i => i.Quantity * i.Product.Price * 0.1m);

            return Ok(new
            {
                MoneyInTA = moneyInTA,
                ChangeInTA = changeInTA,
                RevenueToday = revenueToday,
                RevenueYesterday = revenueYesterday,
                CollectedToday = collectedToday,
                CollectedYesterday = collectedYesterday,
                ServicedToday = servicedToday,
                ServicedYesterday = servicedYesterday
            });
        }

        [HttpGet("sales-trend")]
        [SwaggerOperation(
            Summary = "Тренд продаж за период",
            Description = "Можно получить по сумме или по количеству продаж")]
        [SwaggerResponse(StatusCodes.Status200OK, "Тренд продаж получен", typeof(List<SaleTrend>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        public async Task<IActionResult> GetSalesTrendAsync(
            [FromQuery][SwaggerParameter(Description = "Дата начала периода")] DateTime? startDate,
            [FromQuery][SwaggerParameter(Description = "Дата окончания периода")] DateTime? endDate,
            [FromQuery][SwaggerParameter(Description = "Группировка по сумме (true) или количеству (false)")] bool byAmount = true)
        {
            var start = DateTime.SpecifyKind(startDate?.Date ?? DateTime.UtcNow.Date.AddDays(-9), DateTimeKind.Utc);
            var end = DateTime.SpecifyKind(endDate?.Date ?? DateTime.UtcNow.Date, DateTimeKind.Utc);

            var query = _context.Sales
                .Include(s => s.Product)
                .Where(s => s.SaleDateTime >= start && s.SaleDateTime <= end)
                .GroupBy(s => s.SaleDateTime)
                .OrderBy(g => g.Key);

            var result = byAmount
                ? await query.Select(g => new SaleTrend
                {
                    Date = g.Key.ToString(),
                    Value = g.Sum(s => s.Product != null ? s.Product.Price : 0)
                }).ToListAsync()
                : await query.Select(g => new SaleTrend
                {
                    Date = g.Key.ToString(),
                    Value = g.Count()
                }).ToListAsync();

            return Ok(result);
        }

        [HttpGet("notifications")]
        [SwaggerOperation(Summary = "Уведомления системы")]
        [SwaggerResponse(StatusCodes.Status200OK, "Список уведомлений", typeof(object))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Требуется авторизация")]
        public async Task<IActionResult> GetNotificationsAsync(
            [FromQuery][SwaggerParameter(Description = "ID аппарата для фильтрации")] int? deviceId,
            [FromQuery][SwaggerParameter(Description = "ID приоритета для фильтрации")] int? priorityId,
            [FromQuery][SwaggerParameter(Description = "Количество уведомлений (по умолчанию 10)")] int limit = 10,
            [FromQuery][SwaggerParameter(Description = "Смещение для пагинации (по умолчанию 0)")] int offset = 0)
        {
            var query = _context.Notifications
                .Include(not => not.Device)
                .Include(not => not.User)
                .AsQueryable();

            if (deviceId.HasValue)
            {
                query = query.Where(not => not.DeviceId == deviceId.Value);
            }

            if (priorityId.HasValue)
            {
                query = query.Where(not => not.Priority == priorityId.Value);
            }

            var notifications = await query
                .OrderByDescending(not => not.DateTime)
                .Skip(offset)
                .Take(limit)
                .Select(not => new
                {
                    not.Id,
                    not.DeviceId,
                    not.UserId,
                    not.Type,
                    not.Message,
                    not.Priority,
                    not.DateTime,
                    not.Confirmed
                })
                .ToListAsync();

            return Ok(notifications);
        }
    }
}
