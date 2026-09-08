using ConferenceRoomBooking.Domain;
using Microsoft.EntityFrameworkCore;

namespace ConferenceRoomBooking.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Hall> Halls => Set<Hall>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<HallService> HallServices => Set<HallService>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingService> BookingServices => Set<BookingService>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Конфігурація зв'язків
        modelBuilder.Entity<HallService>()
            .HasKey(hs => new { hs.HallId, hs.ServiceId });

        modelBuilder.Entity<BookingService>()
            .HasKey(bs => new { bs.BookingId, bs.ServiceId });

        // --- SEEDING ПОЧАТКОВИХ ДАНИХ ---

        // 1. Послуги
        var projectorId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var wifiId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var soundId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        modelBuilder.Entity<Service>().HasData(
            new { Id = projectorId, Name = "Проєктор", Price = 500m },
            new { Id = wifiId, Name = "Wi-Fi", Price = 300m },
            new { Id = soundId, Name = "Звук", Price = 700m }
        );

        // 2. Зали
        var hallAId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var hallBId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var hallCId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

        modelBuilder.Entity<Hall>().HasData(
            new { Id = hallAId, Name = "Зал A", Capacity = 50, BaseHourlyRate = 2000m, IsDeleted = false },
            new { Id = hallBId, Name = "Зал B", Capacity = 100, BaseHourlyRate = 3500m, IsDeleted = false },
            new { Id = hallCId, Name = "Зал C", Capacity = 30, BaseHourlyRate = 1500m, IsDeleted = false }
        );

        // 3. Доступні послуги для залів (прив'язка за замовчуванням)
        modelBuilder.Entity<HallService>().HasData(
            // Зал A має всі послуги
            new { HallId = hallAId, ServiceId = projectorId },
            new { HallId = hallAId, ServiceId = wifiId },
            new { HallId = hallAId, ServiceId = soundId },

            // Зал B має всі послуги
            new { HallId = hallBId, ServiceId = projectorId },
            new { HallId = hallBId, ServiceId = wifiId },
            new { HallId = hallBId, ServiceId = soundId },

            // Зал C
            new { HallId = hallCId, ServiceId = wifiId },
            new { HallId = hallCId, ServiceId = projectorId }
        );
    }
}