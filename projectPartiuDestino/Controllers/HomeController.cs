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

                string sql = @"SELECT id, origem_pais, origem_estado,
                                      pais, estado, imagem_url, preco_por_pessoa
                               FROM destinos";

                using MySqlCommand cmd = new MySqlCommand(sql, conn);
                using MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    listaDestinos.Add(new Destinos
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        OrigemPais = reader["origem_pais"].ToString(),
                        OrigemEstado = reader["origem_estado"].ToString(),
                        Pais = reader["pais"].ToString(),
                        Estado = reader["estado"].ToString(),
                        ImagemUrl = reader["imagem_url"]?.ToString() ?? "",
                        PrecoPorPessoa = Convert.ToDecimal(reader["preco_por_pessoa"])
                    });
                }
            }
            ViewBag.Origens = listaDestinos
                .Select(d => $"{d.OrigemPais} - {d.OrigemEstado}")
                .Distinct()
                .OrderBy(o => o)
                .ToList();
            ViewBag.Destinos = listaDestinos
                .Select(d => $"{d.Pais} - {d.Estado}")
                .Distinct()
                .OrderBy(d => d)
                .ToList();

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