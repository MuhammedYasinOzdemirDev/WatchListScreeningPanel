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

    public ScreeningService(IUnitOfWork unitOfWork, MatchingEngine matchingEngine)
    {
        _unitOfWork = unitOfWork;
        _matchingEngine = matchingEngine;
    }

    public async Task<ScreeningRequestDto> ScreenAsync(CreateScreeningRequestDto dto)
    {
        // 1. Tarama isteğini kaydet
        var request = new ScreeningRequest
        {
            SearchQuery = dto.SearchQuery,
            SearchType = dto.SearchType,
            RequestedBy = dto.RequestedBy,
            RequestedAt = DateTime.UtcNow,
            Status = ScreeningStatus.Processing,
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.ScreeningRequests.AddAsync(request);
        await _unitOfWork.SaveChangesAsync();

        // 2. Tüm aktif kayıtları al ve eşleştir
        var sanctionEntries = await _unitOfWork.SanctionEntries.GetAllAsync();
        var results = new List<ScreeningResult>();

        foreach (var entry in sanctionEntries.Where(e => e.IsActive))
        {
            var matchResult = _matchingEngine.CalculateBestMatch(dto.SearchQuery, entry.FullName);

            // Sadece %40 üzerindeki eşleşmeleri kaydet (gürültüyü filtrele)
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

        // 3. Sonuçları kaydet
        foreach (var result in results)
            await _unitOfWork.ScreeningResults.AddAsync(result);

        // 4. İsteği tamamlandı olarak işaretle
        request.Status = ScreeningStatus.Completed;
        request.CompletedAt = DateTime.UtcNow;
        request.TotalMatches = results.Count;
        _unitOfWork.ScreeningRequests.Update(request);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(request, results);
    }

    /// <summary>
    /// Geçmiş bir tarama isteğini ID ile getirir.
    /// Controller GET /api/screening/{id} için kullanır.
    /// </summary>
    public async Task<ScreeningRequestDto?> GetByIdAsync(int id)
    {
        var request = await _unitOfWork.ScreeningRequests.GetByIdAsync(id);
        if (request is null) return null;

        // Results navigation property lazy load edilmez — Query ile çekiyoruz
        var results = _unitOfWork.ScreeningResults
            .Query()
            .Where(r => r.ScreeningRequestId == id)
            .ToList();

        return MapToDto(request, results);
    }

    /// <summary>MatchScore'a göre risk seviyesi atar.</summary>
    private static RiskLevel CalculateRiskLevel(decimal score) => score switch
    {
        >= 95 => RiskLevel.Critical,
        >= 80 => RiskLevel.High,
        >= 60 => RiskLevel.Medium,
        _ => RiskLevel.Low
    };

    private static ScreeningRequestDto MapToDto(ScreeningRequest request, List<ScreeningResult> results) => new()
    {
        Id = request.Id,
        SearchQuery = request.SearchQuery,
        SearchType = request.SearchType,
        RequestedBy = request.RequestedBy,
        RequestedAt = request.RequestedAt,
        CompletedAt = request.CompletedAt,
        Status = request.Status,
        TotalMatches = request.TotalMatches
    };
}
