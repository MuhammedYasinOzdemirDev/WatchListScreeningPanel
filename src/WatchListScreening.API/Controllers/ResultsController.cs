using WatchListScreening.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using WatchListScreening.Application.Interfaces.Services;

namespace WatchListScreening.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResultsController : ControllerBase
{
    private readonly IScreeningResultService _service;

    public ResultsController(IScreeningResultService service) => _service = service;

    /// <summary>Ýnceleme bekleyen sonuçlar — compliance uzmanýnýn iþ listesi.</summary>
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
        => Ok(await _service.GetPendingAsync());

    /// <summary>Bir sonucu incele (Approved / Confirmed / Escalated).</summary>
    [HttpPut("{id:int}/review")]
    public async Task<IActionResult> Review(int id, [FromBody] UpdateReviewDto dto)
    {
        await _service.ReviewAsync(id, dto);
        return NoContent();
    }

    /// <summary>Dashboard istatistikleri.</summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
        => Ok(await _service.GetStatsAsync());
}
