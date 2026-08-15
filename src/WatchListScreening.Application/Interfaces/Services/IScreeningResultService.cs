using WatchListScreening.Application.DTOs;

namespace WatchListScreening.Application.Interfaces.Services;

public interface IScreeningResultService
{
    /// <summary>ReviewStatus = Pending olan tüm sonuçlar — compliance uzmanının iş listesi.</summary>
    Task<IEnumerable<ScreeningResultDto>> GetPendingAsync();

    /// <summary>Bir sonucu incele: Approved, Confirmed veya Escalated yap.</summary>
    Task ReviewAsync(int id, UpdateReviewDto dto);

    /// <summary>Dashboard için özet istatistikler.</summary>
    Task<DashboardStatsDto> GetStatsAsync();
}
