using WatchListScreening.Domain.Entities;

namespace WatchListScreening.Application.Interfaces.Repositories;

public interface ISanctionEntryRepository: IRepository<SanctionEntry>
{
    Task<IEnumerable<SanctionEntry>> SearchByNameAsync(string query);
    Task<IEnumerable<SanctionEntry>> GetByListSourceAsync(string listSource);
}
