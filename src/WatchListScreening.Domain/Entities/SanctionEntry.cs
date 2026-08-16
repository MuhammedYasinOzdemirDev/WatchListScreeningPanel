using WatchListScreening.Domain.Common;
using WatchListScreening.Domain.Enums;

namespace WatchListScreening.Domain.Entities;

/// <summary>
/// International sanction list record.
/// Stores people and organizations obtained from OFAC, EU, UN, MASAK etc.
/// This table is the primary source for screening.
/// </summary>
public class SanctionEntry:BaseEntity
{
    /// <summary>
    /// Full name of person or organization.
    /// Main field used during screening.
    /// </summary>
    public string FullName { get; set; } = null!;

    /// <summary>
    /// First name of the person.
    /// Used for partial matching.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Last name of the person.
    /// Used for partial matching.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Person or Organization.
    /// </summary>
    public EntityType EntityType { get; set; }

    /// <summary>
    /// Country information.
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Date of birth.
    /// Used to distinguish people with identical names.
    /// </summary>
    public DateOnly? DateOfBirth { get; set; }

    /// <summary>
    /// National identification number.
    /// </summary>
    public string? NationalId { get; set; }
    
    /// <summary>FK — which registered source this entry came from.</summary>
    public int? ListSourceId { get; set; }
    
    /// <summary>Navigation — named ListSourceRef to avoid conflict with old ListSource string field removed above.</summary>
    public ListSource? ListSourceRef { get; set; }

    /// <summary>
    /// Aliases stored as JSON.
    /// </summary>
    public string? Aliases { get; set; }

    /// <summary>
    /// Additional structured information in JSON format.
    /// </summary>
    public string? AdditionalInfo { get; set; }

    /// <summary>
    /// Indicates whether sanction is still active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Date when sanction became active.
    /// </summary>
    public DateTime AddedAt { get; set; }

    /// <summary>
    /// Date when sanction was removed.
    /// </summary>
    public DateTime? DeactivatedAt { get; set; }

    /// <summary>
    /// Screening results associated with this sanction record.
    /// </summary>
    public ICollection<ScreeningResult> ScreeningResults { get; set; } = new List<ScreeningResult>();

    public ICollection<HarvestedEntry> HarvestedEntries { get; set; } = new List<HarvestedEntry>();
}