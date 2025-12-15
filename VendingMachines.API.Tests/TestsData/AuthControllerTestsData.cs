using VendingMachines.API.DTOs.Auth;

namespace VendingMachines.API.Tests.TestsData;

public static class AuthControllerTestsData
{
    public static IEnumerable<object[]> GetValidRegisterRequests()
    {
        yield return new object[]
        {
            new RegisterRequest
            {
                LastName = "Иванов",
                FirstName = "Иван",
                MiddleName = null,
                Email = "ivanov@test.com",
                Phone = null,
                RoleId = null,
                CompanyId = null,
                Language = null,
                Password = "123456",
                RepeatPassword = "123456"
            }
        };

        yield return new object[]
        {
            new RegisterRequest
            {
                LastName = "Петров",
                FirstName = "Пётр",
                MiddleName = "Петрович",
                Email = "petr@example.com",
                Phone = "+79991234567",
                RoleId = 2,
                CompanyId = 5,
                Language = "en",
                Password = "securePass123!",
                RepeatPassword = "securePass123!"
            }
        };

        yield return new object[]
        {
            new RegisterRequest
            {
                LastName = "Сидорова",
                FirstName = "Мария",
                MiddleName = "Александровна",
                Email = "maria.sidorova@mail.ru",
                Phone = null,
                RoleId = null,
                CompanyId = null,
                Language = "ru",
                Password = "MyPass2025",
                RepeatPassword = "MyPass2025"
            }
        };
    }
    
    public static IEnumerable<object[]> GetInvalidRegisterRequests()
    {
        yield return new object[]
        {
            new RegisterRequest
            {
                LastName = "", 
                FirstName = "Иван", 
                Email = "test@test.com", 
                Password = "123456",
                RepeatPassword = "123456"
            },
            "Фамилия обязательна"
        };

        yield return new object[]
        {
            new RegisterRequest 
            { 
                LastName = "Иванов", 
                FirstName = "", 
                Email = "test@test.com", 
                Password = "123456", 
                RepeatPassword = "123456" 
            },
            "Имя обязательно"
        };

        yield return new object[]
        {
            new RegisterRequest
            {
                LastName = "Иванов", 
                FirstName = "Иван", 
                Email = "", 
                Password = "123456", 
                RepeatPassword = "123456" 
            },
            "Email обязателен"
        };

        yield return new object[]
        {
            new RegisterRequest
            {
                LastName = "Иванов", 
                FirstName = "Иван", 
                Email = "не-email", 
                Password = "123456", 
                RepeatPassword = "123456"
            },
            "Неверный формат email"
        };

        yield return new object[]
        {
            new RegisterRequest
            {
                LastName = "Иванов", 
                FirstName = "Иван", 
                Email = "test@test.com", 
                Password = "123456", 
                RepeatPassword = "654321"
            },
            "Пароли не совпадают"
        };

        yield return new object[]
        {
            new RegisterRequest
            {
                LastName = "Иванов", 
                FirstName = "Иван", 
                Email = "test@test.com", 
                Password = "123", 
                RepeatPassword = "123"
            },
            "Пароль должен содержать минимум 6 символов"
        };
    }
}