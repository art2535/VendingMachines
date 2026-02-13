using VendingMachines.API.DTOs.Bookings.Enums;
using VendingMachines.API.DTOs.Events.Enums;

namespace VendingMachines.API.Extensions
{
    public static class BookingEnumExtensions
    {
        public static string ToRussianDb(this BookingStatusEnum status) => status switch
        {
            BookingStatusEnum.Pending => "Ожидает подтверждения",
            BookingStatusEnum.Confirmed => "Подтверждено",
            BookingStatusEnum.Active => "Активно",
            BookingStatusEnum.Completed => "Завершено",
            BookingStatusEnum.Cancelled => "Отменено",
            BookingStatusEnum.Rejected => "Отклонено",
            _ => status.ToString()
        };

        public static string ToRussianDb(this OwnershipTypeEnum type) => type switch
        {
            OwnershipTypeEnum.Rent => "Аренда",
            OwnershipTypeEnum.Lease => "Лизинг",
            OwnershipTypeEnum.Purchase => "Выкуп",
            OwnershipTypeEnum.Trial => "Тестовый период",
            OwnershipTypeEnum.Other => "Прочее",
            _ => type.ToString()
        };

        public static string ToRussianDb(this EventTypeEnum type) => type switch
        {
            EventTypeEnum.Enabling => "Включение",
            EventTypeEnum.Error => "Ошибка",
            EventTypeEnum.Updating => "Обновление",
            EventTypeEnum.Disabling => "Отключение",
            EventTypeEnum.Rebooting => "Перезагрузка",
            EventTypeEnum.Calibration => "Калибровка",
            _ => type.ToString()
        };
    }
}
