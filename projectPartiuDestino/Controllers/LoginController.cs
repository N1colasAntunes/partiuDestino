using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using projectPartiuDestino.Autenticacao;
using BCrypt.Net;

namespace projectPartiuDestino.Controllers
{
    public class LoginController : Controller
    {
        private string conexao = "server=localhost;database=bdpartiudestino;uid=root;pwd=12345678;";

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult GerarHash()
        {
            return Content(BCrypt.Net.BCrypt.HashPassword("12345"));
        }

        [HttpPost]
        public IActionResult Entrar(string email, string senha)
        {
            using (MySqlConnection conn = new MySqlConnection(conexao))
            {
                conn.Open();

                string sql = @"SELECT id, nome, email, senha, tipo
                       FROM usuarios
                       WHERE email = @Email AND ativo = 1";

           MySqlCommand cmd = new MySqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@Email", email);

                MySqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    string senhaBanco = reader["senha"].ToString();

                    bool senhaCorreta =
                        BCrypt.Net.BCrypt.Verify(senha, senhaBanco);

                    if (senhaCorreta)
                    {
                        int id = Convert.ToInt32(reader["id"]);
                        string nome = reader["nome"].ToString();
                        string emailUsuario = reader["email"].ToString();
                        string tipo = reader["tipo"].ToString();

                        HttpContext.Session.SetInt32("UserId", id);
                        HttpContext.Session.SetString("UserName", nome);
                        HttpContext.Session.SetString("UserEmail", emailUsuario);
                        HttpContext.Session.SetString("UserRole", tipo);

                        if (tipo == "admin")
                        {
                            return RedirectToAction("Index", "Admin");
                        }

                        return RedirectToAction("Index", "Home");
                    }
                }
            }

            ViewBag.MensagemErro = "E-mail ou senha incorretos.";

            return View("Index");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }
    }
}