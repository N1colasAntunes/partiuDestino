using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using projectPartiuDestino.Models;

namespace projectPartiuDestino.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private string conexao = "server=localhost;database=bdpartiudestino;uid=root;pwd=12345678;";

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            List<Destinos> listaDestinos = new List<Destinos>();

            using (MySqlConnection conn = new MySqlConnection(conexao))
            {
                conn.Open();

                string sql = "SELECT * FROM destinos";

                MySqlCommand cmd = new MySqlCommand(sql, conn);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        listaDestinos.Add(new Destinos
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            OrigemPais = reader["origem_pais"].ToString(),
                            OrigemEstado = reader["origem_estado"].ToString(),
                            Pais = reader["pais"].ToString(),
                            Estado = reader["estado"].ToString()
                        });
                    }
                }
            }

            return View(listaDestinos);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}