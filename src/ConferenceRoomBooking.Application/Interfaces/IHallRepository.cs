using ConferenceRoomBooking.Domain;

namespace ConferenceRoomBooking.Application.Interfaces;

public interface IHallRepository
{
    Task<Hall?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Hall?> GetWithServicesByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Hall>> GetAllWithServicesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Hall>> GetAllActiveWithServicesAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Hall hall, CancellationToken cancellationToken = default);
    void Update(Hall hall);
    Task AddServiceAsync(Service service, CancellationToken cancellationToken = default);
    void Delete(Hall hall);
    Task<IReadOnlyList<Hall>> GetAvailableHallsAsync(DateTime start, DateTime end, int capacity, CancellationToken cancellationToken = default);
}