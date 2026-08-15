using WatchListScreening.Application.DTOs;
using WatchListScreening.Application.Interfaces.Repositories;
using WatchListScreening.Application.Interfaces.Services;
using WatchListScreening.Domain.Entities;

namespace WatchListScreening.Application.Services;

public class SanctionEntryService : ISanctionEntryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    private const string AllCacheKey = "sanctions:all";
    private static string IdCacheKey(int id) => $"sanctions:{id}";

    public SanctionEntryService(IUnitOfWork unitOfWork, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _cache      = cache;
    }

    public async Task<IEnumerable<SanctionEntryDto>> GetAllAsync()
    {
        // 1. Önce cache (TTL: 60 dk)
        var cached = await _cache.GetAsync<IEnumerable<SanctionEntryDto>>(AllCacheKey);
        if (cached is not null)
            return cached;

        // 2. Cache MISS — DB'den çek
        var entries = await _unitOfWork.SanctionEntries.GetAllAsync();
        var result  = entries.Select(MapToDto).ToList();

        // 3. Cache'e yaz
        await _cache.SetAsync(AllCacheKey, result, TimeSpan.FromHours(1));
        return result;
    }

    public async Task<SanctionEntryDto?> GetByIdAsync(int id)
    {
        var cached = await _cache.GetAsync<SanctionEntryDto>(IdCacheKey(id));
        if (cached is not null)
            return cached;

        var entry = await _unitOfWork.SanctionEntries.GetByIdAsync(id);
        if (entry is null) return null;

        var dto = MapToDto(entry);
        await _cache.SetAsync(IdCacheKey(id), dto, TimeSpan.FromMinutes(30));
        return dto;
    }

    public async Task<SanctionEntryDto> CreateAsync(CreateSanctionEntryDto dto)
    {
        var entry = new SanctionEntry
        {
            FullName   = dto.FullName,
            EntityType = dto.EntityType,
            Country    = dto.Country,
            ListSource = dto.ListSource,
            IsActive   = true,
            AddedAt    = DateTime.UtcNow,
            CreatedAt  = DateTime.UtcNow
        };
        await _unitOfWork.SanctionEntries.AddAsync(entry);
        await _unitOfWork.SaveChangesAsync();

        // Yeni kayıt → tüm liste cache'i geçersiz
        await _cache.RemoveAsync(AllCacheKey);
        return MapToDto(entry);
    }

    public async Task UpdateAsync(int id, UpdateSanctionEntryDto dto)
    {
        var entry = await _unitOfWork.SanctionEntries.GetByIdAsync(id);
        if (entry is null) throw new KeyNotFoundException($"SanctionEntry {id} not found.");

        entry.FullName  = dto.FullName;
        entry.Country   = dto.Country;
        entry.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.SanctionEntries.Update(entry);
        await _unitOfWork.SaveChangesAsync();

        // Güncellenen kayıt + tüm liste cache'ini sil (invalidation)
        await _cache.RemoveAsync(IdCacheKey(id));
        await _cache.RemoveAsync(AllCacheKey);
    }

    public async Task DeleteAsync(int id)
    {
        var entry = await _unitOfWork.SanctionEntries.GetByIdAsync(id);
        if (entry is null) throw new KeyNotFoundException($"SanctionEntry {id} not found.");

        entry.IsActive     = false; // Soft delete
        entry.DeactivatedAt = DateTime.UtcNow;
        entry.UpdatedAt    = DateTime.UtcNow;
        _unitOfWork.SanctionEntries.Update(entry);
        await _unitOfWork.SaveChangesAsync();

        await _cache.RemoveAsync(IdCacheKey(id));
        await _cache.RemoveAsync(AllCacheKey);
    }

    public async Task<IEnumerable<SanctionEntryDto>> SearchAsync(string query, string? listSource)
    {
        // Arama sonuçları cache'lenmez — her sorgu farklı olabilir
        var entries = await _unitOfWork.SanctionEntries.SearchByNameAsync(query);
        if (listSource is not null)
            entries = entries.Where(e => e.ListSource == listSource);
        return entries.Select(MapToDto);
    }

    private static SanctionEntryDto MapToDto(SanctionEntry e) => new()
    {
        Id         = e.Id,
        FullName   = e.FullName,
        EntityType = e.EntityType,
        Country    = e.Country,
        ListSource = e.ListSource,
        IsActive   = e.IsActive,
        DateOfBirth = e.DateOfBirth,
        CreatedAt  = e.CreatedAt
    };
}
