using DataAccess.Repositories;
using Law_Model.Models;

namespace DATA.Repositories.Document_repo
{
    public interface IDocument_Service : IRepository<Documented>
    {
        Task UpdateAsyc(Documented model);

        Task SaveAsyc(Documented model);
    }
}
