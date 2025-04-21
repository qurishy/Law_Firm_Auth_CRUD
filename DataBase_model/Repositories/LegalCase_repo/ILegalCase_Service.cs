using DataAccess.Repositories;
using Law_Model.Models;

namespace DATA.Repositories.LegalCase_repo
{
    public interface ILegalCase_Service : IRepository<LegalCase>
    {
        Task UpdateAsyc(LegalCase model);

        Task SaveAsyc(LegalCase model);
    }
}
