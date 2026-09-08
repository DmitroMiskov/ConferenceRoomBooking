using ConferenceRoomBooking.Application.DTOs;
using ConferenceRoomBooking.Application.Interfaces;
using ConferenceRoomBooking.Domain;
using FluentAssertions;
using Moq;
using Xunit;

namespace ConferenceRoomBooking.UnitTests;

public class BookingServiceTests
{
    private readonly ConferenceRoomBooking.Application.Services.BookingService _bookingService;
    private readonly Mock<IHallRepository> _hallRepositoryMock;
    private readonly Mock<IBookingRepository> _bookingRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public BookingServiceTests()
    {
        _hallRepositoryMock = new Mock<IHallRepository>();
        _bookingRepositoryMock = new Mock<IBookingRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _bookingService = new ConferenceRoomBooking.Application.Services.BookingService(
            _hallRepositoryMock.Object,
            _bookingRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task CreateBookingAsync_WhenStartTimeIsAfterEndTime_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var request = new CreateBookingRequestDto(
            HallId: Guid.NewGuid(),
            CustomerName: "Тест Клієнт",
            CustomerEmail: "test@example.com",
            StartTime: DateTime.UtcNow.AddHours(3),
            EndTime: DateTime.UtcNow.AddHours(2),
            SelectedServiceIds: new List<Guid>()
        );

        // Act
        var act = async () => await _bookingService.CreateBookingAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Час початку має бути меншим за час закінчення*");
    }

    [Fact]
    public async Task CreateBookingAsync_WhenStartTimeIsInThePast_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var request = new CreateBookingRequestDto(
            HallId: Guid.NewGuid(),
            CustomerName: "Тест Клієнт",
            CustomerEmail: "test@example.com",
            StartTime: DateTime.UtcNow.AddHours(-2),
            EndTime: DateTime.UtcNow.AddHours(1),
            SelectedServiceIds: new List<Guid>()
        );

        // Act
        var act = async () => await _bookingService.CreateBookingAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Неможливо забронювати зал у минулому часі*");
    }

    [Fact]
    public async Task CreateBookingAsync_WhenHallNotFound_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var hallId = Guid.NewGuid();
        var request = new CreateBookingRequestDto(
            HallId: hallId,
            CustomerName: "Тест Клієнт",
            CustomerEmail: "test@example.com",
            StartTime: DateTime.UtcNow.AddDays(1),
            EndTime: DateTime.UtcNow.AddDays(1).AddHours(2),
            SelectedServiceIds: new List<Guid>()
        );

        _hallRepositoryMock
            .Setup(r => r.GetWithServicesByIdAsync(hallId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Hall?)null);

        // Act
        var act = async () => await _bookingService.CreateBookingAsync(request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{hallId}*");
    }

    [Fact]
    public async Task CreateBookingAsync_WhenTimeSlotIsOccupied_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var hallId = Guid.NewGuid();
        var start = DateTime.UtcNow.AddDays(1);
        var end = start.AddHours(2);
        var hall = new Hall("Конференц-Зал", 50, 400m);

        var request = new CreateBookingRequestDto(
            HallId: hallId,
            CustomerName: "Тест Клієнт",
            CustomerEmail: "test@example.com",
            StartTime: start,
            EndTime: end,
            SelectedServiceIds: new List<Guid>()
        );

        _hallRepositoryMock
            .Setup(r => r.GetWithServicesByIdAsync(hallId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hall);

        _bookingRepositoryMock
            .Setup(r => r.HasOverlappingBookingAsync(hallId, start, end, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var act = async () => await _bookingService.CreateBookingAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Обраний часовий слот для цього залу вже зайнятий*");
    }

    [Fact]
    public async Task CreateBookingAsync_WhenValidDataProvided_ShouldCalculatePriceAndPersistBooking()
    {
        // Arrange
        var start = DateTime.UtcNow.AddDays(1);
        var end = start.AddHours(3);
        var hall = new Hall("Великий Зал", 100, 500m);

        var service = new Service("Проєктор 4K", 300m);
        var hallService = new HallService(hall.Id, service.Id);
        // Use reflection to set the private setter for testing purposes
        typeof(HallService).GetProperty("Service")!
            .SetValue(hallService, service);
        hall.AvailableServices.Add(hallService);

        var request = new CreateBookingRequestDto(
            HallId: hall.Id,
            CustomerName: "Іван Петренко",
            CustomerEmail: "ivan@example.com",
            StartTime: start,
            EndTime: end,
            SelectedServiceIds: new List<Guid> { service.Id }
        );

        _hallRepositoryMock
            .Setup(r => r.GetWithServicesByIdAsync(hall.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hall);

        _bookingRepositoryMock
            .Setup(r => r.HasOverlappingBookingAsync(hall.Id, start, end, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var response = await _bookingService.CreateBookingAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.CustomerName.Should().Be("Іван Петренко");
        response.HallCost.Should().Be(1500m);
        response.ServicesCost.Should().Be(300m);
        response.TotalPrice.Should().Be(1800m);
        response.ServiceNames.Should().ContainSingle().Which.Should().Be("Проєктор 4K");

        _bookingRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}