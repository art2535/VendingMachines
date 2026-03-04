# Backend-часть системы управления торговыми автоматами  
**Clean Architecture - .NET - PostgreSQL - REST API**

## О проекте

Серверное API для учёта и управления вендинговыми (торговыми) автоматами. 

Основные сущности системы:
- Торговые автоматы
- Товары и их остатки
- Пользователи
- Продажи / транзакции
- Пополнение запасов

## Технологический стек

- **Backend** — .NET 9
- **Архитектура** — Clean Architecture
- **Слои проекта**:
  - **API** — контроллеры, DTO, конфигурация приложения
  - **Core** — доменные модели
  - **Infrastructure** — доступ к данным, репозитории, внешние сервисы
- **База данных** — PostgreSQL
- **ORM** — Entity Framework Core
- **Аутентификация** — JWT Bearer

## Структура проекта

```
VendingMachines/
├── DataBase/
│   ├── Compile Queries.sql
│   ├── CreateTables-En.sql
│   ├── DeleteTables.sql
│   ├── InsertData.sql
│   ├── VendingMachines.pgerd
│   └── Схема БД.jpg
├── VendingMachines.API/
│   ├── Controllers/
│   ├── DTOs/
│   ├── Extensions/
│   ├── Filters/
│   ├── Properties/
│   ├── Program.cs
│   ├── VendingMachines.API.csproj
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── VendingMachines.API.http
├── VendingMachines.Core/
│   └── Models/
├── VendingMachines.Infrastructure/
│   ├── Data/
│   └── Services/
├── VendingMachines.sln
└── .gitignore
```

## Как запустить локально

### 1. Клонирование и выбор ветки

```bash
git clone https://github.com/art2535/VendingMachines.git
cd VendingMachines
git checkout practica-api
```

### 2. Настройка подключения к базе данных

Откройте файл  
`VendingMachines.API/appsettings.Development.json` (или `appsettings.json`)

Добавьте/измените строку подключения:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=VendingMachines;Username=postgres;Password=ваш_пароль"
  }
}
```

### 3. Подготовка базы данных

Вариант А — использовать готовые SQL-скрипты:

1. Создайте пустую базу `VendingMachines` в PostgreSQL
2. Выполните по порядку скрипты из папки `DataBase/`:

   ```sql
   CreateTables-En.sql
   InsertData.sql
   ```

Вариант Б — если появятся EF Core миграции в будущем:

```bash
cd VendingMachines.API
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 4. Запуск API

```bash
cd VendingMachines.API
dotnet restore
dotnet run
```

API запускается обычно на:  
http://localhost:5000  или  https://localhost:5001  
(точный адрес и порты смотрите в консоли или в `Properties/launchSettings.json`)

Swagger UI → http://localhost:5000/swagger

## Эндпоинты

API построено по REST-принципам. Ниже приведён список основных контроллеров и их предполагаемое назначение (актуальный список HTTP-методов и путей смотрите непосредственно в коде контроллеров или в Swagger после запуска проекта).

| Контроллер                | Основное назначение                               | Базовый путь (пример)     |
|---------------------------|---------------------------------------------------|---------------------------|
| AuthController            | Аутентификация, вход, регистрация, refresh-токены | `/api/auth`               |
| BookingsController        | Управление бронированиями/заказами                | `/api/bookings`           |
| CompaniesController       | Управление компаниями/организациями-владельцами   | `/api/companies`          |
| ContractsController       | Работа с договорами                               | `/api/contracts`          |
| DevicesController         | Управление торговыми автоматами                   | `/api/devices`            |
| EventsController          | Логи событий, история действий автоматов          | `/api/events`             |
| GenerateValuesController  | Генерация тестовых/демо-значений                  | `/api/generate`           |
| MonitoringController      | Мониторинг состояния автоматов                    | `/api/monitoring`         |
| ProductsController        | Управление товарами и ассортиментом               | `/api/products`           |
| SalesController           | Регистрация продаж, история транзакций            | `/api/sales`              |
| UsersController           | Управление пользователями                         | `/api/users`              |

### Примеры типичных эндпоинтов

- `GET /api/devices`              — список всех торговых автоматов  
- `GET /api/devices/{id}`         — информация по конкретному автомату  
- `POST /api/devices`              — создание нового автомата  
- `PUT /api/devices/{id}`         — обновление автомата  
- `DELETE /api/devices/{id}`         — удаление автомата  

- `GET /api/products`             — список товаров  
- `POST /api/products`             — добавление товара в ассортимент  

- `POST /api/sales`                — регистрация продажи  
- `GET /api/sales?deviceId=123`   — продажи по конкретному автомату  

- `POST /api/auth/login`           — вход в систему  
- `POST /api/auth/refresh`         — обновление токена  

- `GET /api/monitoring/{deviceId}` — текущее состояние автомата  

Полный и актуальный список (с параметрами, моделями запроса/ответа, статус-кодами) доступен в **Swagger UI** после запуска проекта.
