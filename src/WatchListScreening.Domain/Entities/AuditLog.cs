using WatchListScreening.Domain.Common;

namespace WatchListScreening.Domain.Entities;

/// <summary>
/// Stores audit information for important system actions.
/// </summary>
public class AuditLog:BaseEntity
{
    /// <summary>
    /// Action performed.
    /// Example: Create, Update, Delete, Screen, Export.
    /// </summary>
    public string Action { get; set; } = null!;

    /// <summary>
    /// Entity affected by the action.
    /// </summary>
    public string EntityType { get; set; } = null!;

    /// <summary>
    /// Related entity identifier.
    /// </summary>
    public int? EntityId { get; set; }

    /// <summary>
    /// User who performed the action.
    /// </summary>
    public string PerformedBy { get; set; } = null!;

    /// <summary>
    /// Action timestamp.
    /// </summary>
    public DateTime PerformedAt { get; set; }

    /// <summary>
    /// Previous values in JSON format.
    /// </summary>
    public string? OldValues { get; set; }

    /// <summary>
    /// New values in JSON format.
    /// </summary>
    public string? NewValues { get; set; }

    /// <summary>
    /// Client IP address.
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Additional details.
    /// </summary>
    public string? Details { get; set; }
}