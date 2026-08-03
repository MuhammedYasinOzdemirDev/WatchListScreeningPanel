namespace WatchListScreening.Application.DTOs;

/// <summary>
/// Dashboard ana sayfasındaki özet istatistikler.
/// Bu veriler Redis'te cache'lenir (Modül 5) — her istekte DB sorgusu atmamak için.
/// </summary>
public class DashboardStatsDto
{
    /// <summary>
    /// Sistemdeki toplam aktif yaptırım kaydı sayısı.
    /// "Kaç kişiyi/kuruluşu tarayabiliyoruz?" sorusunun cevabı.
    /// </summary>
    public int TotalSanctionEntries { get; set; }

    /// <summary>
    /// Bugün yapılan tarama sayısı.
    /// Compliance ekibinin iş yükünü gösterir.
    /// </summary>
    public int TodayScreenings { get; set; }

    /// <summary>
    /// Henüz incelenmemiş sonuç sayısı.
    /// Bu sayı yüksekse compliance ekibi meşguldür → SLA riski.
    /// </summary>
    public int PendingReviews { get; set; }

    /// <summary>
    /// High veya Critical RiskLevel'daki bekleyen sonuç sayısı.
    /// Bunlar öncelikli incelenmeli — gerçek eşleşme olabilir.
    /// </summary>
    public int HighRiskMatches { get; set; }

    /// <summary>İstatistiklerin son güncellenme zamanı (cache için).</summary>
    public DateTime LastUpdated { get; set; }
}
