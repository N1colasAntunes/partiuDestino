using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using projectPartiuDestino.Models;
using System.Text.Json;

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

                // ADICIONADO: exige que a Passagem tenha sido escolhida antes da Hospedagem
                string? vooJson = HttpContext.Session.GetString($"Voo_pacote_{id}");
                if (string.IsNullOrEmpty(vooJson))
                {
                    TempData["Erro"] = "Selecione primeiro sua passagem para continuar.";
                    return RedirectToAction("Passagem", new { id });
                }
                ViewBag.Voo = JsonSerializer.Deserialize<SelecaoVoo>(vooJson);

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

        // GET: /Pacotes/Passagem/5
        public IActionResult Passagem(int id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Index", "Login");

            Pacotes? pacote = null;
            List<string> assentosOcupados = new List<string>();

            using (MySqlConnection conn = new MySqlConnection(conexao))
            {
                conn.Open();

                string sql = @"SELECT p.id, p.nome, p.tipo_viagem, p.duracao_dias,
                               p.data_partida, p.data_retorno, p.preco_por_pessoa,
                               p.vagas_disponiveis, p.imagem_url,
                               d.pais AS destino_pais, d.estado AS destino_estado,
                               d.origem_pais, d.origem_estado
                        FROM pacotes p
                        INNER JOIN destinos d ON d.id = p.destino_id
                        WHERE p.id = @id";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        pacote = new Pacotes
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Nome = reader["nome"].ToString(),
                            TipoViagem = reader["tipo_viagem"].ToString(),
                            DuracaoDias = Convert.ToInt32(reader["duracao_dias"]),
                            DataPartida = Convert.ToDateTime(reader["data_partida"]),
                            DataRetorno = Convert.ToDateTime(reader["data_retorno"]),
                            PrecoPorPessoa = Convert.ToDecimal(reader["preco_por_pessoa"]),
                            VagasDisponiveis = Convert.ToInt32(reader["vagas_disponiveis"]),
                            ImagemUrl = reader["imagem_url"]?.ToString() ?? ""
                        };

                        ViewBag.DestinoPais = reader["destino_pais"].ToString();
                        ViewBag.DestinoEstado = reader["destino_estado"].ToString();
                        ViewBag.OrigemPais = reader["origem_pais"].ToString();
                        ViewBag.OrigemEstado = reader["origem_estado"].ToString();
                    }
                }

                // Buscar assentos já ocupados para este pacote
                string sqlAssentos = "SELECT nome_item FROM pedidos WHERE tipo_item = 'pacote' AND nome_item LIKE @pattern";
                using (var cmdA = new MySqlCommand(sqlAssentos, conn))
                {
                    cmdA.Parameters.AddWithValue("@pattern", $"%{pacote?.Nome}%Assento:%");
                    using var readerA = cmdA.ExecuteReader();
                    while (readerA.Read())
                    {
                        string nomeItem = readerA["nome_item"].ToString()!;
                        // Extrair o número do assento do nome do item (ex: "... Assento: 12A")
                        var parts = nomeItem.Split("Assento: ");
                        if (parts.Length > 1)
                        {
                            assentosOcupados.Add(parts[1].Trim());
                        }
                    }
                }
            }

            if (pacote == null)
                return NotFound();

            ViewBag.AssentosOcupados = assentosOcupados;
            return View(pacote);
        }

        // POST: /Pacotes/Passagem
        [HttpPost]
        public IActionResult Passagem(int pacoteId, string classeViagem, string tipoAssento, string numeroAssento)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Index", "Login");

            if (string.IsNullOrEmpty(numeroAssento))
            {
                TempData["Erro"] = "Por favor, selecione um assento no mapa.";
                return RedirectToAction("Passagem", new { id = pacoteId });
            }

            // Verificar novamente se o assento está ocupado
            using (MySqlConnection conn = new MySqlConnection(conexao))
            {
                conn.Open();
                string sqlCheck = "SELECT COUNT(*) FROM pedidos WHERE tipo_item = 'pacote' AND nome_item LIKE @pattern";
                using var cmdCheck = new MySqlCommand(sqlCheck, conn);
                cmdCheck.Parameters.AddWithValue("@pattern", $"%Assento: {numeroAssento}%");
                long count = (long)cmdCheck.ExecuteScalar();
                if (count > 0)
                {
                    TempData["Erro"] = "Este assento já foi selecionado por outro usuário. Por favor, escolha outro.";
                    return RedirectToAction("Passagem", new { id = pacoteId });
                }
            }

            // Preço fixado no servidor — nunca confiar em valor vindo do cliente
            decimal precoAdicional = classeViagem switch
            {
                "Executiva" => 450.00m,
                "Primeira Classe" => 1200.00m,
                _ => 0.00m
            };

            var selecao = new SelecaoVoo
            {
                ItemId = pacoteId,
                TipoItem = "pacote",
                ClasseViagem = classeViagem,
                TipoAssento = tipoAssento,
                NumeroAssento = numeroAssento,
                PrecoAdicional = precoAdicional
            };

            HttpContext.Session.SetString(
                $"Voo_pacote_{pacoteId}",
                JsonSerializer.Serialize(selecao)
            );

            return RedirectToAction("Detalhes", new { id = pacoteId });
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