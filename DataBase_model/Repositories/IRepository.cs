using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    //In here we are creating a repository that we are going to use for other repositories, it is the base of them 
    public interface IRepository< T> where T : class
    {
        //T---> is Model in here 
        IEnumerable<T> GetAll();

        T Get(Expression<Func<T, bool>> filter );
        void Add(T Entity);

        void Delete(T Entity);

        void DeleteRange(IEnumerable<T> entities);


    }
}
