using RefillingStation.Application.DTOs.Trips;

namespace RefillingStation.Application.Interfaces
{
    public interface ITripService
    {
        Task<IEnumerable<TripDetailResponse>> GetAllAsync();
        Task<TripDetailResponse> GetByIdAsync(int id);
        Task<int> CreateAsync(TripCreateRequest request);
        Task UpdateAsync(int id, TripUpdateRequest request);
        Task DeleteAsync(int id);
    }
}
