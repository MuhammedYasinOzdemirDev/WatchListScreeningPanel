using WatchListScreening.Application.DTOs;

namespace WatchListScreening.Application.Interfaces.Services;

public interface IHarvestService
{
    /// <summary>Publish a HarvestCommandMessage to RabbitMQ for a specific source.</summary>
    Task TriggerAsync(int listSourceId, string triggeredBy = "Manual");

    /// <summary>Get the latest run status for a source.</summary>
    Task<ListSourceRunDto?> GetRunStatusAsync(int listSourceId);

    /// <summary>Promote a HarvestedEntry to SanctionEntries (manual approval flow).</summary>
    Task<bool> ApproveEntryAsync(int harvestedEntryId);

    /// <summary>Reject a HarvestedEntry — marks IsProcessed=true without promoting.</summary>
    Task RejectEntryAsync(int harvestedEntryId);
}
