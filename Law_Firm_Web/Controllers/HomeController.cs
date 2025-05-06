using DATA.Repositories.Lawyer_repo;
using Law_Firm_Web.Models;
using Law_Model.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Law_Firm_Web.Areas.Client_Area.Controllers
{
    //[Area("Client_Area")]
    // [Authorize(Policy = "ClientOnly")]

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ILawyer_Service _lawyer;

        public HomeController(ILogger<HomeController> logger, ILawyer_Service lawyer)
        {
            _logger = logger;
            _lawyer = lawyer;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Contacts()
        {
            return View();
        }
        public async Task<IActionResult> Attorneys()
        {
           IEnumerable<Personnel> lawyers = await _lawyer.GetAllLawyersAsync();

            if (lawyers != null)
            {
                return View(lawyers);
            }

            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
