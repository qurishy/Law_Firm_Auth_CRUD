using DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccess.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly AplicationDB _context;
        internal DbSet<T> dbset;

        public Repository(AplicationDB dB)
        {
            _context = dB;
            this.dbset=_context.Set<T>();
            //_context."your model another entity" = dbset;
        }
        public async Task Add(T Entity)
        {
            dbset.AddAsync(Entity);
        }

        public async Task Delete(T Entity)
        {
            dbset.Remove(Entity);
        }

        public async Task DeleteRange(IEnumerable<T> entities)
        {
            dbset.RemoveRange(entities);
        }

        public Task<T> Get(Expression<Func<T, bool>> filter)
        {

            IQueryable<T> entit = dbset;

            entit = entit.Where(filter);

            return entit.FirstOrDefaultAsync();

        }

        public async Task<IEnumerable<T>> GetAll()
        {
            IQueryable<T> values = dbset;

            return values.ToList();
        }
    }
}
