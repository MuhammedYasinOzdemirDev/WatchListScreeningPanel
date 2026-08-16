using WatchListScreening.Domain.Entities;

namespace WatchListScreening.Application.Interfaces.Repositories;

public interface IUnitOfWork: IDisposable
{
    ISanctionEntryRepository SanctionEntries { get; }
    IRepository<ScreeningRequest> ScreeningRequests { get; }
    IScreeningResultRepository ScreeningResults { get; }

    IRepository<AuditLog> AuditLogs { get; }

    IRepository<ListSourceRun> ListSourceRuns { get; }
    IListSourceRepository ListSources { get; }
    IHarvestedEntryRepository HarvestedEntries { get; }


    Task<int> SaveChangesAsync();
}
