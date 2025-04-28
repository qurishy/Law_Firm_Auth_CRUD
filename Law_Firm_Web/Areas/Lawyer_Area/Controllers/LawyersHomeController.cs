using DATA.Repositories.Lawyer_repo;
using DATA.Repositories.LegalCase_repo;
using Law_Model.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Law_Model.Static_file.Static_datas;

namespace Law_Firm_Web.Areas.Lawyer_Area.Controllers
{
    [Area("Lawyer_Area")]
    [Authorize(policy: "LawerOnly")]
    public class LawyersHomeController(ILawyer_Service lawyer_service, ILegalCase_Service legalcase_service) : Controller
    {
        private readonly ILawyer_Service _lawyer_service=lawyer_service;
        private readonly ILegalCase_Service _legalcase_service=legalcase_service;

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var lawyer = await _lawyer_service.GetPersonnelUserByIdAsync(userId);

            if (lawyer == null)
            {
                return NotFound();
            }

            return View(lawyer);
        }

        // Function to show all LegalCases assigned to the lawyer
        [HttpGet]
        public async Task<IActionResult> AssignedCases()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var lawyer = await _lawyer_service.GetPersonnelUserByIdAsync(userId);

            if (lawyer == null)
            {
                return NotFound();
            }

            var assignedCases = lawyer.AssignedCases;

            return View(assignedCases);
        }


        // Function to show details of a specific LegalCase
        [HttpGet]
        public async Task<IActionResult> CaseDetails(int caseId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var legalCase = await _legalcase_service.Get(c => c.Id == caseId);

            if (legalCase == null)
            {
                return NotFound();
            }

            return View(legalCase);
        }


        // Function to add a document to an existing case and change the case status
        [HttpPost]
        public async Task<IActionResult> AddDocumentAndChangeStatus(int caseId, List<IFormFile> files, CaseStatus newStatus)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var legalCase = await _legalcase_service.Get(c => c.Id == caseId);

            if (legalCase == null)
            {
                return NotFound();
            }

            // Upload documents
            await _legalcase_service.UploadDocsAsync(files, caseId, userId);

            // Update case status
            legalCase.Status = newStatus;
            await _legalcase_service.UpdateAsyc(legalCase);

            // Save changes
            await _legalcase_service.SaveAsyc();

            return RedirectToAction("AssignedCases");
        }

    }
}
