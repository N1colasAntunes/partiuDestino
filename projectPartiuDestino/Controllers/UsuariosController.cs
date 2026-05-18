using Microsoft.AspNetCore.Mvc;

namespace projectPartiuDestino.Controllers
{
    public class UsuariosController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
