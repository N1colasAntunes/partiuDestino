using Microsoft.AspNetCore.Mvc;

namespace projectPartiuDestino.Controllers
{
    public class DestinosAdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
