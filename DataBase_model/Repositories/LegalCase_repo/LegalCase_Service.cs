using DataAccess.Data;
using DataAccess.Repositories;
using Law_Model.Models;

namespace DATA.Repositories.LegalCase_repo
{
    internal class LegalCase_Service : Repository<LegalCase>, ILegalCase_Service
    {
        private readonly AplicationDB _db;

        public LegalCase_Service(AplicationDB dB) : base(dB)
        {
            _db = dB;

        }
        public async Task SaveAsyc(LegalCase model)
        {

            await _db.SaveChangesAsync();
        }

        public Task UpdateAsyc(LegalCase model)
        {
            throw new NotImplementedException();
        }
    }
}
