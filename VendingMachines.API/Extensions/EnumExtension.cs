using VendingMachines.API.DTOs.Bookings.Enums;

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

        public static BookingStatusEnum ToBookingStatusEnum(this string? dbValue) => dbValue switch
        {
            "Ожидает подтверждения" => BookingStatusEnum.Pending,
            "Подтверждено" => BookingStatusEnum.Confirmed,
            "Активно" => BookingStatusEnum.Active,
            "Завершено" => BookingStatusEnum.Completed,
            "Отменено" => BookingStatusEnum.Cancelled,
            "Отклонено" => BookingStatusEnum.Rejected,
            _ => BookingStatusEnum.Pending
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

        public static OwnershipTypeEnum ToOwnershipTypeEnum(this string dbValue) => dbValue switch
        {
            "Аренда" => OwnershipTypeEnum.Rent,
            "Лизинг" => OwnershipTypeEnum.Lease,
            "Выкуп" => OwnershipTypeEnum.Purchase,
            "Тестовый период" => OwnershipTypeEnum.Trial,
            "Прочее" => OwnershipTypeEnum.Other,
            _ => OwnershipTypeEnum.Other
        };
    }
}
