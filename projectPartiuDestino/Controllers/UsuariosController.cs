using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace projectPartiuDestino.Controllers
{
    public class UsuariosController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Delete(int id)
        {
            string conexao = "server=localhost;database=bdpartiudestino;uid=root;pwd=12345678;";

            using (MySqlConnection conn = new MySqlConnection(conexao))
            {
                conn.Open();

                string sql = "DELETE FROM usuarios WHERE id = @id";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", id);

                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }
        public IActionResult Criar()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Criar(string nome, string email, string senha)
        {
            string conexao = "server=localhost;database=bdpartiudestino;uid=root;pwd=12345678;";

            using (MySqlConnection conn = new MySqlConnection(conexao))
            {
                conn.Open();

                string sql = @"
            INSERT INTO usuarios
            (
                nome,
                email,
                senha
            )
            VALUES
            (
                @nome,
                @email,
                @senha
            )";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@nome", nome);
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@senha", senha);

                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index");
        }
    }
}
