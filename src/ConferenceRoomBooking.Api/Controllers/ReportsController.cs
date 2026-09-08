using System;
using System.Threading;
using System.Threading.Tasks;
using ConferenceRoomBooking.Application.DTOs;
using ConferenceRoomBooking.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceRoomBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>
    /// Звіт завантаженості залів у % та годинах за період
    /// </summary>
    [HttpGet("occupancy")]
    public async Task<IActionResult> GetOccupancy(
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc,
        CancellationToken cancellationToken)
    {
        var result = await _reportService.GetOccupancyReportAsync(
            new DateRangeReportRequestDto(fromUtc, toUtc),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Звіт щодо загального доходу, середнього чека та структури продажів (оренда vs послуги)
    /// </summary>
    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenue(
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc,
        CancellationToken cancellationToken)
    {
        var result = await _reportService.GetRevenueReportAsync(
            new DateRangeReportRequestDto(fromUtc, toUtc),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Рейтинг популярності та прибутковості додаткових послуг (upsell)
    /// </summary>
    [HttpGet("popular-services")]
    public async Task<IActionResult> GetPopularServices(
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc,
        CancellationToken cancellationToken)
    {
        var result = await _reportService.GetPopularServicesReportAsync(
            new DateRangeReportRequestDto(fromUtc, toUtc),
            cancellationToken);
        return Ok(result);
    }
}