using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using projectPartiuDestino.Models;

namespace projectPartiuDestino.Controllers
{
    public class DestinosController : Controller
    {
        private string conexao = "server=localhost;database=bdpartiudestino;uid=root;pwd=12345678;";

        public IActionResult Index(string origem, string destino)
        {

            List<Destinos> listaDestinos = new List<Destinos>();

            using (MySqlConnection conn = new MySqlConnection(conexao))
            {
                conn.Open();

                string sql = @"SELECT id, origem_pais, origem_estado,
                                   pais, estado, imagem_url, preco_por_pessoa
                                   FROM destinos
                                   WHERE 1=1";

                if (!string.IsNullOrEmpty(origem))
                {
                    sql += " AND CONCAT(origem_pais, ' - ', origem_estado) = @origem";
                }

                if (!string.IsNullOrEmpty(destino))
                {
                    sql += " AND CONCAT(pais, ' - ', estado) = @destino";
                }


                using MySqlCommand cmd = new MySqlCommand(sql, conn);
                if (!string.IsNullOrEmpty(origem))
                {
                    cmd.Parameters.AddWithValue("@origem", origem);
                }

                if (!string.IsNullOrEmpty(destino))
                {
                    cmd.Parameters.AddWithValue("@destino", destino);
                }
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
            Pacotes? pacote = null;

            using (MySqlConnection conn = new MySqlConnection(conexao))
            {
                conn.Open();

                // 1. Dados do pacote
                string sqlPacote = @"SELECT id, destino_id, nome, descricao, tipo_viagem,
                                     duracao_dias, data_partida, data_retorno,
                                     preco_por_pessoa, vagas_disponiveis, imagem_url
                              FROM pacotes WHERE id = @id";

                using (var cmd = new MySqlCommand(sqlPacote, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        pacote = new Pacotes
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
                        };
                    }
                }

                if (pacote == null)
                    return NotFound();

                // 2. Hospedagens do pacote
                string sqlHosp = "SELECT * FROM hospedagens WHERE pacote_id = @id";
                using (var cmdH = new MySqlCommand(sqlHosp, conn))
                {
                    cmdH.Parameters.AddWithValue("@id", id);
                    using var readerH = cmdH.ExecuteReader();
                    while (readerH.Read())
                    {
                        pacote.Hospedagens.Add(new Hospedagem
                        {
                            Id = Convert.ToInt32(readerH["id"]),
                            PacoteId = Convert.ToInt32(readerH["pacote_id"]),
                            Nome = readerH["nome"].ToString()!,
                            Categoria = readerH["categoria"]?.ToString(),
                            Descricao = readerH["descricao"]?.ToString(),
                            Endereco = readerH["endereco"]?.ToString(),
                            ImagemUrl = readerH["imagem_url"]?.ToString()
                        });
                    }
                }

                // 3. Quartos de cada hospedagem
                foreach (var hospedagem in pacote.Hospedagens)
                {
                    string sqlQuartos = "SELECT * FROM quartos WHERE hospedagem_id = @hid";
                    using var cmdQ = new MySqlCommand(sqlQuartos, conn);
                    cmdQ.Parameters.AddWithValue("@hid", hospedagem.Id);

                    using var readerQ = cmdQ.ExecuteReader();
                    while (readerQ.Read())
                    {
                        hospedagem.Quartos.Add(new Quarto
                        {
                            Id = Convert.ToInt32(readerQ["id"]),
                            HospedagemId = Convert.ToInt32(readerQ["hospedagem_id"]),
                            TipoQuarto = readerQ["tipo_quarto"].ToString()!,
                            CapacidadeAdultos = Convert.ToInt32(readerQ["capacidade_adultos"]),
                            CapacidadeCriancas = Convert.ToInt32(readerQ["capacidade_criancas"]),
                            PrecoAdicional = Convert.ToDecimal(readerQ["preco_adicional"]),
                            QuantidadeDisponivel = Convert.ToInt32(readerQ["quantidade_disponivel"]),
                            Comodidades = readerQ["comodidades"]?.ToString(),
                            ImagemUrl = readerQ["imagem_url"]?.ToString()
                        });
                    }
                }
            }

            return View(pacote);
        }

        public IActionResult DetalhesPassagens(int id)
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