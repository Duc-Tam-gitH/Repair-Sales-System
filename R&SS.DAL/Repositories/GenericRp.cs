using Microsoft.EntityFrameworkCore;
using R_SS.DAL.Data;
using R_SS.DAL.Repositories.Interfaces;
using System.Linq.Expressions;

namespace R_SS.DAL.Repositories
{
    public class GenericRp<T> : IGenericRp<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRp(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T?> GetByIdAsync(object id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<T?> GetByNameAsync(string name)
        {
            var entityType = typeof(T);
            var candidateProperties = new[]
            {
                "Name",
                "RoleName",
                "Username",
                "FullName",
                "CategoryName",
                "ProductName",
                "SupplierName"
            };

            var propertyName = candidateProperties
                .FirstOrDefault(p => entityType.GetProperty(p) != null);

            if (propertyName == null)
                throw new NotSupportedException($"Entity type '{entityType.Name}' does not expose a supported name property.");

            return await _dbSet.FirstOrDefaultAsync(entity =>
        EF.Property<string>(entity, propertyName).ToLower() == name.ToLower());
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }
    }
}
