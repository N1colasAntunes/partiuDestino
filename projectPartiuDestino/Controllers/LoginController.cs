using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using projectPartiuDestino.Data;

namespace projectPartiuDestino.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            Conexao conexao = new Conexao();

            using (MySqlConnection conn = conexao.ObterConexao())
            {
                conn.Open();
                Console.WriteLine("Conectado com sucesso!");
            }

            return View();
        }
    }
}
