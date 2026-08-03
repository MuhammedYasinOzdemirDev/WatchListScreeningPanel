using WatchListScreening.Domain.Common;
using WatchListScreening.Domain.Enums;
using MatchType = WatchListScreening.Domain.Enums.MatchType;

namespace WatchListScreening.Domain.Entities;

/// <summary>
/// Represents a match produced during screening.
/// </summary>
public class ScreeningResult:BaseEntity
{

    /// <summary>
    /// Related screening request.
    /// </summary>
    public int ScreeningRequestId { get; set; }

    /// <summary>
    /// Matched sanction entry.
    /// </summary>
    public int SanctionEntryId { get; set; }

    /// <summary>
    /// Similarity score between 0.00 and 100.00.
    /// </summary>
    public decimal MatchScore { get; set; }

    /// <summary>
    /// Matching algorithm used.
    /// </summary>
    public MatchType MatchedType { get; set; }

    /// <summary>
    /// Calculated risk level.
    /// </summary>
    public RiskLevel RiskLevel { get; set; }

    /// <summary>
    /// Review status assigned by compliance team.
    /// </summary>
    public ReviewStatus ReviewStatus { get; set; }

    /// <summary>
    /// Reviewer name.
    /// </summary>
    public string? ReviewedBy { get; set; }

    /// <summary>
    /// Review timestamp.
    /// </summary>
    public DateTime? ReviewedAt { get; set; }

    /// <summary>
    /// Review notes.
    /// </summary>
    public string? ReviewNotes { get; set; }

    public ScreeningRequest ScreeningRequest { get; set; } = null!;

    public SanctionEntry SanctionEntry { get; set; } = null!;
}