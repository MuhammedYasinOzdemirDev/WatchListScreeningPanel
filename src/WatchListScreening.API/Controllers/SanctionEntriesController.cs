using WatchListScreening.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using WatchListScreening.Application.Interfaces.Services;

namespace WatchListScreening.API.Controllers;

/// <summary>
/// Yaptırım kayıtları CRUD ve arama endpoint'leri.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SanctionEntriesController : ControllerBase
{
    private readonly ISanctionEntryService _service;

    // Constructor Injection — DI container bunu otomatik çözer
    public SanctionEntriesController(ISanctionEntryService service)
    {
        _service = service;
    }

    /// <summary>Tüm aktif yaptırım kayıtlarını listeler.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }

    /// <summary>ID'ye göre tek yaptırım kaydı döner.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Yeni yaptırım kaydı oluşturur.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSanctionEntryDto dto)
    {
        var result = await _service.CreateAsync(dto);
        // 201 Created + Location header → REST best practice
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Mevcut yaptırım kaydını günceller.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSanctionEntryDto dto)
    {
        await _service.UpdateAsync(id, dto);
        return NoContent(); // 204 — başarılı ama dönecek içerik yok
    }

    /// <summary>Yaptırım kaydını pasif yapar (soft delete).</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>İsme veya kaynak ID'ye göre arama yapar.</summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string query,
        [FromQuery] int? sourceId = null)
    {
        var result = await _service.SearchAsync(query, sourceId);
        return Ok(result);
    }
}
