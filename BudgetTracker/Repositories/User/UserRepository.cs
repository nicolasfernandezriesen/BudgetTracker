using BudgetTracker.Data;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Repositories.User
{
    public class UserRepository : Repository<BudgetTracker.Models.User>, IUserRepository
    {
        public UserRepository(BudgettrackerdbContext context) : base(context) { }

        public async Task<BudgetTracker.Models.User?> GetUserByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.UserEmail == email);
        }

        public async Task<BudgetTracker.Models.User?> GetUserByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.UserName == name);
        }
    }
}
