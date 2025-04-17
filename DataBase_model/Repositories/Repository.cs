using DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

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
        public void Add(T Entity)
        {
            dbset.Add(Entity);
        }

        public void Delete(T Entity)
        {
            dbset.Remove(Entity);
        }

        public void DeleteRange(IEnumerable<T> entities)
        {
            dbset.RemoveRange(entities);
        }

        public T Get(Expression<Func<T, bool>> filter)
        {

            IQueryable<T> entit = dbset;

            entit = entit.Where(filter);

            return entit.FirstOrDefault();

        }

        public IEnumerable<T> GetAll()
        {
            IQueryable<T> values = dbset;

            return values.ToList();
        }
    }
}
