using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConferenceRoomBooking.Application.DTOs;
using ConferenceRoomBooking.Application.Interfaces;

namespace ConferenceRoomBooking.Application.Services;

public class ReportService : IReportService
{
    private readonly IHallRepository _hallRepository;
    private readonly IBookingRepository _bookingRepository;

    public ReportService(IHallRepository hallRepository, IBookingRepository bookingRepository)
    {
        _hallRepository = hallRepository;
        _bookingRepository = bookingRepository;
    }

    public async Task<IReadOnlyList<HallOccupancyReportDto>> GetOccupancyReportAsync(
        DateRangeReportRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(request.FromUtc, request.ToUtc);

        var halls = await _hallRepository.GetAllActiveWithServicesAsync(cancellationToken);
        var totalDays = Math.Max(1, (request.ToUtc - request.FromUtc).TotalDays);
        // Припустимо, робочий день закладу — 15 годин (з 08:00 до 23:00)
        var totalPossibleOperatingHours = totalDays * 15.0;

        var result = new List<HallOccupancyReportDto>();

        foreach (var hall in halls)
        {
            // Отримуємо всі бронювання залу за проміжок
            var bookings = hall.Bookings
                .Where(b => b.StartTime < request.ToUtc && b.EndTime > request.FromUtc)
                .ToList();

            double totalHours = 0;
            foreach (var b in bookings)
            {
                // Враховуємо лише ту частину бронювання, що потрапляє у запитуваний інтервал
                var effectiveStart = b.StartTime < request.FromUtc ? request.FromUtc : b.StartTime;
                var effectiveEnd = b.EndTime > request.ToUtc ? request.ToUtc : b.EndTime;
                totalHours += (effectiveEnd - effectiveStart).TotalHours;
            }

            var percentage = totalPossibleOperatingHours > 0
                ? Math.Round((totalHours / totalPossibleOperatingHours) * 100.0, 2)
                : 0;

            result.Add(new HallOccupancyReportDto(
                hall.Id,
                hall.Name,
                bookings.Count,
                Math.Round(totalHours, 2),
                percentage > 100 ? 100 : percentage
            ));
        }

        return result.OrderByDescending(r => r.OccupancyPercentage).ToList();
    }

    public async Task<RevenueReportDto> GetRevenueReportAsync(
        DateRangeReportRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(request.FromUtc, request.ToUtc);

        var halls = await _hallRepository.GetAllActiveWithServicesAsync(cancellationToken);

        var hallRevenues = new List<HallRevenueDto>();
        decimal totalRent = 0;
        decimal totalServices = 0;
        int totalBookingsCount = 0;

        foreach (var hall in halls)
        {
            var bookings = hall.Bookings
                .Where(b => b.StartTime >= request.FromUtc && b.StartTime <= request.ToUtc)
                .ToList();

            var hallRent = bookings.Sum(b => b.HallCost);
            var hallServ = bookings.Sum(b => b.ServicesCost);
            var hallTotal = hallRent + hallServ;

            totalRent += hallRent;
            totalServices += hallServ;
            totalBookingsCount += bookings.Count;

            hallRevenues.Add(new HallRevenueDto(
                hall.Id,
                hall.Name,
                hallRent,
                hallServ,
                hallTotal
            ));
        }

        var grandTotal = totalRent + totalServices;
        var avgValue = totalBookingsCount > 0 ? Math.Round(grandTotal / totalBookingsCount, 2) : 0m;

        return new RevenueReportDto(
            request.FromUtc,
            request.ToUtc,
            grandTotal,
            totalRent,
            totalServices,
            totalBookingsCount,
            avgValue,
            hallRevenues.OrderByDescending(h => h.TotalRevenue).ToList()
        );
    }

    public async Task<IReadOnlyList<ServicePopularityReportDto>> GetPopularServicesReportAsync(
        DateRangeReportRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(request.FromUtc, request.ToUtc);

        var halls = await _hallRepository.GetAllActiveWithServicesAsync(cancellationToken);

        var allServices = halls
            .SelectMany(h => h.AvailableServices)
            .Select(hs => hs.Service)
            .DistinctBy(s => s.Id)
            .ToList();

        var bookings = halls
            .SelectMany(h => h.Bookings)
            .Where(b => b.StartTime >= request.FromUtc && b.StartTime <= request.ToUtc)
            .ToList();

        var result = new List<ServicePopularityReportDto>();

        foreach (var service in allServices)
        {
            // Рахуємо скільки разів послуга була обрана в бронюваннях
            var bookedCount = bookings
                .SelectMany(b => b.BookingServices)
                .Count(bs => bs.ServiceId == service.Id);

            result.Add(new ServicePopularityReportDto(
                service.Id,
                service.Name,
                bookedCount,
                bookedCount * service.Price
            ));
        }

        return result.OrderByDescending(s => s.TimesBooked).ThenByDescending(s => s.TotalRevenueGenerated).ToList();
    }

    private static void ValidateDateRange(DateTime from, DateTime to)
    {
        if (from >= to)
            throw new ArgumentException("Дата початку вибірки повинна бути меншою за дату закінчення.");
    }
}