using WatchListScreening.Application.DTOs;

namespace WatchListScreening.Application.Interfaces.Services;

/// <summary>
/// Yaptırım listesi tarama operasyonlarının sözleşmesi.
/// Controller buraya bağımlı olur, implementasyonu bilmez.
/// </summary>
public interface IScreeningService
{
    /// <summary>
    /// Tek bir isim için tarama başlatır, sonuçları döner.
    /// Senkron çalışır — küçük taramalar için.
    /// </summary>
    Task<ScreeningRequestDto> ScreenAsync(CreateScreeningRequestDto dto);

    /// <summary>
    /// Tarama isteğini ID ile getirir (geçmiş sorgusu için).
    /// </summary>
    Task<ScreeningRequestDto?> GetByIdAsync(int id);
}
