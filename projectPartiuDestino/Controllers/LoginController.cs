using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using projectPartiuDestino.Data;

namespace projectPartiuDestino.Controllers
{
    public class LoginController : Controller
    {
        private string conexao = "server=localhost;database=bdpartiudestino;uid=root;pwd=nicolas123;";

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Entrar(string email, string senha)
        {
            using (MySqlConnection conn = new MySqlConnection(conexao))
            {
                conn.Open();

                string sql = "SELECT tipo FROM usuarios WHERE email = @Email AND senha = @Senha";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Senha", senha);

                object resultado = cmd.ExecuteScalar();

                if (resultado != null)
                {
                    string tipo = resultado.ToString();

                    if (tipo == "admin")
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
            }

            return Content("Email ou senha inválidos");
        }
    }
}
