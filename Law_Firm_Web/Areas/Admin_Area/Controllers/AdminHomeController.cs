using DATA.Repositories.Lawyer_repo;
using DataAccess.Data;
using Law_Model.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
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

        // Async method to display a specific Legal Case
        [HttpGet]
        public async Task<IActionResult> LegalCaseDetail(int id)
        {

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var legalCase = await _context.LegalCases
                .Include(l => l.Client) // Include Client
                .ThenInclude(c => c.User) // Include ApplicationUser for Client
                .Include(l => l.AssignedLawyer) // Include Assigned Lawyer (Personnel)
                .ThenInclude(p => p.User) // Include ApplicationUser for Personnel
                .Include(l => l.Documents) // Include Documents
                .FirstOrDefaultAsync(l => l.Id == id);

            if (legalCase == null)
            {
                return NotFound();
            }

            return View(legalCase);
        }

        //Delete Legal Case
        public async Task<IActionResult> DeleteLegalCase(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (id <= 0)
            {
                return BadRequest();
            }


            var legalCase = await _context.LegalCases
                .Include(l => l.Documents) // Include related documents
                .FirstOrDefaultAsync(l => l.Id == id);

            if (legalCase == null)
            {
                return NotFound();
            }

            // Remove related documents
            _context.Documents.RemoveRange(legalCase.Documents);

            // Remove the legal case itself
            _context.LegalCases.Remove(legalCase);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(AllLegalCase));
        }


        //===============================================================================================================================================================

        // Async method to display all Personnel
        [HttpGet]
        public async Task<IActionResult> AllPersonnel()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }


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

        // Async method to delete a specific Personnel
        public async Task<IActionResult> DeletePersonel(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (id <= 0)
            {
                return BadRequest();
            }


            var personel = await _context.Personnel
      .Include(c => c.User)
      .Include(c => c.AssignedCases) // Ensure related LegalCases are loaded
      .FirstOrDefaultAsync(c => c.Id == id);

            if (personel != null)
            {
                

                // Remove related LegalCases
                _context.LegalCases.RemoveRange(personel.AssignedCases);

                // Remove the client itself
                _context.Personnel.Remove(personel);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(AllPersonnel));
            }

            return NotFound();
        }
        //================================================================================================================================================================

        // Async method to display all Clients
        [HttpGet]
        public async Task<IActionResult> AllClients()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            IEnumerable<Client> clients = await _context.Clients
                .Include(c => c.User) // Include related ApplicationUser data
                .Include(c => c.Cases) // Include related LegalCases
                .ToListAsync();

            return View(clients);
        }

        // Async method to delete a specific Client

        public async Task<IActionResult> DeleteClient(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (id <= 0)
            {
                return BadRequest();
            }


            var client = await _context.Clients
                .Include(c => c.Cases) // Include related LegalCases
                .ThenInclude(c => c.Documents) // Include related Documents
                .Include(c => c.User) // Include related ApplicationUser data
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client != null)
            {
                // Remove related Documents
                _context.Documents.RemoveRange(client.Cases.SelectMany(c => c.Documents));

                // Remove related LegalCases
                _context.LegalCases.RemoveRange(client.Cases);

                // Remove the client itself
                _context.Clients.Remove(client);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(AllClients));
            }

            return NotFound();
        }


        //================================================================================================================================================================
        // Async method to display all Documents
        [HttpGet]
        public async Task<IActionResult> AllAppopintmetnts()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            IEnumerable<Appointment> appointments = await _context.Appointments
      .Include(a => a.Case)
          .ThenInclude(c => c.Client) // Include Client
              .ThenInclude(client => client.User) // Include ApplicationUser for Client
      .Include(a => a.Case.AssignedLawyer) // Include Assigned Lawyer
          .ThenInclude(lawyer => lawyer.User) // Include ApplicationUser for Lawyer
      .ToListAsync();


            if (appointments == null)
            {
                return NotFound();
            }
            appointments = appointments.OrderByDescending(a => a.ScheduledTime);

            return View(appointments);
        }




        // Async method to display all Documents
        [HttpGet]
        public async Task<IActionResult> AllDocuments()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            IEnumerable<Documented> documents = await _context.Documents
                .Include(d => d.Case)
                .Include(d => d.UploadedBy)
                .ToListAsync();

            documents = documents.OrderByDescending(d => d.UploadDate);

            return View(documents);



        }



    }

}
