using ConferenceRoomBooking.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConferenceRoomBooking.Infrastructure.Configurations
{
    public class HallConfiguration : IEntityTypeConfiguration<Hall>
    {
        public static readonly Guid HallAId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        public static readonly Guid HallBId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        public static readonly Guid HallCId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        public void Configure(EntityTypeBuilder<Hall> builder)
        {
            builder.HasKey(h => h.Id);
            builder.Property(h => h.Name).IsRequired().HasMaxLength(100);
            builder.Property(h => h.Capacity).IsRequired();
            builder.Property(h => h.BaseHourlyRate).IsRequired().HasPrecision(18, 2);
            builder.HasMany(h => h.AvailableServices)
                   .WithOne(hs => hs.Hall)
                   .HasForeignKey(hs => hs.HallId)
                   .OnDelete(DeleteBehavior.Restrict);
            builder.HasQueryFilter(h => !h.IsDeleted);
            builder.HasData(
                new { Id = HallAId, Name = "Зал A", Capacity = 50, BaseHourlyRate = 500m, IsDeleted = false },
                new { Id = HallBId, Name = "Зал B", Capacity = 100, BaseHourlyRate = 900m, IsDeleted = false },
                new { Id = HallCId, Name = "Зал C", Capacity = 20, BaseHourlyRate = 300m, IsDeleted = false }
            );
        }
    }
}
