using DataAccess.Data;
using DataAccess.Repositories;
using Law_Model.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DATA.Repositories.LegalCase_repo
{
    public class LegalCase_Service : Repository<LegalCase>, ILegalCase_Service
    {
        private readonly AplicationDB _db;

        public LegalCase_Service(AplicationDB dB) : base(dB)
        {
            _db = dB;

        }





        public async Task<LegalCase> GetCaseByUserId(string id)
        {
            try
            {

                // Assuming the UserId is stored in the Client or AssignedLawyer property
                return await _db.LegalCases
                    .Include(c => c.Client) // Include Client to access UserId
                    .Where(x => x.Client != null && x.Client.UserId == id)
                    .FirstOrDefaultAsync();

            }
            catch (Exception)
            {
                return null;
            }
        }



        public Task UpdateAsyc(LegalCase model)
        {
            throw new NotImplementedException();
        }



        public async Task SaveAsyc()
        {

            await _db.SaveChangesAsync();
        }

        public async Task UploadDocsAsync(List<IFormFile> files, int caseId, string userId)
        {

            if (files == null || files.Count == 0)
            {
                throw new ArgumentException("No files provided for upload.");
            }

            // Retrieve the LegalCase by caseId
            var legalCase = await _db.LegalCases
                .Include(c => c.Documents) // Include existing documents
                .FirstOrDefaultAsync(c => c.Id == caseId);

            if (legalCase == null)
            {
                throw new ArgumentException($"No LegalCase found with ID {caseId}.");
            }

            var documentedList = new List<Documented>();

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    // Save the file to the file system
                    var uploadsFolder = Path.Combine("wwwroot/uploads");
                    Directory.CreateDirectory(uploadsFolder); // Ensure the directory exists
                    var filePath = Path.Combine(uploadsFolder, file.FileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Create a Documented object
                    var documented = new Documented
                    {
                        Title = file.FileName,

                        FilePath = filePath,
                        UploadDate = DateTime.UtcNow,
                        UploadedById = userId,
                        CaseId = caseId
                    };

                    documentedList.Add(documented);

                    // Add the document to the LegalCase's Documents collection
                    legalCase.Documents.Add(documented);
                }
            }

            // Add the documents to the database
            await _db.Documents.AddRangeAsync(documentedList);

            // Save changes to the LegalCase and Documents
            await _db.SaveChangesAsync();

        }
    }
}
