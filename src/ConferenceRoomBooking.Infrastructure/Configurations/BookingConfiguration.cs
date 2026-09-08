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
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.HasKey(b => b.Id);
            builder.Property(b => b.StartTime).IsRequired();
            builder.Property(b => b.EndTime).IsRequired();
            builder.Property(b => b.HallCost).IsRequired().HasPrecision(18, 2);
            builder.Property(b => b.ServicesCost).IsRequired().HasPrecision(18, 2);
            builder.Property(b => b.CustomerName).IsRequired().HasMaxLength(150);
            builder.Property(b => b.CustomerEmail).IsRequired().HasMaxLength(150);
            builder.Ignore(b => b.TotalPrice);
            builder.HasOne(b => b.Hall)
                   .WithMany(h => h.Bookings)
                   .HasForeignKey(b => b.HallId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
