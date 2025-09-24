using DriveList.Domain.Entities;

namespace DriveList.Application.Common.Interfaces
{
    public interface IPredictionRepository
    {
        Task AddAsync(Prediction prediction);
        Task<IEnumerable<Prediction>> GetAllAsync();
        Task<Prediction?> GetByIdAsync(int id);
    }
}
