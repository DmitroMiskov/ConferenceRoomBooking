using ConferenceRoomBooking.Application.Interfaces;
using ConferenceRoomBooking.Domain;
using ConferenceRoomBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ConferenceRoomBooking.Infrastructure.Repositories;

public class HallRepository : IHallRepository
{
    private readonly ApplicationDbContext _context;

    public HallRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Hall?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Halls.FindAsync(new object?[] { id }, cancellationToken);
    }

    public async Task<Hall?> GetWithServicesByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Halls
            .Include(h => h.AvailableServices)
            .ThenInclude(hs => hs.Service)
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Hall>> GetAllWithServicesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Halls
            .Include(h => h.AvailableServices)
            .ThenInclude(hs => hs.Service)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Hall>> GetAllActiveWithServicesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Halls
            .Include(h => h.AvailableServices)
            .ThenInclude(hs => hs.Service)
            .Where(h => !h.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Hall hall, CancellationToken cancellationToken = default)
    {
        await _context.Halls.AddAsync(hall, cancellationToken);
    }

    public void Update(Hall hall)
    {
        _context.Halls.Update(hall);
    }

    public void Delete(Hall hall)
    {
        _context.Halls.Remove(hall);
    }

    public async Task AddServiceAsync(Service service, CancellationToken cancellationToken = default)
    {
        await _context.Services.AddAsync(service, cancellationToken);
    }

    public async Task<IReadOnlyList<Hall>> GetAvailableHallsAsync(
        DateTime start,
        DateTime end,
        int capacity,
        CancellationToken cancellationToken = default)
    {
        // Знаходимо зали, які відповідають за місткістю і не мають перетинів із бронюваннями
        return await _context.Halls
            .Include(h => h.AvailableServices)
            .ThenInclude(hs => hs.Service)
            .Where(h => h.Capacity >= capacity)
            .Where(h => !_context.Bookings.Any(b =>
                b.HallId == h.Id &&
                start < b.EndTime &&
                end > b.StartTime))
            .ToListAsync(cancellationToken);
    }
}