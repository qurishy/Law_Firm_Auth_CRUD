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
    public class AdminHomeController(AplicationDB context, ILawyer_Service lawyer_service) : Controller
    {
        private readonly AplicationDB _context = context;
        private readonly ILawyer_Service _lawyer_service = lawyer_service;

       

        // Async method to display all lawyers
        public async Task<IActionResult> Index()
        {
            return View();
        }



        // Async method to display all Legal Cases
        [HttpGet]
        public async Task<IActionResult> AllLegalCase()
        {
            IEnumerable<LegalCase> legalCases = await _context.LegalCases
     .Include(l => l.Client) // Include Client
         .ThenInclude(c => c.User) // Include ApplicationUser for Client
     .Include(l => l.AssignedLawyer) // Include Assigned Lawyer (Personnel)
         .ThenInclude(p => p.User) // Include ApplicationUser for Personnel
     .ToListAsync();


            return View(legalCases);

        }





        // Async method to display all Personnel
        [HttpGet]
        public async Task<IActionResult> AllPersonnel()
        {
            var personnelWithCaseCounts = await _context.Personnel
        
                .Include(p => p.User) // Include related ApplicationUser data
                .Select(p => new
                {
                    Personnel = p,
                    AssignedCaseCount = p.AssignedCases.Count // Count of assigned LegalCases
                })
                 .ToListAsync();
            
            ViewData["PersonnelWithCaseCounts"] = personnelWithCaseCounts;
            return View();

            //return View(personnelWithCaseCounts);

        }



        // Async method to display all Clients
        [HttpGet]
        public async Task<IActionResult> AllClients()
        {
            IEnumerable<Client> clients = await _context.Clients
                .Include(c => c.User) // Include related ApplicationUser data
                .Include(c => c.Cases) // Include related LegalCases
                .ToListAsync();

            return View(clients);
        }







    }
}
