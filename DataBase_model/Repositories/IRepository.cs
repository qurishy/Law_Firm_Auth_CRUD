using System.Linq.Expressions;

namespace DataAccess.Repositories
{
    //In here we are creating a repository that we are going to use for other repositories, it is the base of them 
    public interface IRepository<T> where T : class
    {
        //T---> is Model in here 
        Task<IEnumerable<T>> GetAll();

        Task<T> Get(Expression<Func<T, bool>> filter);
        Task Add(T Entity);

        Task Delete(T Entity);

        Task DeleteRange(IEnumerable<T> entities);


    }
}
