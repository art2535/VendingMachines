using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VendingMachines.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class GenerateValuesController : ControllerBase
    {
        private readonly Random _random;

        public GenerateValuesController()
        {
            _random = new Random();
        }

        [HttpGet("money")]
        public IActionResult GetMoney()
        {
            var money = _random.Next(0, 10000);
            return Ok(new
            {
                amount = money,
                currency = "RUB"
            });
        }

        [HttpGet("connection")]
        public IActionResult GetConnectionStatus()
        {
            var statuses = new[] { "Online", "Offline", "Unstable" };
            var selected = statuses[_random.Next(statuses.Length)];

            return Ok(new
            {
                status = selected,
                lastUpdate = DateTime.Now
            });
        }

        [HttpGet("stock")]
        public IActionResult GetStockAsync()
        {
            var stock = new
            {
                coffee = _random.Next(0, 100),
                sugar = _random.Next(0, 100),
                milk = _random.Next(0, 100),
                cups = _random.Next(0, 100),
                lids = _random.Next(0, 100),
                stirrers = _random.Next(0, 100)
            };

            return Ok(stock);
        }

        [HttpGet("cash")]
        public IActionResult GetCashStatus()
        {
            var cash = new
            {
                cashInBox = _random.Next(0, 5000),
                cashlessPayments = _random.Next(0, 10000),
                total = 0
            };

            var result = new
            {
                cash.cashInBox,
                cash.cashlessPayments,
                total = cash.cashInBox + cash.cashlessPayments
            };

            return Ok(result);
        }

        [HttpGet("statuses")]
        public IActionResult GetStatuses()
        {
            var statuses = new string[]
            {
                "Работает",
                "На обслуживании",
                "Ошибка: нет воды",
                "Ошибка: нет кофе",
                "Ошибка: замятие купюры",
                "Выключен"
            };

            var count = _random.Next(1, 3);
            var activeStatuses = statuses
                .OrderBy(_ => _random.Next())
                .Take(count)
                .ToArray();

            return Ok(new
            {
                statuses = activeStatuses,
                lastCheck = DateTime.Now
            });
        }
    }
}
