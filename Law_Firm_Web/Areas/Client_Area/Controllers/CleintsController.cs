using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Law_Firm_Web.Areas.Client_Area.Controllers
{
    [Area("Client_Area")]
    [Authorize(Policy = "ClientOnly")]
    public class CleintsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
