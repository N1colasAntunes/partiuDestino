using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using projectPartiuDestino.Autenticacao;

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

                string sql = @"SELECT id, nome, email, tipo
                               FROM usuarios
                               WHERE email = @Email
                               AND senha = @Senha";

                MySqlCommand cmd = new MySqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Senha", senha);

                MySqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    HttpContext.Session.SetInt32(
                        SessionKeys.UserId,
                        Convert.ToInt32(reader["id"])
                    );

                    HttpContext.Session.SetString(
                        SessionKeys.UserName,
                        reader["nome"].ToString()
                    );

                    HttpContext.Session.SetString(
                        SessionKeys.UserEmail,
                        reader["email"].ToString()
                    );

                    HttpContext.Session.SetString(
                        SessionKeys.UserRole,
                        reader["tipo"].ToString()
                    );

                    string tipo = reader["tipo"].ToString();

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

            ViewBag.MensagemErro = "E-mail ou senha incorretos.";

            return View("Index");
        }
    }
}