using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.Entity;
using DXWebApplication4.Models;

namespace DXWebApplication4.Repository
{
    public class EfRepository<T> : IRepository<T> where T : class
    {
        protected readonly QLBanHangEntities1 _context;
        protected readonly DbSet<T> _dbSet;
        public EfRepository(QLBanHangEntities1 context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }
        public virtual async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();
        public virtual async Task<T> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

        public virtual async Task AddAsync(T entity)
        {
            _dbSet.Add(entity);
            await SaveAsync();
        }
        public virtual async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await SaveAsync();
        }
        public virtual async Task SaveAsync() => await _context.SaveChangesAsync();
    }
}