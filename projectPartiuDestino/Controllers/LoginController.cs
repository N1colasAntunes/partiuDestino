using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using projectPartiuDestino.Data;

namespace projectPartiuDestino.Controllers
{
    public class LoginController : Controller
    {
        private string conexao = "server=localhost;database=bdpartiudestino;uid=root;pwd=nicolas123;";

        [HttpPost]
        public IActionResult Entrar(string email, string senha)
        {
            using (MySqlConnection conn = new MySqlConnection(conexao))
            {
                conn.Open();

                string sql = "SELECT COUNT(*) FROM usuarios WHERE email = @Email AND senha = @Senha";

                MySqlCommand cmd = new MySqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Senha", senha);

                int usuarioExiste = Convert.ToInt32(cmd.ExecuteScalar());

                if (usuarioExiste > 0)
                {
                    return Content("Login realizado com sucesso");
                }
            }

            return Content("Email ou senha inválidos");
        }
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
