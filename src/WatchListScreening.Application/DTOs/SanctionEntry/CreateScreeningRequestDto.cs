using WatchListScreening.Domain.Enums;

namespace WatchListScreening.Application.DTOs;

/// <summary>
/// Yeni bir tarama isteği başlatmak için API'ye gönderilen veri.
/// </summary>
public class CreateScreeningRequestDto
{
    /// <summary>
    /// Taranacak kişi veya kurumun adı.
    /// Örn: "John Smith"
    /// </summary>
    public string SearchQuery { get; set; } = string.Empty;

    /// <summary>
    /// Tarama türü: Person (1) veya Organization (2)
    /// </summary>
    public EntityType SearchType { get; set; }

    /// <summary>
    /// İsteği yapan kullanıcının adı veya ID'si (Audit Log için).
    /// </summary>
    public string RequestedBy { get; set; } = string.Empty;
}
