using WatchListScreening.Domain.Enums;

namespace WatchListScreening.Application.DTOs;

/// <summary>
/// API'den dışarıya dönülen tarama isteği detayları.
/// </summary>
public class ScreeningRequestDto
{
    public int Id { get; set; }
    public string SearchQuery { get; set; } = string.Empty;
    public EntityType SearchType { get; set; }
    public string RequestedBy { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public ScreeningStatus Status { get; set; }
    public int? TotalMatches { get; set; }
    
    /// <summary>
    /// Tarama isteğinin sonucunda bulunan eşleşmelerin listesi.
    /// </summary>
    public List<ScreeningResultDto> Results { get; set; } = new();
}
