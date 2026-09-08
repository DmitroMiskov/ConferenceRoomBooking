using System;
using System.Collections.Generic;

namespace ConferenceRoomBooking.Application.DTOs;

// --- DTOs для Бронювань ---
public record CreateBookingRequestDto(
    Guid HallId,
    string CustomerName,
    string CustomerEmail,
    DateTime StartTime,
    DateTime EndTime,
    List<Guid> SelectedServiceIds
);

public record BookingResponse(
    Guid Id,
    Guid HallId,
    string HallName,
    string CustomerName,
    string CustomerEmail,
    DateTime StartTime,
    DateTime EndTime,
    decimal HallCost,
    decimal ServicesCost,
    decimal TotalPrice,
    List<string> ServiceNames
);

// --- DTOs для Залів та Послуг ---
public record HallDto(
    Guid Id,
    string Name,
    int Capacity,
    decimal BaseHourRate,
    List<ServiceDto> AvailableServices
);

public record ServiceDto(
    Guid Id,
    string Name,
    decimal Price
);

public record CreateServiceItemDto(
    string Name,
    decimal Price
);

public record CreateHallRequestDto(
    string Name,
    int Capacity,
    decimal BaseHourRate,
    List<CreateServiceItemDto> Services
);

public record UpdateHallRequestDto(
    string Name,
    int Capacity,
    decimal BaseHourRate,
    List<CreateServiceItemDto>? NewServices
);

public record SearchAvailableHallsRequestDto(
    DateTime StartTime,
    DateTime EndTime,
    int RequiredCapacity
);

public record HallCreatedResponse(
    Guid Id,
    string Name
);