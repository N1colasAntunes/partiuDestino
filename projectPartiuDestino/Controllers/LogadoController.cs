using Microsoft.AspNetCore.Mvc;

namespace projectPartiuDestino.Controllers
{
    public class LogadoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}