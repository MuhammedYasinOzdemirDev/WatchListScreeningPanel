

using WatchListScreening.Application.Interfaces.Repositories;
using WatchListScreening.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace WatchListScreening.Infrastructure.Data.Repositories;

public class SanctionEntryRepository(AppDbContext context) : Repository<SanctionEntry>(context), ISanctionEntryRepository
{
    public async Task<IEnumerable<SanctionEntry>> SearchByNameAsync(string query)
        => await _dbSet
            .Where(x => x.IsActive && x.FullName.Contains(query))
            .ToListAsync();
    public async Task<IEnumerable<SanctionEntry>> GetByListSourceAsync(string listSource)
        => await _dbSet
            .Where(x => x.IsActive && x.ListSource == listSource)
            .ToListAsync();
}
