using DataAccess.Repositories;
using Law_Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATA.Repositories.Document_repo
{
    public interface IDocument_Service : IRepository<Documented>
    {
        Task UpdateAsyc(Documented model);

        Task SaveAsyc(Documented model);
    }
}
