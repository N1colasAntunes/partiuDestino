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
    }
}