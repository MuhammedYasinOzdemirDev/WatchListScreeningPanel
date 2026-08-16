using Microsoft.EntityFrameworkCore;
using WatchListScreening.Application.Interfaces.Repositories;
using WatchListScreening.Domain.Entities;

namespace WatchListScreening.Infrastructure.Data.Repositories;

public class ListSourceRepository(AppDbContext context) : Repository<ListSource>(context), IListSourceRepository
{
    public async Task<IEnumerable<ListSource>> GetActiveWithScraperAsync()
        => await _dbSet.Where(x => x.IsActive && x.HasScraperImpl).ToListAsync();

    public async Task<ListSource?> GetByIdWithRunsAsync(int id, int runCount = 20)
        => await _dbSet
            .Include(x => x.Runs.OrderByDescending(r => r.StartedAt).Take(runCount))
            .FirstOrDefaultAsync(x => x.Id == id);
}
