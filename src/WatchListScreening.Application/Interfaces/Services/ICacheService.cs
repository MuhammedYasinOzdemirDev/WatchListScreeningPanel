namespace WatchListScreening.Application.Interfaces.Services;

/// <summary>
/// Cache servisinin sözleşmesi (Application katmanı).
/// Burada StackExchange.Redis gibi somut kütüphane referansı YOK — Clean Architecture kuralı.
/// </summary>
public interface ICacheService
{
    /// <summary>Cache'ten T tipinde veri okur. Key yoksa null döner (Cache MISS).</summary>
    Task<T?> GetAsync<T>(string key);

    /// <summary>Cache'e T tipinde veri yazar. expiry verilmezse varsayılan 30 dk kullanılır.</summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);

    /// <summary>Cache'teki bir key'i siler. Invalidation için kullanılır.</summary>
    Task RemoveAsync(string key);
}
