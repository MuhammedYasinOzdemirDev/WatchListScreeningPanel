using WatchListScreening.Domain.Enums;

namespace WatchListScreening.Application.DTOs.Harvest;

public class CleanedItem
{
    public string RawFullName { get; set; } = string.Empty;
    public string CleanedFullName { get; set; } = string.Empty;
    public string? Country { get; set; }
    public EntityType? EntityType { get; set; }
    public string ContentHash { get; set; } = string.Empty;
    public string? DateOfBirth { get; set; }
    public string? CleanedFirstName { get; set; }
    public string? CleanedLastName { get; set; }
}
