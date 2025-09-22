using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DriveList.Application.Common.Interfaces
{
    public interface IPredictionRepository
    {
        Task AddAsync(Prediction prediction);
        Task<IEnumerable<Prediction>> GetAllAsync();
        Task<Prediction?> GetByIdAsync(int id);
    }
}
