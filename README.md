# Conference Room Booking API

RESTful API для автоматизації процесів управління конференц-залами, бронювання приміщень із динамічним ціноутворенням та аналітичної звітності для бізнесу. Проєкт побудований на платформі **.NET 8** за принципами **Clean Architecture** та **Domain-Driven Design (DDD)** з використанням **PostgreSQL** та **Entity Framework Core**.

---

## 🎯 Бізнес-контекст та завдання

Система вирішує завдання оптимізації завантаження комерційної нерухомості та автоматизації розрахунків:

1. **Управління фондом залів та сервісів**:
   * Облік залів із різною місткістю, базовою ціною та додатковим обладнанням (проєктор, звук, Wi-Fi).
   * Можливість динамічно прив'язувати нові послуги до залів без порушення зв'язків у базі даних.
   * М'яке видалення залів (`Soft Delete`) для збереження цілісності історичних фінансових звітів.

2. **Захист від подвійних бронювань (Overlap Prevention)**:
   * Автоматична перевірка перетину інтервалів часу на рівні збереження даних: $StartTime < BookingEnd \land EndTime > BookingStart$.

3. **Динамічне сегментне ціноутворення**:
   * Погодинний розрахунок оренди залежно від часу доби:
     * **06:00 – 09:00 (Ранкові години):** знижка **10%** (множник `0.9`)
     * **09:00 – 12:00, 14:00 – 18:00 (Стандартні години):** базова ставка (множник `1.0`)
     * **12:00 – 14:00 (Пікові години):** націнка **15%** (множник `1.15`)
     * **18:00 – 23:00 (Вечірні години):** знижка **20%** (множник `0.8`)
   * Додаткові послуги підсумовуються як фіксована разова вартість.

4. **Звітність та бізнес-аналітика**:
   * Моніторинг завантаженості залів (коефіцієнт Occupancy Rate у % та годинах).
   * Фінансовий звіт: сукупний дохід, розподіл (оренда vs додаткові послуги), середній чек.
   * Рейтинг популярності послуг для оцінки доцільності інвестицій у нове обладнання.

---

## 🏛 Архітектура рішення

Проєкт структурований відповідно до Clean Architecture з чітким розділенням зон відповідальності:

```text
ConferenceRoomBooking/
├── src/
│   ├── ConferenceRoomBooking.Domain/           # Доменні сутності, інваріанти, правила розрахунку
│   │   ├── Entities/                           # Hall, Booking, Service, HallService, BookingService
│   │   └── Services/                           # PricingCalculator
│   │
│   ├── ConferenceRoomBooking.Application/      # Сценарії використання (Use Cases), DTO, контракти
│   │   ├── DTOs/                               # Request/Response контракти
│   │   ├── Interfaces/                         # IHallRepository, IBookingRepository, IUnitOfWork, IReportService
│   │   └── Services/                           # HallService, BookingService, ReportService
│   │
│   ├── ConferenceRoomBooking.Infrastructure/   # Доступ до даних та зовнішні залежності
│   │   ├── Persistence/                        # ApplicationDbContext, конфігурації зв'язків
│   │   ├── Repositories/                       # Реалізації IHallRepository, IBookingRepository, UnitOfWork
│   │   └── Migrations/                         # Міграції EF Core та початковий Seeding
│   │
│   └── ConferenceRoomBooking.Api/              # ASP.NET Core Web API
│       ├── Controllers/                        # HallsController, BookingsController, ReportsController
│       ├── Middleware/                         # GlobalExceptionHandler (RFC 7807 Problem Details)
│       └── Program.cs                          # Конфігурація DI-контейнера та пайплайну
└── README.md
```
---
## Початкові дані (Database Seeding)
Під час першого застосування міграцій база даних автоматично заповнюється початковими сутностями:
### Зали
| ID | Назва | Місткість | Базова ставка (грн/год) |
| :--- | :--- | :--- | :--- |
| `aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa` | Зал A | 50 осіб | 2000 |
| `bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb` | Зал B | 100 осіб | 3500 |
| `cccccccc-cccc-cccc-cccc-cccccccccccc` | Зал C | 30 осіб | 1500 |

### Додаткові послуги
| ID | Назва | Вартість (грн) |
| :--- | :--- | :--- |
| `11111111-1111-1111-1111-111111111111` | Проєктор | 500 |
| `22222222-2222-2222-2222-222222222222` | Wi-Fi | 300 |
| `33333333-3333-3333-3333-333333333333` | Звук | 700 |

---

## 🚀 Встановлення та запуск

### Вимоги
* .NET 8.0 SDK
* PostgreSQL 14+

### Інструкція з розгортання

1. **Клонуйте репозиторій:**
   ```bash
   git clone [https://github.com/](https://github.com/)<your-username>/ConferenceRoomBooking.git
   cd ConferenceRoomBooking
   ```
2. **Налаштуйте з'єднання з базою даних:**
   У файлі src/ConferenceRoomBooking.Api/appsettings.json відредагуйте параметри доступу до вашої PostgreSQL:
   ```JSON
   "ConnectionStrings": {
      "DefaultConnection": "Host=localhost;Port=5432;Database=ConferenceRoomBookingDb;Username=postgres;Password=your_password"
   }
   ```
3. **Застосуйте міграції:**
   ```bash
   dotnet ef database update --project src/ConferenceRoomBooking.Infrastructure --startup-project src/ConferenceRoomBooking.Api
   ```
4. **Запустіть бекенд:**
   ```bash
   dotnet run --project src/ConferenceRoomBooking.Api
   ```
5. **Тестування API:**
  Відкрийте в браузері Swagger UI: http://localhost:5290/swagger (або порт, вказаний у терміналі під час старту).

---

## 🔌 Огляд API Endpoints

1. **Зали (/api/Halls)**
   * GET /api/Halls/search?startTime=...&endTime=...&requiredCapacity=... — пошук доступних залів під критерії часу та кількості учасників (зайняті зали автоматично виключаються).
   * POST /api/Halls — створення нового залу.
   * PUT /api/Halls/{id} — оновлення параметрів залу та підключення нових послуг.
   * DELETE /api/Halls/{id} — м'яке видалення залу (встановлення прапорця IsDeleted = true).
2. **Бронювання (/api/Bookings)**
   * POST /api/Bookings — створення бронювання з валідацією слотів та розрахунком ціни за формулою тарифних зон.
   
   Приклад тіла запиту на бронювання:
   ```JSON
   {
     "hallId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
     "customerName": "Олександр",
     "customerEmail": "alex@example.com",
     "startTime": "2026-10-01T11:00:00Z",
     "endTime": "2026-10-01T14:00:00Z",
     "selectedServiceIds": [
       "11111111-1111-1111-1111-111111111111"
      ]
   }
   ```
3. **Аналітика та звіти (/api/Reports)**
   * GET /api/Reports/occupancy?fromUtc=...&toUtc=... — процентний коефіцієнт та кількість заброньованих годин залів.
   * GET /api/Reports/revenue?fromUtc=...&toUtc=... — фінансовий звіт: виручка з оренди, виручка з послуг, середній чек.
   * GET /api/Reports/popular-services?fromUtc=...&toUtc=... — аналітика замовлення супутніх послуг.
---

## 🛠 Технологічний стек
* Платформа: C# 12 / .NET 8
* База даних: PostgreSQL
* ORM: Entity Framework Core (Npgsql)
* Документація: Swagger / OpenAPI
* Стандарти помилок: RFC 7807 (Problem Details)
* Підхід до архітектури: Clean Architecture, Domain-Driven Design, Repository & Unit of Work Patterns
