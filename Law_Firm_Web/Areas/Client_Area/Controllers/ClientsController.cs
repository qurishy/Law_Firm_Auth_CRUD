using DATA.Repositories.Client_repo;
using DATA.Repositories.Lawyer_repo;
using DATA.Repositories.LegalCase_repo;
using Law_Model.Models;
using Law_Model.Static_file;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Law_Firm_Web.Areas.Client_Area.Controllers
{
    [Area("Client_Area")]
    [Authorize(policy: "ClientOnly")]
    public class ClientsController(ILegalCase_Service caseService, ILawyer_Service lawyerService, IClient_Service clientService) : Controller
    {

        private readonly ILegalCase_Service _caseService = caseService;
        private readonly ILawyer_Service _lawyerService = lawyerService;
        private readonly IClient_Service _clientService = clientService;





        //this is the index page of client
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            Client client = await _clientService.GetClientUserByIdAsync(userId);


            return View(client);

        }


        // Function to show all cases of the user
        public async Task<IActionResult> MyCases()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var clien = await _clientService.GetClientUserByIdAsync(userId);

            if (clien == null)
            {
                return NotFound();
            }

            var assignedCases = clien.Cases.ToList();

         

            return View(assignedCases);
        }





        [HttpGet]
        public async Task<IActionResult> CreateCase()
        {
            try
            {
                var model = new LegalCase();
                var lawyers = await _lawyerService.GetAll();

                // Ensure lawyers is not null before passing to view
                ViewBag.Lawyers = lawyers.ToList() ?? new List<Personnel>();

                return View(model);
            }
            catch (Exception ex)
            {
                // Log error
              
                return View(new LegalCase());
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateCase(LegalCase model, int lawyerId, List<IFormFile> uploadedDocuments)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var client = await _clientService.GetClientUserByIdAsync(userId);
            
                // Create new case
                LegalCase newCase = new LegalCase
                {
                    ClientId = client.Id,
                    Client = client,
                    AssignedLawyerId = lawyerId,
                    Title = model.Title,
                    Description = model.Description,
                    Status = Static_datas.CaseStatus.New,
                    OpenDate = DateTime.UtcNow,
                    Type = model.Type
                };

                await _caseService.Add(newCase);
                await _caseService.SaveAsyc();

                if (uploadedDocuments != null && uploadedDocuments.Count > 0)
                {
                    await _caseService.UploadDocsAsync(uploadedDocuments, newCase.Id, userId);
                }

                return RedirectToAction("MyCases");
            

            // If we got this far, something failed - repopulate lawyers
            var lawyers = await _lawyerService.GetAll();
            ViewBag.Lawyers = lawyers;
            return View(model);
        }




        // Functionality to upload documents to a pending case
        [HttpPost]
        public async Task<IActionResult> UploadDocument(int caseId, IFormFile document)
        {
            if (document == null)
            {
                ModelState.AddModelError("", "Please select a valid document to upload.");
                return RedirectToAction("MyCases");
            }

            var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            try
            {
                // Define the upload path
                var uploadPath = Path.Combine("wwwroot/uploads", document.FileName);

                // Check if the file already exists
                if (System.IO.File.Exists(uploadPath))
                {
                    // If the file exists, associate it with the case
                    var documented = new Documented
                    {
                        Title = document.FileName,
                        FilePath = uploadPath,
                        Description = "Uploaded",
                        UploadDate = DateTime.UtcNow,
                        UploadedById = userId,
                        CaseId = caseId
                    };

                    // Retrieve the case and add the document
                    var legalCase = await _caseService.GetCaseByUserId(userId);

                    if (legalCase != null)
                    {
                        legalCase.Documents.Add(documented);
                        await _caseService.UpdateAsyc(legalCase);
                        await _caseService.SaveAsyc();
                    }

                    return RedirectToAction("MyCases");
                }
                else
                {
                    // If the file does not exist, save it
                    await _caseService.UploadDocsAsync(new List<IFormFile> { document }, caseId, userId);

                    return RedirectToAction("MyCases");
                }
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                // _logger.LogError(ex, "Error uploading document");
                ModelState.AddModelError("", "An error occurred while uploading the document. Please try again.");
            }

            // If upload fails, redirect with an error message
            ModelState.AddModelError("", "Failed to upload the document. Please try again.");
            return RedirectToAction("MyCases");
        }
    }
}
