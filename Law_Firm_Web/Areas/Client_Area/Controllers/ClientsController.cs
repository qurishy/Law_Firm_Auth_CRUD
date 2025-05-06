using DATA.Repositories.Appointment_repo;
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
    public class ClientsController(ILegalCase_Service caseService, ILawyer_Service lawyerService, IClient_Service clientService, IAppointment_Service appointmentService) : Controller
    {

        private readonly ILegalCase_Service _caseService = caseService;
        private readonly ILawyer_Service _lawyerService = lawyerService;
        private readonly IClient_Service _clientService = clientService;
        private readonly IAppointment_Service _appointmentService = appointmentService;





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

        // Function to show all appointments of the user
        public async Task<IActionResult> MyAppointments()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            IEnumerable<Appointment> appointments = await _appointmentService.GetAllAppointmentsByUserIdClient(userId);

            if (appointments == null || !appointments.Any())
            {
                return View();
            }

            return View(appointments);


        }




        //===============================================================================
        //This is the get part of the create appointment page
        [HttpGet]
        public async Task<IActionResult> CreateAppointment(int caseId)
        {

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (caseId != null)
            {
                var appoint = await _appointmentService.Get(x => x.CaseId == caseId);

                if (appoint == null)
                {
                    if (appoint.ScheduledTime <= DateTime.Now)
                    {

                        ViewBag.CaseId = caseId;
                        return View();



                    }
                    else
                    {
                        return RedirectToAction("MyAppointments");


                    }

                }
                ViewBag.CaseId = caseId;
                return View();
            }
            return RedirectToAction("MyCases");
        }

        //This is the post part of the create appointment page
        [HttpPost]
        public async Task<IActionResult> CreateAppointment(Appointment model, int caseId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if(model.ScheduledTime <= DateTime.Now)
            {
                return RedirectToAction("CreateAppointment" , new { caseId = caseId });
            }


            if (caseId != null&& model != null)
            {
                Appointment apointment = new Appointment
                {
                    CaseId = caseId,
                    Title = model.Title,
                    Notes = model.Notes,
                    ScheduledTime = model.ScheduledTime

                };

                await _appointmentService.CreateAppointment(apointment);

                return RedirectToAction("MyAppointments");
            }

            var result = await _caseService.Get(x => x.Id == caseId);

            return View(result);



        }


        //==========================================================================================

        //This is the get part of the create case page
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
        //this is the post part of the create case page
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



        }


        //=============================================================================================

        //we are deleting the appointment using by using the id
      
        public async Task<IActionResult> DeleteAppointment(int Id)
        {
          
            bool result =  await _appointmentService.DeleteAppointmentAsync(Id);

            if (result)
            { 
                return RedirectToAction("MyCases");
              
            }

             return NotFound();
        }

    }
}
