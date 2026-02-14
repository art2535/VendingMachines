using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using VendingMachines.API.DTOs.Bookings.Enums;
using VendingMachines.API.DTOs.Events.Enums;
using VendingMachines.API.DTOs.Monitoring.Enums;

namespace VendingMachines.API.Filters
{
    public class EnumDescriptionSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (!context.Type.IsEnum)
                return;

            var lines = new List<string>();

            if (context.Type == typeof(OwnershipTypeEnum))
            {
                lines.Add("<strong>Значения:</strong>");
                lines.Add("0 — Аренда");
                lines.Add("1 — Лизинг");
                lines.Add("2 — Выкуп");
                lines.Add("3 — Тестовый период");
                lines.Add("4 — Прочее");
            }
            else if (context.Type == typeof(BookingStatusEnum))
            {
                lines.Add("<strong>Значения:</strong>");
                lines.Add("0 — Ожидает подтверждения");
                lines.Add("1 — Подтверждено");
                lines.Add("2 — Активно");
                lines.Add("3 — Завершено");
                lines.Add("4 — Отменено");
                lines.Add("5 — Отклонено");
            }
            else if (context.Type == typeof(EventTypeEnum))
            {
                lines.Add("<strong>Значения:</strong>");
                lines.Add("0 — Включение");
                lines.Add("1 — Ошибка");
                lines.Add("2 — Обновление");
                lines.Add("3 — Отключение");
                lines.Add("4 — Перезагрузка");
                lines.Add("5 — Калибровка");
            }
            else if (context.Type == typeof(DeviceStatusesEnum))
            {
                lines.Add("<strong>Значения:</strong>");
                lines.Add("0 — Активен");
                lines.Add("1 — Неактивен");
                lines.Add("2 — На обслуживании");
                lines.Add("3 — Списан");
                lines.Add("4 — Ожидает");
                lines.Add("5 — Ошибка");
                lines.Add("6 — Онлайн");
                lines.Add("7 — Оффлайн");
                lines.Add("8 — Тестирование");
                lines.Add("9 — Утилизирован");
            }

            if (lines.Count != 0)
            {
                var additional = string.Join("<br />", lines);
                schema.Description = (schema.Description ?? string.Empty)
                    .TrimEnd()
                    + (string.IsNullOrEmpty(schema.Description) ? "" : "<br /><br />")
                    + additional;
            }
        }
    }
}
