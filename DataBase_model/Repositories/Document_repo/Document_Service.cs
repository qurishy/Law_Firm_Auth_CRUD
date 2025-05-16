using DataAccess.Data;
using DataAccess.Repositories;
using Law_Model.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DATA.Repositories.Document_repo
{
    public class Document_Service : Repository<Documented>, IDocument_Service
    {
        private readonly AplicationDB _db;

        public Document_Service(AplicationDB dB) : base(dB)
        {
            _db = dB;

        }

        public async Task<IEnumerable<Documented>> GetAllDocumentBYUserIdAsync(string userId)
        {

            try
            {
                var documents =await _db.Documents.Where(d => d.UploadedById == userId)
                    .Include(d => d.UploadedBy)
                    .ToListAsync();



                return (documents);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error retrieving documents: " + ex.Message);
                return null;
            }

        }

        public async Task SaveAsyc(Documented model)
        {
            await _db.SaveChangesAsync();
        }

        public Task UpdateAsyc(Documented model)
        {
            throw new NotImplementedException();
        }

        public async Task UploadDocsAsync(List<IFormFile> files, int caseId, string userId)
        {

            if (files == null || files.Count == 0)
            {
                throw new ArgumentException("No files provided for upload.");
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
                }
            }

            // Add the documents to the database
            await _db.Documents.AddRangeAsync(documentedList);
            await _db.SaveChangesAsync();

        }
    }
}
