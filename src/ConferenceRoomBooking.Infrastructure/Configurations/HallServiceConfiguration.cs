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
    public class HallServiceConfiguration : IEntityTypeConfiguration<HallService>
    {
        public void Configure(EntityTypeBuilder<HallService> builder)
        {
            builder.HasKey(hs => new { hs.HallId, hs.ServiceId });
            builder.HasOne(hs => hs.Hall)
                   .WithMany(h => h.AvailableServices)
                   .HasForeignKey(hs => hs.HallId)
                   .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(hs => hs.Service)
                   .WithMany()
                   .HasForeignKey(hs => hs.ServiceId)
                   .OnDelete(DeleteBehavior.Restrict);
            builder.HasData(
                // Зал A: Проєктор, Wi-Fi, Звукова система
                new { HallId = HallConfiguration.HallAId, ServiceId = ServiceConfiguration.ProjectorId },
                new { HallId = HallConfiguration.HallAId, ServiceId = ServiceConfiguration.WifiId },
                new { HallId = HallConfiguration.HallAId, ServiceId = ServiceConfiguration.SoundSystemId },

                // Зал B: Проєктор, Wi-Fi
                new { HallId = HallConfiguration.HallBId, ServiceId = ServiceConfiguration.ProjectorId },
                new { HallId = HallConfiguration.HallBId, ServiceId = ServiceConfiguration.WifiId },

                // Зал C: Wi-Fi
                new { HallId = HallConfiguration.HallCId, ServiceId = ServiceConfiguration.WifiId }
            );
        }
    }
}
