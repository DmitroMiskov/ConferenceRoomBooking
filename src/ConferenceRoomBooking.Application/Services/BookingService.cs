using ConferenceRoomBooking.Application.DTOs;
using ConferenceRoomBooking.Application.Interfaces;
using ConferenceRoomBooking.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConferenceRoomBooking.Application.Services;

public class BookingService : IBookingService
{
    private readonly IHallRepository _hallRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BookingService(
        IHallRepository hallRepository,
        IBookingRepository bookingRepository,
        IUnitOfWork unitOfWork)
    {
        _hallRepository = hallRepository;
        _bookingRepository = bookingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BookingResponse> CreateBookingAsync(CreateBookingRequestDto request, CancellationToken cancellationToken = default)
    {
        // 1. Валідація часових меж
        if (request.StartTime >= request.EndTime)
        {
            throw new InvalidOperationException("Час початку має бути меншим за час закінчення.");
        }

        if (request.StartTime < DateTime.UtcNow)
        {
            throw new InvalidOperationException("Неможливо забронювати зал у минулому часі.");
        }

        // 2. Перевірка існування залу та підтягування доступних сервісів
        var hall = await _hallRepository.GetWithServicesByIdAsync(request.HallId, cancellationToken);
        if (hall == null)
        {
            throw new KeyNotFoundException($"Зал з ідентифікатором {request.HallId} не знайдено.");
        }

        // 3. Перевірка перетину часових слотів
        var hasOverlap = await _bookingRepository.HasOverlappingBookingAsync(
            request.HallId,
            request.StartTime,
            request.EndTime,
            cancellationToken);

        if (hasOverlap)
        {
            throw new InvalidOperationException("Обраний часовий слот для цього залу вже зайнятий.");
        }

        // 4. Перевірка та відбір послуг, прив'язаних до цього залу
        var requestedServiceIds = request.SelectedServiceIds?.Distinct().ToList() ?? new List<Guid>();
        var availableHallServices = hall.AvailableServices
            .Where(hs => requestedServiceIds.Contains(hs.ServiceId))
            .Select(hs => hs.Service)
            .ToList();

        if (requestedServiceIds.Count != availableHallServices.Count)
        {
            throw new InvalidOperationException("Одна або декілька обраних послуг недоступні для даного залу.");
        }

        // 5. Розрахунок вартості з урахуванням тарифних зон
        var hallCost = CalculateHallCost(hall.BaseHourlyRate, request.StartTime, request.EndTime);
        var servicesCost = availableHallServices.Sum(s => s.Price);
        var totalPrice = hallCost + servicesCost;

        // 6. Створення сутності бронювання
        var booking = new Booking(
            hall.Id,
            request.CustomerName,
            request.CustomerEmail,
            request.StartTime,
            request.EndTime,
            hallCost,
            servicesCost);

        foreach (var service in availableHallServices)
        {
            booking.AddService(service.Id, service.Price);
        }

        await _bookingRepository.AddAsync(booking, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new BookingResponse(
            booking.Id,
            hall.Id,
            hall.Name,
            booking.CustomerName,
            booking.CustomerEmail,
            booking.StartTime,
            booking.EndTime,
            booking.HallCost,
            booking.ServicesCost,
            booking.TotalPrice,
            availableHallServices.Select(s => s.Name).ToList()
        );
    }

    public async Task<IReadOnlyList<HallDto>> GetAvailableHallsAsync(CancellationToken cancellationToken = default)
    {
        var halls = await _hallRepository.GetAllActiveWithServicesAsync(cancellationToken);

        return halls.Select(h => new HallDto(
            h.Id,
            h.Name,
            h.Capacity,
            h.BaseHourlyRate,
            h.AvailableServices.Select(hs => new ServiceDto(
                hs.Service.Id,
                hs.Service.Name,
                hs.Service.Price
            )).ToList()
        )).ToList();
    }

    /// <summary>
    /// Розрахунок вартості залу з урахуванням погодинних знижок і націнок:
    /// - 06:00 - 09:00: знижка 10% (коефіцієнт 0.9)
    /// - 12:00 - 14:00: пікові години, націнка 15% (коефіцієнт 1.15)
    /// - 18:00 - 23:00: вечірні години, знижка 20% (коефіцієнт 0.8)
    /// - Інші години: базова ставка (коефіцієнт 1.0)
    /// </summary>
    private static decimal CalculateHallCost(decimal baseHourlyRate, DateTime start, DateTime end)
    {
        decimal total = 0m;
        var current = start;

        while (current < end)
        {
            var nextHour = current.Date.AddHours(current.Hour + 1);
            var segmentEnd = nextHour > end ? end : nextHour;
            var durationInHours = (decimal)(segmentEnd - current).TotalHours;

            var hour = current.Hour;
            decimal multiplier = 1.0m;

            if (hour >= 6 && hour < 9)
            {
                multiplier = 0.9m;  // -10%
            }
            else if (hour >= 12 && hour < 14)
            {
                multiplier = 1.15m; // +15%
            }
            else if (hour >= 18 && hour < 23)
            {
                multiplier = 0.8m;  // -20%
            }
            else
            {
                multiplier = 1.0m;  // Стандартні години
            }

            total += baseHourlyRate * multiplier * durationInHours;
            current = segmentEnd;
        }

        return Math.Round(total, 2);
    }
}