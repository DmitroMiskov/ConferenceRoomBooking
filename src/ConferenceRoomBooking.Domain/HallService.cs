using System;

namespace ConferenceRoomBooking.Domain;

public class HallService
{
    public Guid HallId { get; private set; }
    public Guid ServiceId { get; private set; }
    public Hall Hall { get; private set; } = null!;
    public Service Service { get; private set; } = null!;

    private HallService() { }

    public HallService(Guid hallId, Service service)
    {
        HallId = hallId;
        ServiceId = service.Id;
        Service = service;
    }
}