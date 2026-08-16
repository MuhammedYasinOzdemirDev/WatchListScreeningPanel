using WatchListScreening.Domain.Entities;

namespace WatchListScreening.Application.Interfaces.Repositories;

/// <summary>
/// Specialized repository for ListSource queries.
/// Include() calls are NOT here — they live in the Infrastructure implementation.
/// </summary>
public interface IListSourceRepository : IRepository<ListSource>
{
    /// <summary>Only sources where HasScraperImpl=true AND IsActive=true.</summary>
    Task<IEnumerable<ListSource>> GetActiveWithScraperAsync();

    /// <summary>Source with its last N runs — used in Details page.</summary>
    Task<ListSource?> GetByIdWithRunsAsync(int id, int runCount = 20);
}
