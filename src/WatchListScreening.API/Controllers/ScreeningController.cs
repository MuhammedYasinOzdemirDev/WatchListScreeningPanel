using WatchListScreening.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using WatchListScreening.Application.Interfaces.Services;

namespace WatchListScreening.API.Controllers;

/// <summary>
/// Yaptýrým listesi tarama endpoint'leri.
/// Bir isim girer › sistem tüm listeyle karþýlaþtýrýr › eþleþmeleri döner.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ScreeningController : ControllerBase
{
    private readonly IScreeningService _service;

    public ScreeningController(IScreeningService service)
    {
        _service = service;
    }

    /// <summary>
    /// Tek isim taramasý baþlatýr ve sonuçlarý döner.
    /// Senkron — sonuç hazýr olana kadar bekler.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Screen([FromBody] CreateScreeningRequestDto dto)
    {
        var result = await _service.ScreenAsync(dto);
        return Ok(result);
    }

    /// <summary>
    /// Geçmiþ bir tarama isteðini ID ile getirir.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }
}
