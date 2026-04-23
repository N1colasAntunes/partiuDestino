using Microsoft.AspNetCore.Mvc;

namespace projectPartiuDestino.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        
        // cad
        public IActionResult Cadastro()
        {
            return RedirectToAction("Index", "Cadastro");
        }
    }
}
