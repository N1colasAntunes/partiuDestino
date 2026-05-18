using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using projectPartiuDestino.Models;

namespace projectPartiuDestino.Controllers
{
    public class CadastroController : Controller
    {
        private string conexao = "server=localhost;database=bdpartiudestino;uid=root;pwd=nicolas123;";

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Criar(Usuario usuario)
        {
            using (MySqlConnection conn = new MySqlConnection(conexao))
            {
                conn.Open();

                string verificar = "SELECT COUNT(*) FROM usuarios WHERE email = @Email";
                MySqlCommand cmdVerificar = new MySqlCommand(verificar, conn);
                cmdVerificar.Parameters.AddWithValue("@Email", usuario.Email);

                int existe = Convert.ToInt32(cmdVerificar.ExecuteScalar());

                if (existe > 0)
                {
                    return Content("Email já cadastrado");
                }

                string sql = "INSERT INTO usuarios(nome, email, senha) VALUES (@Nome, @Email, @Senha)";
                MySqlCommand cmd = new MySqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@Nome", usuario.Nome);
                cmd.Parameters.AddWithValue("@Email", usuario.Email);
                cmd.Parameters.AddWithValue("@Senha", usuario.Senha);

                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index", "Login");
        }
    }
}
