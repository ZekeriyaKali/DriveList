using DriveList.Application.Common.Interfaces;
using DriveList.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DriveList.Infrastructure.Persistence.Repositories
{
    public class PredictionRepository: IPredictionRepository
    {
        private readonly AppDbContext _context;

        public PredictionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Prediction prediction)
        {
            await _context.Predictions.AddAsync(prediction);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Prediction>> GetAllAsync()
        {
            return await _context.Predictions.ToListAsync();
        }

        public async Task<Prediction> GetByIdAsync(int id)
        {
            return await _context.Predictions.FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
