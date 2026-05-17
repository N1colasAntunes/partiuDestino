using Microsoft.AspNetCore.Mvc;

namespace projectPartiuDestino.Controllers
{
    public class DestinosController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
