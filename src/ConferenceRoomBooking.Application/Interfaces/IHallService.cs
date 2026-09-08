using ConferenceRoomBooking.Application.DTOs;

namespace ConferenceRoomBooking.Application.Interfaces;

public interface IHallService
{
    Task<HallCreatedResponse> CreateHallAsync(CreateHallRequestDto request, CancellationToken cancellationToken = default);
    Task UpdateHallAsync(Guid id, UpdateHallRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteHallAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<HallDto>> SearchAvailableHallsAsync(SearchAvailableHallsRequestDto request, CancellationToken cancellationToken = default);
}