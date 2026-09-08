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
    public class BookingServiceConfiguration : IEntityTypeConfiguration<BookingService>
    {
        public void Configure(EntityTypeBuilder<BookingService> builder)
        {
            builder.HasKey(bs => new { bs.BookingId, bs.ServiceId });
            builder.Property(bs => bs.PriceAtBooking).IsRequired().HasPrecision(18, 2);
            builder.HasOne(bs => bs.Booking)
                   .WithMany(b => b.BookingServices)
                   .HasForeignKey(bs => bs.BookingId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(bs => bs.Service)
                   .WithMany()
                   .HasForeignKey(bs => bs.ServiceId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
