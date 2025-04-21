using Microsoft.AspNetCore.Mvc;

namespace Law_Firm_Web.Areas.Admin_Area.Controllers
{
    [Area("AdminOnly")]
    public class AdminsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
