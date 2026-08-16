using WatchListScreening.Application.DTOs;

namespace WatchListScreening.Application.Interfaces.Services;

public interface IListSourceService
{
    Task<IEnumerable<ListSourceDto>> GetAllAsync();
    Task<ListSourceDto?> GetByIdAsync(int id);
    Task<ListSourceDto?> GetByIdWithHistoryAsync(int id);
    Task<ListSourceDto> CreateAsync(CreateListSourceDto dto);
    Task UpdateAsync(int id, UpdateListSourceDto dto);
    Task DeleteAsync(int id);
    Task<IEnumerable<ListSourceDto>> GetActiveAsync();
}
