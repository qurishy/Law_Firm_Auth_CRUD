using DataAccess.Repositories;
using Law_Model.Models;
using Microsoft.AspNetCore.Http;

namespace DATA.Repositories.LegalCase_repo
{
    public interface ILegalCase_Service : IRepository<LegalCase>
    {
        Task UploadDocsAsync(List<IFormFile> files, int caseId, string userId);

        Task<IEnumerable<LegalCase>> GetAllCasesByUserIdAsync(string userId);

        Task<LegalCase> GetCaseByUserId(string id);

        Task<LegalCase> GetCaseDocumentById(int id);
        Task UpdateAsyc(LegalCase model);



        Task SaveAsyc();
    }
}
