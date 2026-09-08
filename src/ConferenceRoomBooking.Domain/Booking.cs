using System;
using System.Collections.Generic;

namespace ConferenceRoomBooking.Domain
{
    public class Booking
    {
        public Guid Id { get; private set; }
        public Guid HallId { get; private set; }
        public Hall Hall { get; private set; } = null!;
        public string CustomerName { get; private set; } = null!;
        public string CustomerEmail { get; private set; } = null!;
        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }
        public decimal HallCost { get; private set; }
        public decimal ServicesCost { get; private set; }
        public decimal TotalPrice => HallCost + ServicesCost;
        public ICollection<BookingService> BookingServices { get; private set; } = new List<BookingService>();
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        private Booking() { }

        public Booking(
            Guid hallId,
            string customerName,
            string customerEmail,
            DateTime startTime,
            DateTime endTime,
            decimal hallCost,
            decimal servicesCost)
        {
            if (string.IsNullOrWhiteSpace(customerName))
            {
                throw new ArgumentException("Customer name cannot be empty.", nameof(customerName));
            }

            if (string.IsNullOrWhiteSpace(customerEmail))
            {
                throw new ArgumentException("Customer email cannot be empty.", nameof(customerEmail));
            }

            if (startTime >= endTime)
            {
                throw new ArgumentException("Start time must be before end time.");
            }

            if (hallCost < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(hallCost), "Hall cost cannot be negative.");
            }

            if (servicesCost < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(servicesCost), "Services cost cannot be negative.");
            }

            Id = Guid.NewGuid();
            HallId = hallId;
            CustomerName = customerName;
            CustomerEmail = customerEmail;
            StartTime = startTime;
            EndTime = endTime;
            HallCost = hallCost;
            ServicesCost = servicesCost;
        }

        public void AddService(Guid serviceId, decimal priceAtBooking)
        {
            var bookingService = new BookingService(Id, serviceId, priceAtBooking);
            BookingServices.Add(bookingService);
        }
    }
}