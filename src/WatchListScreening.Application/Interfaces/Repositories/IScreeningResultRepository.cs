using WatchListScreening.Domain.Entities;

namespace WatchListScreening.Application.Interfaces.Repositories;

/// <summary>
/// ScreeningResult için özelleşmiş repository.
/// Navigation property gerektiren sorgular burada tanımlanır.
/// "Ne istiyorum" Application'da (interface), "nasıl yüklüyorum" Infrastructure'da (impl).
/// Bu sayede Application katmanı hiç EF Core görmez.
/// </summary>
public interface IScreeningResultRepository : IRepository<ScreeningResult>
{
    /// <summary>
    /// Pending durumundaki sonuçları SanctionEntry ve ScreeningRequest
    /// navigation property'leriyle birlikte getirir.
    /// Include() detayı Infrastructure implementasyonunda gizlidir.
    /// </summary>
    Task<IEnumerable<ScreeningResult>> GetPendingWithDetailsAsync();
}
