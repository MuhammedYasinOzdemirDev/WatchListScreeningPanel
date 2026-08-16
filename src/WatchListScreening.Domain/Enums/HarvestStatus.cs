namespace WatchListScreening.Domain.Enums;

/// <summary>
/// Status of a single harvest run for a list source.
/// NOT to be confused with ScreeningStatus.
/// </summary>
public enum HarvestStatus
{
    Running = 1,
    Success = 2,
    Failed = 3,
    PartialSuccess = 4
}
