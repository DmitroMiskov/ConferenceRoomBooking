using ConferenceRoomBooking.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConferenceRoomBooking.Application.Interfaces
{
    public interface IBookingService
    {
        Task<BookingResponse> CreateBookingAsync(CreateBookingRequestDto request, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<HallDto>> GetAvailableHallsAsync(CancellationToken cancellationToken = default);
    }
}
