using WatchListScreening.Application.DTOs;
using WatchListScreening.Application.Interfaces.Repositories;
using WatchListScreening.Application.Interfaces.Services;
using WatchListScreening.Domain.Entities;

namespace WatchListScreening.Application.Services;

public class SanctionEntryService : ISanctionEntryService
{
    private readonly IUnitOfWork _unitOfWork;

    public SanctionEntryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<SanctionEntryDto>> GetAllAsync()
    {
        var entries = await _unitOfWork.SanctionEntries.GetAllAsync();
        return entries.Select(e => MapToDto(e));
    }

    public async Task<SanctionEntryDto?> GetByIdAsync(int id)
    {
        var entry = await _unitOfWork.SanctionEntries.GetByIdAsync(id);
        return entry is null ? null : MapToDto(entry);
    }

    public async Task<SanctionEntryDto> CreateAsync(CreateSanctionEntryDto dto)
    {
        var entry = new SanctionEntry
        {
            FullName = dto.FullName,
            EntityType = dto.EntityType,
            Country = dto.Country,
            ListSource = dto.ListSource,
            IsActive = true,
            AddedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.SanctionEntries.AddAsync(entry);
        await _unitOfWork.SaveChangesAsync();
        return MapToDto(entry);
    }

    public async Task UpdateAsync(int id, UpdateSanctionEntryDto dto)
    {
        var entry = await _unitOfWork.SanctionEntries.GetByIdAsync(id);
        if (entry is null) throw new KeyNotFoundException($"SanctionEntry {id} not found.");

        entry.FullName = dto.FullName;
        entry.Country = dto.Country;
        entry.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.SanctionEntries.Update(entry);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entry = await _unitOfWork.SanctionEntries.GetByIdAsync(id);
        if (entry is null) throw new KeyNotFoundException($"SanctionEntry {id} not found.");

        entry.IsActive = false;         // Soft delete!
        entry.DeactivatedAt = DateTime.UtcNow;
        entry.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.SanctionEntries.Update(entry);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<SanctionEntryDto>> SearchAsync(string query, string? listSource)
    {
        var entries = await _unitOfWork.SanctionEntries.SearchByNameAsync(query);
        if (listSource is not null)
            entries = entries.Where(e => e.ListSource == listSource);
        return entries.Select(e => MapToDto(e));
    }

    private static SanctionEntryDto MapToDto(SanctionEntry e) => new()
    {
        Id = e.Id,
        FullName = e.FullName,
        EntityType = e.EntityType,
        Country = e.Country,
        ListSource = e.ListSource,
        IsActive = e.IsActive,
        DateOfBirth = e.DateOfBirth,
        CreatedAt = e.CreatedAt
    };
}
