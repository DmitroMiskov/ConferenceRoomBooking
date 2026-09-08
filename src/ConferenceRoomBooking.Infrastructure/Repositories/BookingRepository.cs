using ConferenceRoomBooking.Application.Interfaces;
using ConferenceRoomBooking.Domain;
using ConferenceRoomBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace ConferenceRoomBooking.Infrastructure.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly ApplicationDbContext _context;

        public BookingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> HasOverlappingBookingAsync(Guid hallId, DateTime start, DateTime end, CancellationToken cancellationToken = default)
        {
            // Перетин умов: (StartA < EndB) та (EndA > StartB)
            return await _context.Bookings
                .AnyAsync(b => b.HallId == hallId && b.StartTime < end && b.EndTime > start, cancellationToken);
        }

        public async Task AddAsync(Booking booking, CancellationToken cancellationToken = default)
        {
            await _context.Bookings.AddAsync(booking, cancellationToken);
        }

        public async Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Bookings
                .Include(b => b.BookingServices)
                .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        }
    }
}
