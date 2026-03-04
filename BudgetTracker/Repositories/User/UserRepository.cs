using BudgetTracker.Data;
using BudgetTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Repositories.UserRepository
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(BudgettrackerdbContext context) : base(context) { }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _dbSet.AsNoTracking().FirstOrDefaultAsync(u => u.UserEmail == email);
        }

        public async Task<User?> GetUserByNameAsync(string name)
        {
            return await _dbSet.AsNoTracking().FirstOrDefaultAsync(u => u.UserName == name);
        }
    }
}
