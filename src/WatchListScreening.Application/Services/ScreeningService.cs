using WatchListScreening.Application.DTOs;
using WatchListScreening.Application.Interfaces.Repositories;
using WatchListScreening.Application.Interfaces.Services;
using WatchListScreening.Domain.Entities;
using WatchListScreening.Domain.Enums;

namespace WatchListScreening.Application.Services;

public class ScreeningService : IScreeningService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly MatchingEngine _matchingEngine;
    private readonly ICacheService _cache;

    // Aynı sorgu tekrar gelirse cache'ten dön (TTL: 15 dk)
    // Hash: sorgu + tip birleşiminden üretilir — her benzersiz arama için farklı key
    private static string ScreeningCacheKey(string query, EntityType type)
        => $"screening:{query.ToLowerInvariant().Trim()}:{(int)type}";

    public ScreeningService(IUnitOfWork unitOfWork, MatchingEngine matchingEngine, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _matchingEngine = matchingEngine;
        _cache = cache;
    }

    public async Task<ScreeningRequestDto> ScreenAsync(CreateScreeningRequestDto dto)
    {
        var cacheKey = ScreeningCacheKey(dto.SearchQuery, dto.SearchType);

        // 1. Aynı isimle daha önce tarama yapıldıysa cache'ten dön
        var cached = await _cache.GetAsync<ScreeningRequestDto>(cacheKey);
        if (cached is not null)
            return cached;

        // 2. Cache MISS — gerçek tarama yap
        var request = new ScreeningRequest
        {
            SearchQuery = dto.SearchQuery,
            SearchType  = dto.SearchType,
            RequestedBy = dto.RequestedBy,
            // RequestedAt kaldırıldı — BaseEntity.CreatedAt kullanılıyor
            Status      = ScreeningStatus.Processing,
            CreatedAt   = DateTime.UtcNow
        };
        await _unitOfWork.ScreeningRequests.AddAsync(request);
        await _unitOfWork.SaveChangesAsync();

        var sanctionEntries = await _unitOfWork.SanctionEntries.GetAllAsync();
        var results = new List<ScreeningResult>();

        foreach (var entry in sanctionEntries.Where(e => e.IsActive))
        {
            var matchResult = _matchingEngine.CalculateBestMatch(dto.SearchQuery, entry.FullName);

            if (matchResult.Score < 40) continue;

            results.Add(new ScreeningResult
            {
                ScreeningRequestId = request.Id,
                SanctionEntryId = entry.Id,
                MatchScore = matchResult.Score,
                MatchedType = matchResult.MatchType,
                RiskLevel = CalculateRiskLevel(matchResult.Score),
                ReviewStatus = ReviewStatus.Pending,
                CreatedAt = DateTime.UtcNow
            });
        }

        foreach (var result in results)
            await _unitOfWork.ScreeningResults.AddAsync(result);

        request.Status = ScreeningStatus.Completed;
        request.CompletedAt = DateTime.UtcNow;
        request.TotalMatches = results.Count;
        _unitOfWork.ScreeningRequests.Update(request);
        await _unitOfWork.SaveChangesAsync();

        var resultDto = MapToDto(request, results);

        // 3. Sonucu cache'e yaz (15 dk TTL)
        await _cache.SetAsync(cacheKey, resultDto, TimeSpan.FromMinutes(15));

        return resultDto;
    }

    public async Task<ScreeningRequestDto?> GetByIdAsync(int id)
    {
        var request = await _unitOfWork.ScreeningRequests.GetByIdAsync(id);
        if (request is null) return null;

        var results = _unitOfWork.ScreeningResults
            .Query()
            .Where(r => r.ScreeningRequestId == id)
            .ToList();

        return MapToDto(request, results);
    }

    private static RiskLevel CalculateRiskLevel(decimal score) => score switch
    {
        >= 95 => RiskLevel.Critical,
        >= 80 => RiskLevel.High,
        >= 60 => RiskLevel.Medium,
        _ => RiskLevel.Low
    };

    private static ScreeningRequestDto MapToDto(ScreeningRequest request, List<ScreeningResult> results) => new()
    {
        Id           = request.Id,
        SearchQuery  = request.SearchQuery,
        SearchType   = request.SearchType,
        RequestedBy  = request.RequestedBy,
        RequestedAt  = request.CreatedAt,     // RequestedAt → CreatedAt'ten besleniyor
        CompletedAt  = request.CompletedAt,
        Status       = request.Status,
        TotalMatches = request.TotalMatches
    };
}
