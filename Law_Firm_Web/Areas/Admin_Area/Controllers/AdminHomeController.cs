using DATA.Repositories.Lawyer_repo;
using DataAccess.Data;
using Law_Model.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Law_Model.Static_file.Static_datas;

namespace Law_Firm_Web.Areas.Admin_Area.Controllers
{
    [Area("Admin_Area")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminHomeController : Controller
    {
        private readonly AplicationDB _context;
        private readonly ILawyer_Service _lawyer_service;

        public AdminHomeController(AplicationDB context, ILawyer_Service lawyer_service)
        {
            _context = context;
            _lawyer_service = lawyer_service;
        }

        // Async method to display all lawyers
        public async Task<IActionResult> Index()
        {
            var lawyers = await _context.Personnel
                .Where(p => p.Position == "Lawyer")
                .Include(p => p.User) // Include related ApplicationUser data
                .ToListAsync();

            return View(lawyers);
        }

        // Async method to create a new Personnel
        [HttpPost]
        public async Task<IActionResult> CreatePersonnel(Personnel personnel)
        {
            if (ModelState.IsValid)
            {
                _context.Personnel.Add(personnel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(personnel);
        }

        // Async method to assign a LegalCase to a Personnel and update CaseType
        [HttpPost]
        public async Task<IActionResult> AssignCase(int caseId, int personnelId, CaseType newCaseType)
        {
            var legalCase = await _context.LegalCases.FindAsync(caseId);
            var personnel = await _context.Personnel.FindAsync(personnelId);

            if (legalCase == null || personnel == null)
            {
                return NotFound();
            }

            legalCase.AssignedLawyerId = personnelId;
            legalCase.Type = newCaseType;

            _context.LegalCases.Update(legalCase);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
