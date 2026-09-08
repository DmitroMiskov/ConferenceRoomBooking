using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConferenceRoomBooking.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConferenceRoomBooking.Infrastructure.Configurations
{
    public class ServiceConfiguration : IEntityTypeConfiguration<Service>
    {
        public static readonly Guid ProjectorId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        public static readonly Guid WifiId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        public static readonly Guid SoundSystemId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        public void Configure(EntityTypeBuilder<Service> builder)
        {
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Name).IsRequired().HasMaxLength(100);
            builder.Property(s => s.Price).IsRequired().HasPrecision(18, 2);
            builder.HasData(
                new { Id = ProjectorId, Name = "Projector", Price = 200m },
                new { Id = WifiId, Name = "Wi-Fi", Price = 100m },
                new { Id = SoundSystemId, Name = "Sound System", Price = 300m }
            );
        }
    }
}
