using Microsoft.AspNetCore.Mvc;

namespace Law_Firm_Web.Areas.Client_Area.Controllers
{
    [Area("Client_Area")]
    public class ClientsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
