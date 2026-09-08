using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConferenceRoomBooking.Domain
{
    public class Hall
    {
        public Guid Id { get; init; }
        public string Name { get; private set; } = null!;
        public int Capacity { get; private set; }
        public decimal BaseHourlyRate { get; private set; }
        public bool IsDeleted { get; private set; }
        public ICollection<HallService> AvailableServices { get; private set; } = new List<HallService>();
        public ICollection<Booking> Bookings { get; private set; } = new List<Booking>();

        private Hall() { }

        public Hall(string name, int capacity, decimal baseHourlyRate)
        {
            Id = Guid.NewGuid();
            Name = name;
            Capacity = capacity;
            BaseHourlyRate = baseHourlyRate;
            IsDeleted = false;
        }

        public void MarkAsDeleted()
        {
            IsDeleted = true;
        }

        public void UpdateDetails(string name, int capacity, decimal baseHourlyRate)
        {
            if(string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name cannot be null or empty.", nameof(name));
            }
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than zero.");
            }
            if (baseHourlyRate < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(baseHourlyRate), "Base hourly rate cannot be negative.");
            }
            Name = name;
            Capacity = capacity;
            BaseHourlyRate = baseHourlyRate;
        }
    }
}
