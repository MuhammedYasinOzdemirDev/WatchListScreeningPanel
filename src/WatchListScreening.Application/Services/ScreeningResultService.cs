using WatchListScreening.Application.DTOs;
using WatchListScreening.Application.Interfaces.Repositories;
using WatchListScreening.Application.Interfaces.Services;
using WatchListScreening.Domain.Entities;
using WatchListScreening.Domain.Enums;

namespace WatchListScreening.Application.Services;

public class ScreeningResultService : IScreeningResultService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    private const string StatsCacheKey = "dashboard:stats";

    public ScreeningResultService(IUnitOfWork unitOfWork, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _cache      = cache;
    }

    public async Task<IEnumerable<ScreeningResultDto>> GetPendingAsync()
    {
        // Pending listesi cache'lenmez — anlık değişir
        var results = await _unitOfWork.ScreeningResults.GetPendingWithDetailsAsync();
        return results.Select(MapToDto);
    }

    public async Task ReviewAsync(int id, UpdateReviewDto dto)
    {
        var result = await _unitOfWork.ScreeningResults.GetByIdAsync(id);
        if (result is null) throw new KeyNotFoundException($"ScreeningResult {id} bulunamadı.");

        result.ReviewStatus = dto.Status;
        result.ReviewedBy   = dto.ReviewedBy;
        result.ReviewedAt   = DateTime.UtcNow;
        result.ReviewNotes  = dto.ReviewNotes;
        result.UpdatedAt    = DateTime.UtcNow;

        _unitOfWork.ScreeningResults.Update(result);
        await _unitOfWork.SaveChangesAsync();

        // İnceleme sonrası dashboard stats cache'ini geçersiz kıl
        await _cache.RemoveAsync(StatsCacheKey);
    }

    public async Task<DashboardStatsDto> GetStatsAsync()
    {
        // 1. Cache'e bak (TTL: 5 dk)
        var cached = await _cache.GetAsync<DashboardStatsDto>(StatsCacheKey);
        if (cached is not null)
            return cached;

        // 2. Cache MISS — DB'den hesapla
        var today = DateTime.UtcNow.Date;
        var stats = new DashboardStatsDto
        {
            TotalSanctionEntries = _unitOfWork.SanctionEntries.Query().Count(e => e.IsActive),
            TodayScreenings      = _unitOfWork.ScreeningRequests.Query().Count(r => r.CreatedAt >= today),
            PendingReviews       = _unitOfWork.ScreeningResults.Query().Count(r => r.ReviewStatus == ReviewStatus.Pending),
            HighRiskMatches      = _unitOfWork.ScreeningResults.Query().Count(r =>
                                        r.ReviewStatus == ReviewStatus.Pending &&
                                        (r.RiskLevel == RiskLevel.High || r.RiskLevel == RiskLevel.Critical)),
            LastUpdated = DateTime.UtcNow
        };

        // 3. Cache'e yaz (5 dk TTL)
        await _cache.SetAsync(StatsCacheKey, stats, TimeSpan.FromMinutes(5));
        return stats;
    }

    private static ScreeningResultDto MapToDto(ScreeningResult r) => new()
    {
        Id                 = r.Id,
        ScreeningRequestId = r.ScreeningRequestId,
        MatchedFullName    = r.SanctionEntry?.FullName ?? string.Empty,
        MatchedListSource  = r.SanctionEntry?.ListSourceRef?.Name ?? string.Empty,
        MatchScore         = r.MatchScore,
        MatchedType        = r.MatchedType,
        RiskLevel          = r.RiskLevel,
        ReviewStatus       = r.ReviewStatus,
        ReviewedBy         = r.ReviewedBy,
        ReviewedAt         = r.ReviewedAt,
        ReviewNotes        = r.ReviewNotes,
        CreatedAt          = r.CreatedAt
    };
}
