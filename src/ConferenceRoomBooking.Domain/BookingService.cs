using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConferenceRoomBooking.Domain
{
    public class BookingService
    {
        public Guid BookingId { get; private set; }
        public Guid ServiceId { get; private set; }
        public Booking Booking { get; private set; } = null!;
        public Service Service { get; private set; } = null!;
        public decimal PriceAtBooking { get; private set; }

        private BookingService() { }
        public BookingService(Guid bookingId, Guid serviceId, decimal priceAtBooking)
        {
            if (priceAtBooking < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(priceAtBooking), "Price at booking cannot be negative.");
            }
            BookingId = bookingId;
            ServiceId = serviceId;
            PriceAtBooking = priceAtBooking;
        }
    }
}
