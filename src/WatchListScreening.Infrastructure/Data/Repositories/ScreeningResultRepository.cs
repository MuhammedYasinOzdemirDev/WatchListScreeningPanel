using Microsoft.EntityFrameworkCore; 
using WatchListScreening.Application.Interfaces.Repositories;
using WatchListScreening.Domain.Entities;
using WatchListScreening.Domain.Enums;

namespace WatchListScreening.Infrastructure.Data.Repositories;

public class ScreeningResultRepository : Repository<ScreeningResult>, IScreeningResultRepository
{
    public ScreeningResultRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<ScreeningResult>> GetPendingWithDetailsAsync()
        => await _dbSet
            .Include(r => r.SanctionEntry)    
            .Include(r => r.ScreeningRequest)
            .Where(r => r.ReviewStatus == ReviewStatus.Pending)
            .OrderByDescending(r => r.MatchScore)
            .ToListAsync();
}
