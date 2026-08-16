using WatchListScreening.Domain.Entities;

namespace WatchListScreening.Application.Interfaces.Repositories;

public interface IUnitOfWork: IDisposable
{
    ISanctionEntryRepository SanctionEntries { get; }
    IRepository<ScreeningRequest> ScreeningRequests { get; }
    IScreeningResultRepository ScreeningResults { get; }

    IRepository<AuditLog> AuditLogs { get; }

    public IRepository<ListSource> ListSources { get; }
    public IRepository<ListSourceRun> ListSourceRuns { get; }
    public IRepository<HarvestedEntry> HarvestedEntries { get; }

    Task<int> SaveChangesAsync();
}
