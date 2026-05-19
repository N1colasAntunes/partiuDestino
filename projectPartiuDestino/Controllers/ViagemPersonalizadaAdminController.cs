using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace projectPartiuDestino.Controllers
{
    public class ViagemPersonalizadaAdminController : Controller
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

                string sql = "DELETE FROM viagem_personalizada WHERE id = @id";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", id);

                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }
    }
}
