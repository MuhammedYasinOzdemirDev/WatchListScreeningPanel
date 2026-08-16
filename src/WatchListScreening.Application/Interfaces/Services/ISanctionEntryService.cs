using WatchListScreening.Application.DTOs;

namespace WatchListScreening.Application.Interfaces.Services;

public interface ISanctionEntryService
{
    Task<IEnumerable<SanctionEntryDto>> GetAllAsync();
    Task<SanctionEntryDto?> GetByIdAsync(int id);
    Task<SanctionEntryDto> CreateAsync(CreateSanctionEntryDto dto);
    Task UpdateAsync(int id, UpdateSanctionEntryDto dto);
    Task DeleteAsync(int id);
    Task<IEnumerable<SanctionEntryDto>> SearchAsync(string query, int? sourceId);
}
