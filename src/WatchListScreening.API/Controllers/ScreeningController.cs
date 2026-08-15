using Microsoft.AspNetCore.Mvc;
using WatchListScreening.Application.DTOs;
using WatchListScreening.Application.Interfaces.Services;

namespace WatchListScreening.API.Controllers;

/// <summary>
/// Yaptırım listesi tarama endpoint'leri.
/// Bir isim girer → sistem tüm listeyle karşılaştırır → eşleşmeleri döner.
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
    /// Tek isim taraması başlatır ve sonuçları döner.
    /// Senkron — sonuç hazır olana kadar bekler.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Screen([FromBody] CreateScreeningRequestDto dto)
    {
        var result = await _service.ScreenAsync(dto);
        return Ok(result);
    }

    /// <summary>
    /// Geçmiş bir tarama isteğini ID ile getirir.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }
}
