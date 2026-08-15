using WatchListScreening.Domain.Entities;

namespace WatchListScreening.Application.Interfaces.Repositories;

public interface IUnitOfWork: IDisposable
{
    ISanctionEntryRepository SanctionEntries { get; }
    IRepository<ScreeningRequest> ScreeningRequests { get; }
    IScreeningResultRepository ScreeningResults { get; }

    IRepository<AuditLog> AuditLogs { get; }
    Task<int> SaveChangesAsync();
}
