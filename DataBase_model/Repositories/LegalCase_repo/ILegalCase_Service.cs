using DataAccess.Repositories;
using Law_Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATA.Repositories.LegalCase_repo
{
    public interface ILegalCase_Service : IRepository<LegalCase>
    {
        Task UpdateAsyc(LegalCase model);

        Task SaveAsyc(LegalCase model);
    }
}
