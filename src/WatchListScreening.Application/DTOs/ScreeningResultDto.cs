using WatchListScreening.Domain.Enums;

namespace WatchListScreening.Application.DTOs;

/// <summary>
/// Bir tarama isteğinin ürettiği tek bir eşleşme sonucu.
/// MatchScore ve RiskLevel burada — compliance uzmanı bunu görür ve karar verir.
/// </summary>
public class ScreeningResultDto
{
    /// <summary>Sonucun benzersiz kimliği.</summary>
    public int Id { get; set; }

    /// <summary>Hangi tarama isteğine ait.</summary>
    public int ScreeningRequestId { get; set; }

    /// <summary>Eşleşen yaptırım kaydının adı (join ile gelir).</summary>
    public string MatchedFullName { get; set; } = string.Empty;

    /// <summary>Eşleşen yaptırım kaydının listesi.</summary>
    public string MatchedListSource { get; set; } = string.Empty;

    /// <summary>
    /// Eşleşme skoru — 0.00 ile 100.00 arası.
    /// 100 = tam eşleşme, 0 = hiç benzemez.
    /// Bu skor compliance uzmanının önceliklendirme yaparken baktığı ilk alan.
    /// </summary>
    public decimal MatchScore { get; set; }

    /// <summary>
    /// Hangi algoritmayla eşleşti: Exact, Fuzzy, Contains, Phonetic.
    /// Exact match → çok güvenilir. Fuzzy → yazım hatası olabilir, dikkatli bakılmalı.
    /// </summary>
    public WatchListScreening.Domain.Enums.MatchType MatchedType { get; set; }

    /// <summary>
    /// Otomatik hesaplanan risk seviyesi (MatchScore'a göre).
    /// Low/Medium/High/Critical — UI'da renk kodlaması için kullanılır.
    /// </summary>
    public RiskLevel RiskLevel { get; set; }

    /// <summary>
    /// Compliance uzmanının inceleme durumu.
    /// Pending → henüz incelenmedi.
    /// Approved → false positive, risk yok.
    /// Confirmed → gerçek eşleşme, aksiyon gerekiyor.
    /// </summary>
    public ReviewStatus ReviewStatus { get; set; }

    /// <summary>Kim inceledi?</summary>
    public string? ReviewedBy { get; set; }

    /// <summary>Ne zaman incelendi?</summary>
    public DateTime? ReviewedAt { get; set; }

    /// <summary>İnceleme notu — "Farklı kişi, doğum tarihi uymuyor" gibi.</summary>
    public string? ReviewNotes { get; set; }

    /// <summary>Sonuç ne zaman oluşturuldu.</summary>
    public DateTime CreatedAt { get; set; }
}
