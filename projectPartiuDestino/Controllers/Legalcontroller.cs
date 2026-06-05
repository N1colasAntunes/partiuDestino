using Microsoft.AspNetCore.Mvc;

namespace projectPartiuDestino.Controllers
{

    public class LegalController : Controller
    {

        public IActionResult TermosDeUso()
        {
            return View();
        }

        public IActionResult PoliticaDePrivacidade()
        {
            return View();
        }
    }
}