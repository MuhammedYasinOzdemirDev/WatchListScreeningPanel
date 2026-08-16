using Microsoft.EntityFrameworkCore;
using WatchListScreening.Application.Interfaces.Repositories;
using WatchListScreening.Domain.Entities;

namespace WatchListScreening.Infrastructure.Data.Repositories;

public class HarvestedEntryRepository(AppDbContext context) : Repository<HarvestedEntry>(context), IHarvestedEntryRepository
{
    public async Task<HarvestedEntry?> GetByHashAsync(string contentHash)
        => await _dbSet.FirstOrDefaultAsync(x => x.ContentHash == contentHash);

    public async Task<IEnumerable<HarvestedEntry>> GetUnprocessedAsync(int take = 100)
        => await _dbSet.Where(x => !x.IsProcessed).Take(take).ToListAsync();

    public async Task<IEnumerable<HarvestedEntry>> GetByRunIdAsync(int runId)
        => await _dbSet.Where(x => x.ListSourceRunId == runId).ToListAsync();
}
