using ConferenceRoomBooking.Application.DTOs;
using ConferenceRoomBooking.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceRoomBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HallsController : ControllerBase
{
    private readonly IHallService _hallService;

    public HallsController(IHallService hallService)
    {
        _hallService = hallService;
    }

    // 1. Додавання конференц-залу
    [HttpPost]
    [ProducesResponseType(typeof(HallCreatedResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateHall([FromBody] CreateHallRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _hallService.CreateHallAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    // 2. Редагування інформації про зал
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateHall(Guid id, [FromBody] UpdateHallRequestDto request, CancellationToken cancellationToken)
    {
        await _hallService.UpdateHallAsync(id, request, cancellationToken);
        return NoContent();
    }

    // 3. Видалення конференц-залу
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteHall(Guid id, CancellationToken cancellationToken)
    {
        await _hallService.DeleteHallAsync(id, cancellationToken);
        return NoContent();
    }

    // 4. Пошук доступних залів за датою, часом і місткістю
    [HttpGet("search")]
    [ProducesResponseType(typeof(IReadOnlyList<HallDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchAvailableHalls(
        [FromQuery] DateTime startTime,
        [FromQuery] DateTime endTime,
        [FromQuery] int requiredCapacity,
        CancellationToken cancellationToken)
    {
        var request = new SearchAvailableHallsRequestDto(startTime, endTime, requiredCapacity);
        var halls = await _hallService.SearchAvailableHallsAsync(request, cancellationToken);
        return Ok(halls);
    }
}