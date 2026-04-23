using Microsoft.AspNetCore.Mvc;

namespace projectPartiuDestino.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
