using WatchListScreening.Application.DTOs;
using WatchListScreening.Application.Interfaces.Repositories;
using WatchListScreening.Application.Interfaces.Services;
using WatchListScreening.Domain.Entities;
using WatchListScreening.Domain.Enums;

namespace WatchListScreening.Application.Services;

public class ScreeningResultService : IScreeningResultService
{
    private readonly IUnitOfWork _unitOfWork;

    public ScreeningResultService(IUnitOfWork unitOfWork)
        => _unitOfWork = unitOfWork;

    public async Task<IEnumerable<ScreeningResultDto>> GetPendingAsync()
    {
        // Include() nerede olduğunu bilmiyoruz, bilmemize gerek yok
        var results = await _unitOfWork.ScreeningResults.GetPendingWithDetailsAsync();
        return results.Select(MapToDto);
    }

    public async Task ReviewAsync(int id, UpdateReviewDto dto)
    {
        var result = await _unitOfWork.ScreeningResults.GetByIdAsync(id);
        if (result is null) throw new KeyNotFoundException($"ScreeningResult {id} bulunamadı.");

        result.ReviewStatus = dto.Status;
        result.ReviewedBy = dto.ReviewedBy;
        result.ReviewedAt = DateTime.UtcNow;
        result.ReviewNotes = dto.ReviewNotes;
        result.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.ScreeningResults.Update(result);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<DashboardStatsDto> GetStatsAsync()
    {
        var today = DateTime.UtcNow.Date;
        return new DashboardStatsDto
        {
            TotalSanctionEntries = _unitOfWork.SanctionEntries.Query().Count(e => e.IsActive),
            TodayScreenings = _unitOfWork.ScreeningRequests.Query().Count(r => r.RequestedAt >= today),
            PendingReviews = _unitOfWork.ScreeningResults.Query().Count(r => r.ReviewStatus == ReviewStatus.Pending),
            HighRiskMatches = _unitOfWork.ScreeningResults.Query().Count(r =>
                                       r.ReviewStatus == ReviewStatus.Pending &&
                                       (r.RiskLevel == RiskLevel.High || r.RiskLevel == RiskLevel.Critical)),
            LastUpdated = DateTime.UtcNow
        };
    }

    private static ScreeningResultDto MapToDto(ScreeningResult r) => new()
    {
        Id = r.Id,
        ScreeningRequestId = r.ScreeningRequestId,
        MatchedFullName = r.SanctionEntry?.FullName ?? string.Empty,
        MatchedListSource = r.SanctionEntry?.ListSource ?? string.Empty,
        MatchScore = r.MatchScore,
        MatchedType = r.MatchedType,
        RiskLevel = r.RiskLevel,
        ReviewStatus = r.ReviewStatus,
        ReviewedBy = r.ReviewedBy,
        ReviewedAt = r.ReviewedAt,
        ReviewNotes = r.ReviewNotes,
        CreatedAt = r.CreatedAt
    };
}
