using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using VendingMachines.API.DTOs.Bookings.Enums;

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
                lines.Add("0 — Rent (Аренда)");
                lines.Add("1 — Lease (Лизинг)");
                lines.Add("2 — Purchase (Выкуп)");
                lines.Add("3 — Trial (Тестовый период)");
                lines.Add("4 — Other (Прочее)");
            }
            else if (context.Type == typeof(BookingStatusEnum))
            {
                lines.Add("<strong>Значения:</strong>");
                lines.Add("0 — Pending (Ожидает подтверждения)");
                lines.Add("1 — Confirmed (Подтверждено)");
                lines.Add("2 — Active (Активно)");
                lines.Add("3 — Completed (Завершено)");
                lines.Add("4 — Cancelled (Отменено)");
                lines.Add("5 — Rejected (Отклонено)");
            }

            if (lines.Any())
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
