using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using BCrypt.Net;
using projectPartiuDestino.Data;

namespace projectPartiuDestino.Controllers
{
    public class UsuariosController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }


        // -------------------------------------------------------
        // DESATIVAR — substitui o antigo Delete (exclusão física)
        // Apenas marca ativo = 0, nunca apaga o registro
        // -------------------------------------------------------
        [HttpPost]
        public IActionResult Desativar(int id)
        {
            string conexao = "server=localhost;database=bdpartiudestino;uid=root;pwd=12345678;";

            using (MySqlConnection conn = new MySqlConnection(conexao))
            {
                conn.Open();

                string sql = "UPDATE usuarios SET ativo = 0 WHERE id = @id";

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
                    cmd.Parameters.AddWithValue("@senha", BCrypt.Net.BCrypt.HashPassword(senha));

                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index");
        }
        public IActionResult Editar(int id)
        {
            string conexao = "server=localhost;database=bdpartiudestino;uid=root;pwd=12345678;";

            using (MySqlConnection conn = new MySqlConnection(conexao))
            {
                conn.Open();

                string sql = "SELECT * FROM usuarios WHERE id = @id";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            ViewBag.Id = reader["id"];
                            ViewBag.Nome = reader["nome"];
                            ViewBag.Tipo = reader["tipo"];
                        }
                    }
                }
            }

            return View();
        }
        [HttpPost]
        public IActionResult Editar(int id, string nome, string tipo)
        {
            string conexao = "server=localhost;database=bdpartiudestino;uid=root;pwd=12345678;";

            using (MySqlConnection conn = new MySqlConnection(conexao))
            {
                conn.Open();

                string sql = @"
            UPDATE usuarios
            SET
                nome = @nome,
                tipo = @tipo
            WHERE id = @id";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@nome", nome);
                    cmd.Parameters.AddWithValue("@tipo", tipo);

                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index");
        }
    }
}
