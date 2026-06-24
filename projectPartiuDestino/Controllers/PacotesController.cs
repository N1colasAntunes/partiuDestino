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


        // GET: /Pacotes/Passagem
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

                if (pacote == null)
                    return NotFound();

                string sqlAssentos = "SELECT nome_item FROM pedidos WHERE tipo_item = 'pacote' AND nome_item LIKE @pattern";

                using (var cmdA = new MySqlCommand(sqlAssentos, conn))
                {
                    cmdA.Parameters.AddWithValue("@pattern", $"%{pacote.Nome}%Assento:%");

                    using var readerA = cmdA.ExecuteReader();

                    while (readerA.Read())
                    {
                        string nomeItem = readerA["nome_item"].ToString()!;

                        var parts = nomeItem.Split("Assento:");

                        if (parts.Length > 1)
                        {
                            string assentosTexto = parts[1].Trim();

                            string[] assentos = assentosTexto.Split(",");

                            foreach (string assento in assentos)
                            {
                                string assentoLimpo = assento.Trim();

                                if (!string.IsNullOrEmpty(assentoLimpo))
                                {
                                    assentosOcupados.Add(assentoLimpo);
                                }
                            }
                        }
                    }
                }
            }

            ViewBag.AssentosOcupados = assentosOcupados;

            return View(pacote);
        }

        // POST: /Pacotes/Passagem
        [HttpPost]
        public IActionResult Passagem(
            int pacoteId,
            string classeViagem,
            string tipoAssento,
            string numeroAssento,
            int quantidadeAdultos,
            int quantidadeCriancas)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Index", "Login");

            if (quantidadeAdultos < 1)
            {
                TempData["Erro"] = "Informe pelo menos 1 adulto para continuar.";
                return RedirectToAction("Passagem", new { id = pacoteId });
            }

            if (quantidadeCriancas < 0)
            {
                TempData["Erro"] = "A quantidade de crianças não pode ser negativa.";
                return RedirectToAction("Passagem", new { id = pacoteId });
            }

            int quantidadeTotal = quantidadeAdultos + quantidadeCriancas;

            if (quantidadeTotal <= 0)
            {
                TempData["Erro"] = "Informe a quantidade de viajantes.";
                return RedirectToAction("Passagem", new { id = pacoteId });
            }

            if (string.IsNullOrWhiteSpace(numeroAssento))
            {
                TempData["Erro"] = "Selecione os assentos antes de continuar.";
                return RedirectToAction("Passagem", new { id = pacoteId });
            }

            List<string> assentosSelecionados = numeroAssento
                .Split(",")
                .Select(a => a.Trim())
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .Distinct()
                .ToList();

            if (assentosSelecionados.Count != quantidadeTotal)
            {
                TempData["Erro"] = $"Você informou {quantidadeTotal} viajante(s), então precisa escolher {quantidadeTotal} assento(s).";
                return RedirectToAction("Passagem", new { id = pacoteId });
            }

            using (MySqlConnection conn = new MySqlConnection(conexao))
            {
                conn.Open();

                string sqlPacote = "SELECT vagas_disponiveis FROM pacotes WHERE id = @id";

                using (var cmdPacote = new MySqlCommand(sqlPacote, conn))
                {
                    cmdPacote.Parameters.AddWithValue("@id", pacoteId);

                    object? resultado = cmdPacote.ExecuteScalar();

                    if (resultado == null)
                    {
                        TempData["Erro"] = "Pacote não encontrado.";
                        return RedirectToAction("Index");
                    }

                    int vagasDisponiveis = Convert.ToInt32(resultado);

                    if (quantidadeTotal > vagasDisponiveis)
                    {
                        TempData["Erro"] = "A quantidade de viajantes é maior que as vagas disponíveis para este pacote.";
                        return RedirectToAction("Passagem", new { id = pacoteId });
                    }
                }

                foreach (string assento in assentosSelecionados)
                {
                    string sqlCheck = "SELECT COUNT(*) FROM pedidos WHERE tipo_item = 'pacote' AND nome_item LIKE @pattern";

                    using var cmdCheck = new MySqlCommand(sqlCheck, conn);
                    cmdCheck.Parameters.AddWithValue("@pattern", $"%Assento: %{assento}%");

                    long count = Convert.ToInt64(cmdCheck.ExecuteScalar());

                    if (count > 0)
                    {
                        TempData["Erro"] = $"O assento {assento} já foi selecionado por outro usuário. Escolha outro.";
                        return RedirectToAction("Passagem", new { id = pacoteId });
                    }
                }
            }

            decimal precoAdicionalPorPessoa = classeViagem switch
            {
                "Executiva" => 450.00m,
                "Primeira Classe" => 1200.00m,
                _ => 0.00m
            };

            decimal precoAdicionalTotal = precoAdicionalPorPessoa * quantidadeTotal;

            var selecao = new SelecaoVoo
            {
                ItemId = pacoteId,
                TipoItem = "pacote",
                ClasseViagem = classeViagem,
                TipoAssento = "Múltiplos",
                NumeroAssento = string.Join(", ", assentosSelecionados),
                PrecoAdicional = precoAdicionalTotal,
                QuantidadeAdultos = quantidadeAdultos,
                QuantidadeCriancas = quantidadeCriancas,
                QuantidadeTotal = quantidadeTotal
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


        public IActionResult DetalhesHospedagem(int pacoteId, int hospedagemId)
        {
            if (HttpContext.Session.GetString("UserName") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            string? vooJson = HttpContext.Session.GetString($"Voo_pacote_{pacoteId}");

            if (string.IsNullOrEmpty(vooJson))
            {
                TempData["Erro"] = "Selecione primeiro sua passagem para continuar.";
                return RedirectToAction("Passagem", new { id = pacoteId });
            }

            var voo = System.Text.Json.JsonSerializer.Deserialize<SelecaoVoo>(vooJson);

            if (voo == null)
            {
                TempData["Erro"] = "Não foi possível recuperar os dados da passagem.";
                return RedirectToAction("Passagem", new { id = pacoteId });
            }

            Pacotes? pacote = null;
            Hospedagem? hospedagem = null;

            using (MySqlConnection conn = new MySqlConnection(conexao))
            {
                conn.Open();

                string sqlPacote = @"
            SELECT id, destino_id, nome, descricao, tipo_viagem,
                   duracao_dias, data_partida, data_retorno,
                   preco_por_pessoa, vagas_disponiveis, imagem_url
            FROM pacotes
            WHERE id = @pacoteId";

                using (var cmdPacote = new MySqlCommand(sqlPacote, conn))
                {
                    cmdPacote.Parameters.AddWithValue("@pacoteId", pacoteId);

                    using var reader = cmdPacote.ExecuteReader();

                    if (reader.Read())
                    {
                        pacote = new Pacotes
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            DestinoId = Convert.ToInt32(reader["destino_id"]),
                            Nome = reader["nome"].ToString(),
                            Descricao = reader["descricao"]?.ToString(),
                            TipoViagem = reader["tipo_viagem"]?.ToString(),
                            DuracaoDias = Convert.ToInt32(reader["duracao_dias"]),
                            DataPartida = Convert.ToDateTime(reader["data_partida"]),
                            DataRetorno = Convert.ToDateTime(reader["data_retorno"]),
                            PrecoPorPessoa = Convert.ToDecimal(reader["preco_por_pessoa"]),
                            VagasDisponiveis = Convert.ToInt32(reader["vagas_disponiveis"]),
                            ImagemUrl = reader["imagem_url"]?.ToString()
                        };
                    }
                }

                if (pacote == null)
                {
                    return NotFound();
                }

                string sqlHospedagem = @"
            SELECT id, pacote_id, nome, categoria, descricao, endereco, imagem_url,
                   checkin, checkout, cafe_incluso, wifi_incluso, estacionamento,
                   politica_cancelamento, regras_hospedagem, avaliacao, comodidades
            FROM hospedagens
            WHERE id = @hospedagemId
              AND pacote_id = @pacoteId";

                using (var cmdHosp = new MySqlCommand(sqlHospedagem, conn))
                {
                    cmdHosp.Parameters.AddWithValue("@hospedagemId", hospedagemId);
                    cmdHosp.Parameters.AddWithValue("@pacoteId", pacoteId);

                    using var reader = cmdHosp.ExecuteReader();

                    if (reader.Read())
                    {
                        hospedagem = new Hospedagem
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            PacoteId = Convert.ToInt32(reader["pacote_id"]),
                            Nome = reader["nome"].ToString()!,
                            Categoria = reader["categoria"]?.ToString(),
                            Descricao = reader["descricao"]?.ToString(),
                            Endereco = reader["endereco"]?.ToString(),
                            ImagemUrl = reader["imagem_url"]?.ToString(),
                            Checkin = reader["checkin"]?.ToString(),
                            Checkout = reader["checkout"]?.ToString(),
                            CafeIncluso = Convert.ToBoolean(reader["cafe_incluso"]),
                            WifiIncluso = Convert.ToBoolean(reader["wifi_incluso"]),
                            Estacionamento = Convert.ToBoolean(reader["estacionamento"]),
                            PoliticaCancelamento = reader["politica_cancelamento"]?.ToString(),
                            RegrasHospedagem = reader["regras_hospedagem"]?.ToString(),
                            Avaliacao = reader["avaliacao"] == DBNull.Value ? null : Convert.ToDecimal(reader["avaliacao"]),
                            Comodidades = reader["comodidades"]?.ToString()
                        };
                    }
                }

                if (hospedagem == null)
                {
                    return NotFound();
                }

                string sqlQuartos = @"
            SELECT id, hospedagem_id, tipo_quarto, capacidade_adultos,
                   capacidade_criancas, preco_adicional, quantidade_disponivel,
                   comodidades, imagem_url, numero_camas, tipo_camas,
                   cafe_incluso, area_m2, descricao, politica_cancelamento
            FROM quartos
            WHERE hospedagem_id = @hospedagemId
            ORDER BY preco_adicional";

                using (var cmdQuartos = new MySqlCommand(sqlQuartos, conn))
                {
                    cmdQuartos.Parameters.AddWithValue("@hospedagemId", hospedagemId);

                    using var reader = cmdQuartos.ExecuteReader();

                    while (reader.Read())
                    {
                        hospedagem.Quartos.Add(new Quarto
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            HospedagemId = Convert.ToInt32(reader["hospedagem_id"]),
                            TipoQuarto = reader["tipo_quarto"].ToString()!,
                            CapacidadeAdultos = Convert.ToInt32(reader["capacidade_adultos"]),
                            CapacidadeCriancas = Convert.ToInt32(reader["capacidade_criancas"]),
                            PrecoAdicional = Convert.ToDecimal(reader["preco_adicional"]),
                            QuantidadeDisponivel = Convert.ToInt32(reader["quantidade_disponivel"]),
                            Comodidades = reader["comodidades"]?.ToString(),
                            ImagemUrl = reader["imagem_url"]?.ToString(),
                            NumeroCamas = reader["numero_camas"] == DBNull.Value ? null : Convert.ToInt32(reader["numero_camas"]),
                            TipoCamas = reader["tipo_camas"]?.ToString(),
                            CafeIncluso = Convert.ToBoolean(reader["cafe_incluso"]),
                            AreaM2 = reader["area_m2"] == DBNull.Value ? null : Convert.ToDecimal(reader["area_m2"]),
                            Descricao = reader["descricao"]?.ToString(),
                            PoliticaCancelamento = reader["politica_cancelamento"]?.ToString()
                        });
                    }
                }
            }

            ViewBag.Pacote = pacote;
            ViewBag.Voo = voo;

            return View(hospedagem);
        }

    }
}