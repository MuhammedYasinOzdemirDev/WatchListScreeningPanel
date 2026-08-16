using WatchListScreening.Domain.Enums;

namespace WatchListScreening.Application.DTOs;

/// <summary>
/// Bir tarama sonucunun (ScreeningResult) uzman tarafından incelenip
/// karar verilmesi işlemi (Approved veya Confirmed) için gönderilen veri.
/// </summary>
public class UpdateReviewDto
{
    /// <summary>
    /// Uzmanın kararı:
    /// Approved (3) -> False positive (Risk yok)
    /// Confirmed (4) -> True match (Gerçek eşleşme)
    /// </summary>
    public ReviewStatus Status { get; set; }

    /// <summary>
    /// İncelemeyi yapan kişinin notu. (Neden bu kararı verdi?)
    /// Örn: "Doğum yılı uyuşmuyor, farklı bir kişi."
    /// </summary>
    public string ReviewNotes { get; set; } = string.Empty;

    /// <summary>
    /// İncelemeyi yapan kişinin kimliği.
    /// </summary>
    public string ReviewedBy { get; set; } = string.Empty;
}
