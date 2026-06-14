using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using projectPartiuDestino.Models;

namespace projectPartiuDestino.Controllers
{
    public class DestinosController : Controller
    {
        private string conexao = "server=localhost;database=bdpartiudestino;uid=root;pwd=12345678;";

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
                        Pais = reader["pais"].ToString(),
                        Estado = reader["estado"].ToString(),
                        OrigemPais = reader["origem_pais"].ToString(),
                        OrigemEstado = reader["origem_estado"].ToString(),
                        ImagemUrl = reader["imagem_url"]?.ToString() ?? "",
                        PrecoPorPessoa = Convert.ToDecimal(reader["preco_por_pessoa"])  // ADICIONADO
                    });
                }
            }

            return View(listaDestinos);
        }

        public IActionResult Detalhes(int id)
        {
            return View();
        }
        public IActionResult Buscar(string termo)
        {
            string conexao =
                "server=localhost;database=bdpartiudestino;uid=root;pwd=12345678;";

            List<Destinos> destinos = new();

            using (MySqlConnection conn = new MySqlConnection(conexao))
            {
                conn.Open();

                string sql = @"
                    SELECT *
                    FROM destinos
                    WHERE pais LIKE @termo
                       OR estado LIKE @termo";

                MySqlCommand cmd = new(sql, conn);

                cmd.Parameters.AddWithValue(
                    "@termo",
                    "%" + termo + "%");

                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    destinos.Add(new Destinos
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        Pais = reader["pais"].ToString(),
                        Estado = reader["estado"].ToString(),
                        OrigemPais = reader["origem_pais"].ToString(),
                        OrigemEstado = reader["origem_estado"].ToString(),
                        ImagemUrl = reader["imagem_url"]?.ToString() ?? "",
                        PrecoPorPessoa = Convert.ToDecimal(reader["preco_por_pessoa"])
                    });
                }
            }

            return PartialView("_ResultadosBusca", destinos);
        }
    }
}