using WatchListScreening.Domain.Entities;

namespace WatchListScreening.Application.Interfaces.Repositories;

/// <summary>
/// Specialized repository for HarvestedEntry deduplication and batch operations.
/// </summary>
public interface IHarvestedEntryRepository : IRepository<HarvestedEntry>
{
    /// <summary>Duplicate prevention — check by SHA256 hash before insert.</summary>
    Task<HarvestedEntry?> GetByHashAsync(string contentHash);

    /// <summary>Unprocessed entries waiting to be promoted to SanctionEntries.</summary>
    Task<IEnumerable<HarvestedEntry>> GetUnprocessedAsync(int take = 100);

    /// <summary>All entries from a specific harvest run — used in run detail view.</summary>
    Task<IEnumerable<HarvestedEntry>> GetByRunIdAsync(int runId);
}
