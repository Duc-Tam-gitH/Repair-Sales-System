using Microsoft.EntityFrameworkCore;
using R_SS.DAL.Data;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.Models.Entities;

namespace R_SS.DAL.Repositories
{
    public class UserRp : GenericRp<User>, IUserRp
    {
        public UserRp(AppDbContext context) : base(context)
        {
        }

        // 1. Tìm User theo tên tài khoản (Không phân biệt chữ hoa/thường)
        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
        }

        // 2. Tìm User theo Email (Không phân biệt chữ hoa/thường)
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<User?> GetByIdentifierAsync(string identifier)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Username.ToLower() == identifier.ToLower() ||
                    u.Email.ToLower() == identifier.ToLower());
        }

        // 3. Kiểm tra xem tên tài khoản đã có người dùng chưa
        public async Task<bool> ExistsUsernameAsync(string username)
        {
            return await _context.Users
                .AnyAsync(u => u.Username.ToLower() == username.ToLower());
        }

        // 4. Kiểm tra xem Email đã có người dùng đăng ký chưa
        public async Task<bool> ExistsEmailAsync(string email)
        {
            return await _context.Users
                .AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }
    }
}
