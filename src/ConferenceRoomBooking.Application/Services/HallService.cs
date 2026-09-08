using ConferenceRoomBooking.Application.DTOs;
using ConferenceRoomBooking.Application.Interfaces;
using ConferenceRoomBooking.Domain;
using DomainHallService = ConferenceRoomBooking.Domain.HallService;

namespace ConferenceRoomBooking.Application.Services;

public class HallService : IHallService
{
    private readonly IHallRepository _hallRepository;
    private readonly IUnitOfWork _unitOfWork;

    public HallService(IHallRepository hallRepository, IUnitOfWork unitOfWork)
    {
        _hallRepository = hallRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<HallCreatedResponse> CreateHallAsync(CreateHallRequestDto request, CancellationToken cancellationToken = default)
    {
        var hall = new Hall(request.Name, request.Capacity, request.BaseHourRate);

        if (request.Services != null)
        {
            foreach (var item in request.Services)
            {
                var service = new Service(item.Name, item.Price);
                hall.AvailableServices.Add(new DomainHallService(hall.Id, service));
            }
        }

        await _hallRepository.AddAsync(hall, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new HallCreatedResponse(hall.Id, hall.Name);
    }

    public async Task UpdateHallAsync(Guid id, UpdateHallRequestDto request, CancellationToken cancellationToken = default)
    {
        var hall = await _hallRepository.GetWithServicesByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Зал із ID {id} не знайдено.");

        hall.UpdateDetails(request.Name, request.Capacity, request.BaseHourRate);

        if (request.NewServices != null && request.NewServices.Count > 0)
        {
            foreach (var item in request.NewServices)
            {
                if (string.IsNullOrWhiteSpace(item.Name))
                    throw new ArgumentException("Назва послуги не може бути порожньою.");

                if (item.Price < 0)
                    throw new ArgumentException("Ціна послуги не може бути від'ємною.");

                // 1. Створюємо сутність послуги
                var service = new Service(item.Name, item.Price);

                // 2. Явно додаємо її в контекст
                await _hallRepository.AddServiceAsync(service, cancellationToken);

                // 3. Створюємо зв'язок між залом та послугою
                hall.AvailableServices.Add(new DomainHallService(hall.Id, service));
            }
        }

        // Зберігаємо обидві зміни в одній транзакції
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteHallAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var hall = await _hallRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Зал із ID {id} не знайдено.");

        hall.MarkAsDeleted();
        _hallRepository.Update(hall);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<HallDto>> SearchAvailableHallsAsync(
        SearchAvailableHallsRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.StartTime >= request.EndTime)
            throw new InvalidOperationException("Час початку має бути меншим за час закінчення.");

        if (request.RequiredCapacity <= 0)
            throw new ArgumentException("Необхідна місткість має бути більшою за 0.");

        var halls = await _hallRepository.GetAvailableHallsAsync(
            request.StartTime,
            request.EndTime,
            request.RequiredCapacity,
            cancellationToken);

        return halls.Select(h => new HallDto(
            h.Id,
            h.Name,
            h.Capacity,
            h.BaseHourlyRate,
            h.AvailableServices.Select(s => new ServiceDto(s.Service.Id, s.Service.Name, s.Service.Price)).ToList()
        )).ToList();
    }
}