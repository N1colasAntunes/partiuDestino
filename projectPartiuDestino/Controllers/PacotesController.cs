using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Bcpg;
using projectPartiuDestino.Models;
using System.Collections.Generic;

namespace projectPartiuDestino.Controllers
{
    public class PacotesController : Controller
    {
        // String de conexão (use a mesma que você usou no Admin)
        private string conexao = "server=localhost;database=bdpartiudestino;uid=root;pwd=12345678;";

        public IActionResult Index()
        {
            List<Pacotes> listaPacotes = new List<Pacotes>();

            using (MySqlConnection conn = new MySqlConnection(conexao))
            {
                conn.Open();
                // Query para buscar os pacotes e os dados do destino relacionado
                string sql = "SELECT id, destino_id, nome, descricao, tipo_viagem, duracao_dias, data_partida, data_retorno, preco_por_pessoa, vagas_disponiveis FROM pacotes";

                MySqlCommand cmd = new MySqlCommand(sql, conn);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
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
                            VagasDisponiveis = Convert.ToInt32(reader["vagas_disponiveis"])
                        });
                    }
                }
            }

            return View(listaPacotes);
        }
    }
}
