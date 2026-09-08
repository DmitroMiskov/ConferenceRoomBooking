using ConferenceRoomBooking.Application.Interfaces;
using ConferenceRoomBooking.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceRoomBooking.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IHallService, HallService>();
        return services;
    }
}
