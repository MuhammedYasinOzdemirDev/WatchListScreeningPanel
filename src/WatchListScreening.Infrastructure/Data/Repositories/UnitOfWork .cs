using WatchListScreening.Application.Interfaces.Repositories;
using WatchListScreening.Domain.Entities;

namespace WatchListScreening.Infrastructure.Data.Repositories;

internal class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    public ISanctionEntryRepository SanctionEntries { get; }
    public IRepository<ScreeningRequest> ScreeningRequests { get; }
    public IScreeningResultRepository ScreeningResults { get; }
    public IRepository<AuditLog> AuditLogs { get; }
    public IListSourceRepository ListSources { get; }
    public IRepository<ListSourceRun> ListSourceRuns { get; }
    public IHarvestedEntryRepository HarvestedEntries { get; }


    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        SanctionEntries = new SanctionEntryRepository(context);
        ScreeningRequests = new Repository<ScreeningRequest>(context);
        ScreeningResults = new ScreeningResultRepository(context);
        AuditLogs = new Repository<AuditLog>(context);
        ListSources = new ListSourceRepository(context);
        ListSourceRuns = new Repository<ListSourceRun>(context);
        HarvestedEntries = new HarvestedEntryRepository(context);
    }
    public async Task<int> SaveChangesAsync()
        => await _context.SaveChangesAsync();
    public void Dispose()
        => _context.Dispose();
}
