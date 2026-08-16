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

    /// <summary>
    /// Artık ListSource string yerine ListSourceId (FK) ile filtreleme yapılıyor.
    /// </summary>
    public async Task<IEnumerable<SanctionEntry>> GetByListSourceAsync(int? sourceId)
        => await _dbSet
            .Where(x => x.IsActive && x.ListSourceId == sourceId)
            .ToListAsync();
}
