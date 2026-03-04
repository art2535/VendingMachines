# Многофункциональная система управления торговыми автоматами  
**Backend API + Web + Mobile + Desktop**  
.NET - PostgreSQL - Clean Architecture

## О проекте

**VendingMachines** — полноценная экосистема для владельцев, операторов и администраторов вендинговых автоматов.

Система позволяет:
- Управлять торговыми автоматами (регистрация, мониторинг, импорт)
- Вести учёт товаров и остатков
- Регистрировать продажи и транзакции
- Отслеживать события и состояние устройств в реальном времени
- Работать с компаниями, договорами, пользователями и бронированиями
- Использовать удобные интерфейсы: веб, мобильное приложение, десктоп

## Технологии

- **Backend** — ASP.NET Core Web API (.NET 9)
- **Архитектура** — Clean Architecture
- **База данных** — PostgreSQL
- **Фронтенд / клиенты**:
  - Web — ASP.NET Core MVC / Razor Pages
  - Mobile — .NET MAUI (или Xamarin)
  - Desktop — WPF / WinForms
  - Console — .NET Console App
- **Тестирование** — xUnit (юнит-тесты API)
- **Аутентификация** — JWT
- **Языки** — C#, HTML (Razor), PL/pgSQL

## Структура решения

```
VendingMachines/
├── DataBase/                    → SQL-скрипты, схема БД
├── VendingMachines.API/         → REST API (контроллеры, DTO)
├── VendingMachines.API.Tests/   → Юнит-тесты API
├── VendingMachines.Core/        → Доменные модели, сервисы
├── VendingMachines.Infrastructure/ → Репозитории, DbContext
├── VendingMachines.Web/         → Веб-приложение (Razor Pages / MVC)
├── VendingMachines.Mobile/      → Мобильное приложение
├── VendingMachines.Desktop/     → Десктопное приложение
├── VendingMachines.Console/     → Консольная утилита
├── VendingMachines.sln
└── .gitignore
```

## Как запустить

### 1. Клонирование

```bash
git clone https://github.com/art2535/VendingMachines.git
cd VendingMachines
```

### 2. Настройка базы данных

1. Создайте базу PostgreSQL: `VendingMachines`
2. Выполните скрипты из `DataBase/` (порядок важен):

   ```bash
   CreateTables-En.sql
   InsertData.sql
   ```

3. Укажите строку подключения в:

   `VendingMachines.API/appsettings.Development.json`

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=VendingMachines;Username=postgres;Password=your_password"
     }
   }
   ```

### 3. Запуск API (основной бэкенд)

```bash
cd VendingMachines.API
dotnet restore
dotnet run
```

→ https://localhost:5000  
→ Swagger: https://localhost:5000/swagger

### 4. Запуск веб-интерфейса

```bash
cd VendingMachines.Web
dotnet run
```

→ обычно https://localhost:7000 или другой порт

### 5. Мобильное / десктоп / консоль

Откройте соответствующий проект в Visual Studio → выберите конфигурацию → запустите.

## Основные API-эндпоинты

| Группа              | Базовый путь          | Назначение                              |
|---------------------|-----------------------|-----------------------------------------|
| Аутентификация      | `/api/auth`           | Вход, регистрация, refresh токены      |
| Автоматы            | `/api/devices`        | CRUD торговых автоматов                |
| Импорт автоматов    | `/api/device-import`  | Массовый импорт устройств              |
| Товары              | `/api/products`       | Ассортимент и остатки                  |
| Продажи             | `/api/sales`          | Регистрация и история продаж           |
| Мониторинг          | `/api/monitoring`     | Состояние, температура, ошибки         |
| События             | `/api/events`         | Логи и история событий                 |
| Компании            | `/api/companies`      | Владельцы / организации                |
| Договоры            | `/api/contracts`      | Договоры на обслуживание               |
| Бронирования        | `/api/bookings`       | Заказы / брони (если применимо)        |
| Тестовые данные     | `/api/generate-values`| Генерация демо-данных (для разработки) |

Полный список с примерами запросов → в **Swagger** после запуска API.

## Текущий статус

- Полноценная многослойная архитектура
- Реализованы основные контроллеры API
- Добавлены юнит-тесты
- Поддержка нескольких платформ (API + Web + Mobile + Desktop)
- Последние исправления: контроллер устройств и веб-страница

## Планы развития

- Полная JWT-аутентификация + роли
- Глобальная обработка ошибок и валидация
- Расширенные отчёты и дашборды
- Docker / docker-compose
- Интеграционные тесты
- Улучшенная документация Swagger
- Push-уведомления для мобильного приложения

## Автор

**Артём Петров**  
Учебный проект — 2025–2026
