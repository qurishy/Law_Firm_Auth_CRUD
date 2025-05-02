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



        public async Task<LegalCase> GetCaseDocumentById(int id)
        {
            try
            {

                // Retrieve the LegalCase with its associated documents
                return await _db.LegalCases
                    .Include(c => c.Documents) // Include related Documents
                    .FirstOrDefaultAsync(c => c.Id == id); // Filter by caseId

            }
            catch (Exception)
            {
                return null;
            }
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
        
        
        
        public async Task<IEnumerable<LegalCase>> GetAllCasesByUserIdAsync(string userId)
        {
            return await _db.LegalCases
      .Include(a => a.AssignedLawyer)
      .Where(a => a.AssignedLawyer.UserId == userId)
      .ToListAsync();
            ;
        }



        public async Task UpdateAsyc(LegalCase model)
        {
            try
            {
                // Retrieve the existing LegalCase from the database
                var existingCase = await _db.LegalCases
                    .Include(c => c.Documents) // Include related Documents
                    .Include(c => c.Appointments) // Include related Appointments
                    .FirstOrDefaultAsync(c => c.Id == model.Id);

                if (existingCase == null)
                {
                    throw new ArgumentException($"No LegalCase found with ID {model.Id}.");
                }

                // Update the properties of the existing LegalCase
                existingCase.Title = model.Title;
                existingCase.Description = model.Description;
                existingCase.Type = model.Type;
                existingCase.Status = model.Status;
                existingCase.OpenDate = model.OpenDate;
                existingCase.CloseDate = model.CloseDate;


                // Update related collections if necessary
                if (model.Documents != null)
                {


                    // Add or update documents
                    foreach (var document in model.Documents)
                    {
                        var existingDocument = existingCase.Documents.FirstOrDefault(d => d.Id == document.Id);
                        if (existingDocument != null)
                        {
                            // Update existing document
                            existingDocument.Title = document.Title;
                            existingDocument.Description = document.Description;
                            existingDocument.FilePath = document.FilePath;
                            existingDocument.UploadDate = document.UploadDate;
                            existingDocument.UploadedById = document.UploadedById;
                            existingDocument.IsPrivate = document.IsPrivate;
                        }
                        else
                        {
                            // Add new document
                            existingCase.Documents.Add(document);
                        }
                    }
                }

                if (model.Appointments != null)
                {
                    // Add or update appointments
                    foreach (var appointment in model.Appointments)
                    {
                        var existingAppointment = existingCase.Appointments.FirstOrDefault(a => a.Id == appointment.Id);
                        if (existingAppointment != null)
                        {
                            // Update existing appointment
                            existingAppointment.Title = appointment.Title;
                            existingAppointment.ScheduledTime = appointment.ScheduledTime;
                            existingAppointment.Notes = appointment.Notes;
                        }
                        else
                        {
                            // Add new appointment
                            existingCase.Appointments.Add(appointment);
                        }
                    }
                }

                // Save changes to the database
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while updating the LegalCase.", ex);
            }
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
                        Description = filePath,
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
