using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ConferenceRoomBooking.Application.DTOs;

namespace ConferenceRoomBooking.Application.Interfaces;

public interface IReportService
{
    Task<IReadOnlyList<HallOccupancyReportDto>> GetOccupancyReportAsync(
        DateRangeReportRequestDto request,
        CancellationToken cancellationToken = default);

    Task<RevenueReportDto> GetRevenueReportAsync(
        DateRangeReportRequestDto request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ServicePopularityReportDto>> GetPopularServicesReportAsync(
        DateRangeReportRequestDto request,
        CancellationToken cancellationToken = default);
}