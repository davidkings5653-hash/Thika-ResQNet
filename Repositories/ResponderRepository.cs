using Microsoft.EntityFrameworkCore;
using ThikaResQNet.Data;
using ThikaResQNet.Models;

namespace ThikaResQNet.Repositories
{
    public class ResponderRepository : IResponderRepository
    {
        private readonly AppDbContext _context;

        public ResponderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Responder> AddAsync(Responder responder)
        {
            _context.Responders.Add(responder);
            await _context.SaveChangesAsync();
            return responder;
        }

        public async Task DeleteAsync(Responder responder)
        {
            _context.Responders.Remove(responder);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Responder>> GetAllAsync()
        {
            return await _context.Responders.AsNoTracking().ToListAsync();
        }

        public async Task<Responder?> GetByIdAsync(int id)
        {
            return await _context.Responders.FindAsync(id);
        }

        public async Task UpdateAsync(Responder responder)
        {
            _context.Responders.Update(responder);
            await _context.SaveChangesAsync();
        }
    }
}