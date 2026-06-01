using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System;

namespace projectPartiuDestino.Controllers
{
    public class DestinosAdminController : Controller
    {
        private string conexao = "server=localhost;database=bdpartiudestino;uid=root;pwd=12345678;";

        public IActionResult Index()
        {
            return View();
        }

        // =========================
        // EXCLUIR DESTINO
        // =========================
        [HttpPost]
        public IActionResult Delete(int id)
        {
            using (MySqlConnection conn = new MySqlConnection(conexao))
            {
                conn.Open();

                string sql = "DELETE FROM destinos WHERE id = @id";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index");
        }
        public IActionResult Criar()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Criar(string origem_pais,
                           string origem_estado,
                           string pais,
                           string estado)
        {
            using (MySqlConnection conn = new MySqlConnection(conexao))
            {
                conn.Open();

                string sql = @"INSERT INTO destinos
                      (origem_pais, origem_estado, pais, estado)
                      VALUES
                      (@origem_pais, @origem_estado, @pais, @estado)";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@origem_pais", origem_pais);
                    cmd.Parameters.AddWithValue("@origem_estado", origem_estado);
                    cmd.Parameters.AddWithValue("@pais", pais);
                    cmd.Parameters.AddWithValue("@estado", estado);

                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index");
        }
        public IActionResult Editar(int id)
        {
            string origem_pais = "";
            string origem_estado = "";
            string pais = "";
            string estado = "";

            using (MySqlConnection conn = new MySqlConnection(conexao))
            {
                conn.Open();

                string sql = "SELECT * FROM destinos WHERE id = @id";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            origem_pais = reader["origem_pais"].ToString();
                            origem_estado = reader["origem_estado"].ToString();
                            pais = reader["pais"].ToString();
                            estado = reader["estado"].ToString();
                        }
                    }
                }
            }

            ViewBag.Id = id;
            ViewBag.OrigemPais = origem_pais;
            ViewBag.OrigemEstado = origem_estado;
            ViewBag.Pais = pais;
            ViewBag.Estado = estado;

            return View();
        }
        [HttpPost]
        public IActionResult Editar(
    int id,
    string origem_pais,
    string origem_estado,
    string pais,
    string estado)
        {
            using (MySqlConnection conn = new MySqlConnection(conexao))
            {
                conn.Open();

                string sql = @"
            UPDATE destinos
            SET
                origem_pais = @origem_pais,
                origem_estado = @origem_estado,
                pais = @pais,
                estado = @estado
            WHERE id = @id";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@origem_pais", origem_pais);
                    cmd.Parameters.AddWithValue("@origem_estado", origem_estado);
                    cmd.Parameters.AddWithValue("@pais", pais);
                    cmd.Parameters.AddWithValue("@estado", estado);

                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index");
        }
    }
}