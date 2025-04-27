using DataAccess.Repositories;
using Law_Model.Models;
using Microsoft.AspNetCore.Http;

namespace DATA.Repositories.Document_repo
{
    public interface IDocument_Service : IRepository<Documented>
    {
        Task UploadDocsAsync(List<IFormFile> files, int caseId, string userId);
        Task UpdateAsyc(Documented model);

        Task SaveAsyc(Documented model);
    }
}
