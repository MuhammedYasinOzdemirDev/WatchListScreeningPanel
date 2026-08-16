using WatchListScreening.Application.DTOs;

namespace WatchListScreening.Application.Interfaces.Services;

/// <summary>
/// Yaptýrým listesi tarama operasyonlarýnýn sözleþmesi.
/// Controller buraya baðýmlý olur, implementasyonu bilmez.
/// </summary>
public interface IScreeningService
{
    /// <summary>
    /// Tek bir isim için tarama baþlatýr, sonuçlarý döner.
    /// Senkron çalýþýr — küçük taramalar için.
    /// </summary>
    Task<ScreeningRequestDto> ScreenAsync(CreateScreeningRequestDto dto);

    /// <summary>
    /// Tarama isteðini ID ile getirir (geçmiþ sorgusu için).
    /// </summary>
    Task<ScreeningRequestDto?> GetByIdAsync(int id);
}
