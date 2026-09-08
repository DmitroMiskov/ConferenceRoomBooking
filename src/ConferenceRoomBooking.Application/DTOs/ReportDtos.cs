using System;
using System.Collections.Generic;

namespace ConferenceRoomBooking.Application.DTOs;

// Фільтр за датами для звітів
public record DateRangeReportRequestDto(DateTime FromUtc, DateTime ToUtc);

// 1. Звіт завантаженості залів
public record HallOccupancyReportDto(
    Guid HallId,
    string HallName,
    int TotalBookings,
    double TotalBookedHours,
    double OccupancyPercentage // Відсоток від загального фонду робочого часу
);

// 2. Звіт за доходами (Оренда + Додаткові послуги)
public record RevenueReportDto(
    DateTime FromUtc,
    DateTime ToUtc,
    decimal TotalRevenue,
    decimal TotalHallRentRevenue,
    decimal TotalServicesRevenue,
    int TotalBookingsCount,
    decimal AverageBookingValue,
    IReadOnlyList<HallRevenueDto> ByHall
);

public record HallRevenueDto(
    Guid HallId,
    string HallName,
    decimal HallRevenue,
    decimal ServicesRevenue,
    decimal TotalRevenue
);

// 3. Звіт популярності додаткових послуг
public record ServicePopularityReportDto(
    Guid ServiceId,
    string ServiceName,
    int TimesBooked,
    decimal TotalRevenueGenerated
);