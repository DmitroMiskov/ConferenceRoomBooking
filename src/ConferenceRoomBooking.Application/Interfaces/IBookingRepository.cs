using ConferenceRoomBooking.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConferenceRoomBooking.Application.Interfaces
{
    public interface IBookingRepository
    {
        Task<bool> HasOverlappingBookingAsync(Guid hallId, DateTime start, DateTime end, CancellationToken cancellationToken = default);
        Task AddAsync(Booking booking, CancellationToken cancellationToken = default);
        Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
