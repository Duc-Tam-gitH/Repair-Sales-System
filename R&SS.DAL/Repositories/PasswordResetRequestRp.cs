using Microsoft.EntityFrameworkCore;
using R_SS.DAL.Data;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories;

public class PasswordResetRequestRp : GenericRp<PasswordResetRequest>, IPasswordResetRequestRp
{
    public PasswordResetRequestRp(AppDbContext context) : base(context)
    {
    }

    public async Task<PasswordResetRequest?> GetByUserIdAsync(int userId)
    {
        return await _context.PasswordResetRequests
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.UserId == userId);
    }
}
