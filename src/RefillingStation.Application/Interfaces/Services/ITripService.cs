using RefillingStation.Application.DTOs.Trips;

namespace RefillingStation.Application.Interfaces.Services
{
    public interface ITripService
    {
        Task<List<TripDetailResponse>> GetAllAsync();
        Task<TripDetailResponse> GetByIdAsync(int id);
        Task<int> CreateAsync(TripCreateRequest request);
        Task UpdateAsync(int id, TripUpdateRequest request);
        Task DeleteAsync(int id);
        Task<NextTripNumberResponse> GetNextTripNumberAsync(DateOnly date);
    }
}
