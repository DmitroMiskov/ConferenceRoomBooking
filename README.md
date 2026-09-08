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
```text
## Початкові дані (Database Seeding)
