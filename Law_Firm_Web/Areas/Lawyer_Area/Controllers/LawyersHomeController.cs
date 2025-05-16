using DATA.Repositories.Appointment_repo;
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
    public class LawyersHomeController(ILawyer_Service lawyer_service, ILegalCase_Service legalcase_service, IAppointment_Service appointment_Service) : Controller
    {
        private readonly ILawyer_Service _lawyer_service = lawyer_service;
        private readonly ILegalCase_Service _legalcase_service = legalcase_service;
        private readonly IAppointment_Service _appointment_service = appointment_Service;

      



        // Function to show the lawyer's profile
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

            if (assignedCases != null)
            {
                assignedCases = assignedCases.OrderBy(c => c.OpenDate).ToList();
                assignedCases = assignedCases.Where(c => c.Status != CaseStatus.Closed && c.Status != CaseStatus.Archived).ToList(); 
                
                return View(assignedCases);
            }

            return View();

           
        }



        // Function to show all appointments for the lawyer
        [HttpGet]
        public async Task<IActionResult> Appointments()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            IEnumerable<Appointment> appointments = await _appointment_service.GetAllAppointmentsByUserIdLawyer(userId);

            if (appointments != null)
            {

                // Filter appointments to only include those with ScheduledTime in the future
                var upcomingAppointments = appointments.Where(a => a.ScheduledTime >= DateTime.Now).ToList();

                upcomingAppointments = upcomingAppointments.OrderBy(a => a.ScheduledTime).ToList();
                upcomingAppointments = upcomingAppointments.Where(a => a.IsCompleted == null).ToList();



                return View(upcomingAppointments); // Pass the filtered list to the view
            }

            return View();

            //return NotFound();
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

            var legalCase = await _legalcase_service.GetCaseDocumentById(caseId);

            if (legalCase == null)
            {
                return NotFound();
            }

            return View(legalCase);
        }

        // Function to show history of appointments and cases for the lawyer

        [HttpGet]
        public async Task<IActionResult> HistoryOfAppointments()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();

            }
            // Create a new instance of ModelView to hold the combined data

            ModelView combinedModel = new ModelView();

            // Get the lawyer's information

            var legalCase = await _legalcase_service.GetAllCasesByUserIdAsync(userId);

            if (legalCase == null)
            {
                return NotFound();
            }

            // Filter the legal cases to only include those with the specified caseId
            legalCase = legalCase.Where(c => c.Status == CaseStatus.Closed || c.Status == CaseStatus.Archived);
            legalCase = legalCase.OrderBy(c => c.OpenDate).ToList();

            combinedModel.LegalCase = legalCase.ToList();
            

            var appointments = await _appointment_service.GetAllAppointmentsByUserIdLawyer(userId);

            if (appointments == null)
            {
                return NotFound();
            }
            // Filter the appointments to only include those with the passed appointmentId
           // appointments = appointments.Where(a => a.IsCompleted != null);
            appointments = appointments.OrderBy(a => a.ScheduledTime).ToList();
            //appointments = appointments.Where(a => a.ScheduledTime <= DateTime.Now).ToList();


            combinedModel.Appointment = appointments.ToList();


            return View(combinedModel);

        }




        //================================================================================================================================================
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

            if ( files.Count != 0)
            {
                // Upload documents
            
                await _legalcase_service.UploadDocsAsync(files, caseId, userId);
            }

            // if the case is cloesed we have to give the closing date
            if (newStatus == CaseStatus.Closed)
            {
                legalCase.CloseDate = DateTime.Now;
            }
           

            // Update case status
            legalCase.Status = newStatus;
            await _legalcase_service.UpdateAsyc(legalCase);

            return RedirectToAction("AssignedCases");
        }


        // Function to Accept an Appointment or reject it
        public async Task<IActionResult> AcceptAppointment(int id , bool decision)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (id == 0)
            {
                return RedirectToAction("Appointments");
            }



            var appointment = await _appointment_service.AppointmentByCaseId(id);

            if (appointment == null)
            {
                return NotFound();
            }

            if (decision)
            {
                appointment.IsCompleted = true;
                await _appointment_service.update(appointment);
            }
            else
            {
                appointment.IsCompleted = false;
                await _appointment_service.update(appointment);
            }
            return RedirectToAction("Appointments");
        }

    }
}
