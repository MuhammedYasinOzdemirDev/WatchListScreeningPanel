using WatchListScreening.Domain.Common;
using WatchListScreening.Domain.Enums;

namespace WatchListScreening.Domain.Entities;

/// <summary>
/// Represents a screening request initiated by a user.
/// </summary>
public class ScreeningRequest:BaseEntity
{

    /// <summary>
    /// Name entered by the user.
    /// </summary>
    public string SearchQuery { get; set; } = null!;

    /// <summary>
    /// Individual or Organization search.
    /// </summary>
    public EntityType SearchType { get; set; }

    /// <summary>
    /// User who initiated the screening.
    /// </summary>
    public string RequestedBy { get; set; } = null!;

    /// <summary>
    /// Completion timestamp.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Current processing status.
    /// </summary>
    public ScreeningStatus Status { get; set; }

    /// <summary>
    /// Number of matches found.
    /// </summary>
    public int? TotalMatches { get; set; }

    /// <summary>
    /// Indicates whether request is a bulk screening.
    /// </summary>
    public bool IsBulk { get; set; }

    /// <summary>
    /// Optional notes.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Screening results generated for this request.
    /// </summary>
    public ICollection<ScreeningResult> Results { get; set; } = new List<ScreeningResult>();
}