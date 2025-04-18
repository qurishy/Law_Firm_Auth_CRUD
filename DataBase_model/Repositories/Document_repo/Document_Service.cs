using DataAccess.Data;
using DataAccess.Repositories;
using Law_Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DATA.Repositories.Document_repo
{
    public class DocumentService : Repository<Documented>, IDocument_Service
    {
        private readonly AplicationDB _db;

        public DocumentService(AplicationDB dB) : base(dB)
        {
            _db = dB;
            
        }


        public async Task SaveAsyc(Documented model)
        {
           await _db.SaveChangesAsync();
        }

        public Task UpdateAsyc(Documented model)
        {
            throw new NotImplementedException();
        }
    }
}
