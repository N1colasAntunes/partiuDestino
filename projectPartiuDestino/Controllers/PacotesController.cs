using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using projectPartiuDestino.Models;

namespace projectPartiuDestino.Controllers
{
    public class PacotesController : Controller
    {
        private string conexao = "server=localhost;database=bdpartiudestino;uid=root;pwd=12345678;";

        public IActionResult Index()
        {
            List<Pacotes> listaPacotes = new List<Pacotes>();

            using (MySqlConnection conn = new MySqlConnection(conexao))
            {
                conn.Open();

                string sql = @"SELECT id, destino_id, nome, descricao, tipo_viagem,
                                      duracao_dias, data_partida, data_retorno,
                                      preco_por_pessoa, vagas_disponiveis, imagem_url
                               FROM pacotes";

                using MySqlCommand cmd = new MySqlCommand(sql, conn);
                using MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    listaPacotes.Add(new Pacotes
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        DestinoId = Convert.ToInt32(reader["destino_id"]),
                        Nome = reader["nome"].ToString(),
                        Descricao = reader["descricao"].ToString(),
                        TipoViagem = reader["tipo_viagem"].ToString(),
                        DuracaoDias = Convert.ToInt32(reader["duracao_dias"]),
                        DataPartida = Convert.ToDateTime(reader["data_partida"]),
                        DataRetorno = Convert.ToDateTime(reader["data_retorno"]),
                        PrecoPorPessoa = Convert.ToDecimal(reader["preco_por_pessoa"]),
                        VagasDisponiveis = Convert.ToInt32(reader["vagas_disponiveis"]),
                        ImagemUrl = reader["imagem_url"]?.ToString() ?? ""  // ADICIONADO
                    });
                }
            }

            return View(listaPacotes);
        }

        public IActionResult Detalhes(int id)
        {
            return View();
        }
        public IActionResult Buscar(string termo)
        {
            List<Pacotes> pacotes = new();

            using (MySqlConnection conn = new MySqlConnection(conexao))
            {
                conn.Open();

                string sql = @"
            SELECT *
            FROM pacotes
            WHERE nome LIKE @termo
               OR descricao LIKE @termo
               OR tipo_viagem LIKE @termo";

                MySqlCommand cmd = new(sql, conn);

                cmd.Parameters.AddWithValue(
                    "@termo",
                    "%" + termo + "%");

                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    pacotes.Add(new Pacotes
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        DestinoId = Convert.ToInt32(reader["destino_id"]),
                        Nome = reader["nome"].ToString(),
                        Descricao = reader["descricao"].ToString(),
                        TipoViagem = reader["tipo_viagem"].ToString(),
                        DuracaoDias = Convert.ToInt32(reader["duracao_dias"]),
                        DataPartida = Convert.ToDateTime(reader["data_partida"]),
                        DataRetorno = Convert.ToDateTime(reader["data_retorno"]),
                        PrecoPorPessoa = Convert.ToDecimal(reader["preco_por_pessoa"]),
                        VagasDisponiveis = Convert.ToInt32(reader["vagas_disponiveis"]),
                        ImagemUrl = reader["imagem_url"]?.ToString() ?? ""
                    });
                }
            }

            return PartialView("_ResultadosBuscaPacotes", pacotes);
        }
    }
}