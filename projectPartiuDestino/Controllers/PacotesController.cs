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

                string sql = @"
                    SELECT 
                        id, 
                        destino_id, 
                        nome, 
                        descricao, 
                        tipo_viagem,
                        duracao_dias, 
                        data_partida, 
                        data_retorno,
                        preco_por_pessoa, 
                        vagas_disponiveis, 
                        imagem_url
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
                        Descricao = reader["descricao"]?.ToString(),
                        TipoViagem = reader["tipo_viagem"]?.ToString(),
                        DuracaoDias = Convert.ToInt32(reader["duracao_dias"]),
                        DataPartida = Convert.ToDateTime(reader["data_partida"]),
                        DataRetorno = Convert.ToDateTime(reader["data_retorno"]),
                        PrecoPorPessoa = Convert.ToDecimal(reader["preco_por_pessoa"]),
                        VagasDisponiveis = Convert.ToInt32(reader["vagas_disponiveis"]),
                        ImagemUrl = reader["imagem_url"]?.ToString() ?? ""
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

                string sqlPacote = @"
                    SELECT 
                        id, 
                        destino_id, 
                        nome, 
                        descricao, 
                        tipo_viagem,
                        duracao_dias, 
                        data_partida, 
                        data_retorno,
                        preco_por_pessoa, 
                        vagas_disponiveis, 
                        imagem_url
                    FROM pacotes 
                    WHERE id = @id";

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
                            Descricao = reader["descricao"]?.ToString(),
                            TipoViagem = reader["tipo_viagem"]?.ToString(),
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
                {
                    return NotFound();
                }

                string? vooJson = HttpContext.Session.GetString($"Voo_pacote_{id}");

                if (string.IsNullOrEmpty(vooJson))
                {
                    TempData["Erro"] = "Selecione primeiro sua passagem para continuar.";
                    return RedirectToAction("Passagem", new { id });
                }

                ViewBag.Voo = JsonSerializer.Deserialize<SelecaoVoo>(vooJson);

                string sqlHosp = @"
                    SELECT 
                        id,
                        pacote_id,
                        nome,
                        categoria,
                        descricao,
                        endereco,
                        imagem_url,
                        checkin,
                        checkout,
                        cafe_incluso,
                        wifi_incluso,
                        estacionamento,
                        politica_cancelamento,
                        regras_hospedagem,
                        avaliacao,
                        comodidades
                    FROM hospedagens
                    WHERE pacote_id = @id
                    ORDER BY id
                    LIMIT 1";

                using (var cmdH = new MySqlCommand(sqlHosp, conn))
                {
                    cmdH.Parameters.AddWithValue("@id", id);

                    using var readerH = cmdH.ExecuteReader();

                    while (readerH.Read())
                    {
                        pacote.Hospedagens.Add(new Hospedagem
                        {
                            Id = Convert.ToInt32(readerH["id"]),

                            PacoteId = readerH["pacote_id"] == DBNull.Value
                                ? null
                                : Convert.ToInt32(readerH["pacote_id"]),

                            Nome = readerH["nome"].ToString()!,
                            Categoria = readerH["categoria"]?.ToString(),
                            Descricao = readerH["descricao"]?.ToString(),
                            Endereco = readerH["endereco"]?.ToString(),
                            ImagemUrl = readerH["imagem_url"]?.ToString(),

                            Checkin = readerH["checkin"] == DBNull.Value
                                ? "14:00"
                                : readerH["checkin"].ToString(),

                            Checkout = readerH["checkout"] == DBNull.Value
                                ? "12:00"
                                : readerH["checkout"].ToString(),

                            CafeIncluso = readerH["cafe_incluso"] != DBNull.Value
                                && Convert.ToBoolean(readerH["cafe_incluso"]),

                            WifiIncluso = readerH["wifi_incluso"] != DBNull.Value
                                && Convert.ToBoolean(readerH["wifi_incluso"]),

                            Estacionamento = readerH["estacionamento"] != DBNull.Value
                                && Convert.ToBoolean(readerH["estacionamento"]),

                            PoliticaCancelamento = readerH["politica_cancelamento"]?.ToString(),
                            RegrasHospedagem = readerH["regras_hospedagem"]?.ToString(),

                            Avaliacao = readerH["avaliacao"] == DBNull.Value
                                ? null
                                : Convert.ToDecimal(readerH["avaliacao"]),

                            Comodidades = readerH["comodidades"]?.ToString()
                        });
                    }
                }

                foreach (var hospedagem in pacote.Hospedagens)
                {
                    string sqlQuartos = @"
                        SELECT 
                            id,
                            hospedagem_id,
                            tipo_quarto,
                            capacidade_adultos,
                            capacidade_criancas,
                            preco_adicional,
                            quantidade_disponivel,
                            comodidades,
                            imagem_url,
                            numero_camas,
                            tipo_camas,
                            cafe_incluso,
                            area_m2,
                            descricao,
                            politica_cancelamento
                        FROM quartos
                        WHERE hospedagem_id = @hid
                        ORDER BY preco_adicional";

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
                            ImagemUrl = readerQ["imagem_url"]?.ToString(),

                            NumeroCamas = readerQ["numero_camas"] == DBNull.Value
                                ? null
                                : Convert.ToInt32(readerQ["numero_camas"]),

                            TipoCamas = readerQ["tipo_camas"]?.ToString(),

                            CafeIncluso = readerQ["cafe_incluso"] != DBNull.Value
                                && Convert.ToBoolean(readerQ["cafe_incluso"]),

                            AreaM2 = readerQ["area_m2"] == DBNull.Value
                                ? null
                                : Convert.ToDecimal(readerQ["area_m2"]),

                            Descricao = readerQ["descricao"]?.ToString(),
                            PoliticaCancelamento = readerQ["politica_cancelamento"]?.ToString()
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
            {
                return RedirectToAction("Index", "Login");
            }

            Pacotes? pacote = null;
            List<string> assentosOcupados = new List<string>();

            using (MySqlConnection conn = new MySqlConnection(conexao))
            {
                conn.Open();

                string sql = @"
                    SELECT 
                        p.id,
                        p.nome,
                        p.tipo_viagem,
                        p.duracao_dias,
                        p.data_partida,
                        p.data_retorno,
                        p.preco_por_pessoa,
                        p.vagas_disponiveis,
                        p.imagem_url,

                        p.voo_companhia_aerea,
                        p.voo_titulo,
                        p.voo_descricao,
                        p.voo_aeroporto_origem,
                        p.voo_aeroporto_destino,
                        p.voo_horario_ida,
                        p.voo_horario_volta,
                        p.voo_duracao_media,
                        p.voo_bagagem_inclusa,
                        p.voo_tipo_tarifa,
                        p.voo_escala,
                        p.voo_preco_adicional_por_pessoa,

                        d.pais AS destino_pais,
                        d.estado AS destino_estado,
                        d.origem_pais,
                        d.origem_estado
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
                            TipoViagem = reader["tipo_viagem"]?.ToString(),
                            DuracaoDias = Convert.ToInt32(reader["duracao_dias"]),
                            DataPartida = Convert.ToDateTime(reader["data_partida"]),
                            DataRetorno = Convert.ToDateTime(reader["data_retorno"]),
                            PrecoPorPessoa = Convert.ToDecimal(reader["preco_por_pessoa"]),
                            VagasDisponiveis = Convert.ToInt32(reader["vagas_disponiveis"]),
                            ImagemUrl = reader["imagem_url"]?.ToString() ?? "",

                            VooCompanhiaAerea = reader["voo_companhia_aerea"]?.ToString(),
                            VooTitulo = reader["voo_titulo"]?.ToString(),
                            VooDescricao = reader["voo_descricao"]?.ToString(),
                            VooAeroportoOrigem = reader["voo_aeroporto_origem"]?.ToString(),
                            VooAeroportoDestino = reader["voo_aeroporto_destino"]?.ToString(),
                            VooHorarioIda = reader["voo_horario_ida"]?.ToString(),
                            VooHorarioVolta = reader["voo_horario_volta"]?.ToString(),
                            VooDuracaoMedia = reader["voo_duracao_media"]?.ToString(),
                            VooBagagemInclusa = reader["voo_bagagem_inclusa"]?.ToString(),
                            VooTipoTarifa = reader["voo_tipo_tarifa"]?.ToString(),
                            VooEscala = reader["voo_escala"]?.ToString(),

                            VooPrecoAdicionalPorPessoa = reader["voo_preco_adicional_por_pessoa"] == DBNull.Value
                                ? 0
                                : Convert.ToDecimal(reader["voo_preco_adicional_por_pessoa"])
                        };

                        ViewBag.OrigemEstado = reader["origem_estado"]?.ToString();
                        ViewBag.OrigemPais = reader["origem_pais"]?.ToString();
                        ViewBag.DestinoEstado = reader["destino_estado"]?.ToString();
                        ViewBag.DestinoPais = reader["destino_pais"]?.ToString();
                    }
                }

                if (pacote == null)
                {
                    return NotFound();
                }

                string sqlAssentos = @"
                    SELECT nome_item 
                    FROM pedidos 
                    WHERE tipo_item = 'pacote' 
                      AND nome_item LIKE @pattern";

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
            if (HttpContext.Session.GetString("UserName") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (quantidadeAdultos < 1)
            {
                quantidadeAdultos = 1;
            }

            if (quantidadeCriancas < 0)
            {
                quantidadeCriancas = 0;
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
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(a => a.Trim())
                .ToList();

            if (assentosSelecionados.Count != quantidadeTotal)
            {
                TempData["Erro"] = $"Você precisa selecionar exatamente {quantidadeTotal} assento(s).";
                return RedirectToAction("Passagem", new { id = pacoteId });
            }

            decimal precoAdicionalPorPessoa = 0.00m;

            if (classeViagem == "Executiva")
            {
                precoAdicionalPorPessoa = 450.00m;
            }
            else if (classeViagem == "Primeira Classe")
            {
                precoAdicionalPorPessoa = 1200.00m;
            }

            string companhiaAerea = "";
            string tituloVoo = "";
            string descricaoVoo = "";

            string cidadeOrigem = "";
            string cidadeDestino = "";

            string aeroportoOrigem = "";
            string aeroportoDestino = "";
            string horarioIda = "";
            string horarioVolta = "";
            string duracaoMedia = "";
            string bagagemInclusa = "";
            string tipoTarifa = "";
            string escala = "";

            decimal adicionalVooPorPessoa = 0.00m;

            using (MySqlConnection conn = new MySqlConnection(conexao))
            {
                conn.Open();

                string sqlPacote = @"
                    SELECT vagas_disponiveis
                    FROM pacotes
                    WHERE id = @pacoteId";

                using (var cmdPacote = new MySqlCommand(sqlPacote, conn))
                {
                    cmdPacote.Parameters.AddWithValue("@pacoteId", pacoteId);

                    object? result = cmdPacote.ExecuteScalar();

                    if (result == null)
                    {
                        TempData["Erro"] = "Pacote não encontrado.";
                        return RedirectToAction("Index", "Pacotes");
                    }

                    int vagasDisponiveis = Convert.ToInt32(result);

                    if (quantidadeTotal > vagasDisponiveis)
                    {
                        TempData["Erro"] = "Quantidade de viajantes maior que as vagas disponíveis.";
                        return RedirectToAction("Passagem", new { id = pacoteId });
                    }
                }

                foreach (string assento in assentosSelecionados)
                {
                    string sqlAssento = @"
                        SELECT COUNT(*)
                        FROM carrinho
                        WHERE tipo_item = 'pacote'
                          AND item_id = @pacoteId
                          AND nome_item LIKE @assento";

                    using (var cmdAssento = new MySqlCommand(sqlAssento, conn))
                    {
                        cmdAssento.Parameters.AddWithValue("@pacoteId", pacoteId);
                        cmdAssento.Parameters.AddWithValue("@assento", "%" + assento + "%");

                        int ocupado = Convert.ToInt32(cmdAssento.ExecuteScalar());

                        if (ocupado > 0)
                        {
                            TempData["Erro"] = $"O assento {assento} já foi selecionado.";
                            return RedirectToAction("Passagem", new { id = pacoteId });
                        }
                    }
                }

                string sqlVooPacote = @"
                    SELECT
                        p.voo_companhia_aerea,
                        p.voo_titulo,
                        p.voo_descricao,
                        p.voo_aeroporto_origem,
                        p.voo_aeroporto_destino,
                        p.voo_horario_ida,
                        p.voo_horario_volta,
                        p.voo_duracao_media,
                        p.voo_bagagem_inclusa,
                        p.voo_tipo_tarifa,
                        p.voo_escala,
                        p.voo_preco_adicional_por_pessoa,

                        d.origem_estado,
                        d.estado AS destino_estado
                    FROM pacotes p
                    INNER JOIN destinos d ON d.id = p.destino_id
                    WHERE p.id = @pacoteId";

                using (var cmdVoo = new MySqlCommand(sqlVooPacote, conn))
                {
                    cmdVoo.Parameters.AddWithValue("@pacoteId", pacoteId);

                    using var readerVoo = cmdVoo.ExecuteReader();

                    if (readerVoo.Read())
                    {
                        companhiaAerea = readerVoo["voo_companhia_aerea"]?.ToString() ?? "";
                        tituloVoo = readerVoo["voo_titulo"]?.ToString() ?? "";
                        descricaoVoo = readerVoo["voo_descricao"]?.ToString() ?? "";

                        cidadeOrigem = readerVoo["origem_estado"]?.ToString() ?? "";
                        cidadeDestino = readerVoo["destino_estado"]?.ToString() ?? "";

                        aeroportoOrigem = readerVoo["voo_aeroporto_origem"]?.ToString() ?? "";
                        aeroportoDestino = readerVoo["voo_aeroporto_destino"]?.ToString() ?? "";
                        horarioIda = readerVoo["voo_horario_ida"]?.ToString() ?? "";
                        horarioVolta = readerVoo["voo_horario_volta"]?.ToString() ?? "";
                        duracaoMedia = readerVoo["voo_duracao_media"]?.ToString() ?? "";
                        bagagemInclusa = readerVoo["voo_bagagem_inclusa"]?.ToString() ?? "";
                        tipoTarifa = readerVoo["voo_tipo_tarifa"]?.ToString() ?? "";
                        escala = readerVoo["voo_escala"]?.ToString() ?? "";

                        adicionalVooPorPessoa = readerVoo["voo_preco_adicional_por_pessoa"] == DBNull.Value
                            ? 0.00m
                            : Convert.ToDecimal(readerVoo["voo_preco_adicional_por_pessoa"]);
                    }
                    else
                    {
                        TempData["Erro"] = "Não foi possível carregar os dados do voo deste pacote.";
                        return RedirectToAction("Passagem", new { id = pacoteId });
                    }
                }
            }

            decimal precoAdicionalClasseTotal = precoAdicionalPorPessoa * quantidadeTotal;
            decimal precoAdicionalVooTotal = adicionalVooPorPessoa * quantidadeTotal;
            decimal precoAdicionalTotal = precoAdicionalClasseTotal + precoAdicionalVooTotal;

            var selecao = new SelecaoVoo
            {
                ItemId = pacoteId,
                TipoItem = "pacote",

                ClasseViagem = classeViagem,
                TipoAssento = "Múltiplos",
                NumeroAssento = string.Join(", ", assentosSelecionados),

                CidadeOrigem = cidadeOrigem,
                CidadeDestino = cidadeDestino,

                CompanhiaAerea = companhiaAerea,
                TituloVoo = tituloVoo,
                DescricaoVoo = descricaoVoo,
                AeroportoOrigem = aeroportoOrigem,
                AeroportoDestino = aeroportoDestino,
                HorarioIda = horarioIda,
                HorarioVolta = horarioVolta,
                DuracaoMedia = duracaoMedia,
                BagagemInclusa = bagagemInclusa,
                TipoTarifa = tipoTarifa,
                Escala = escala,

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

                cmd.Parameters.AddWithValue("@termo", "%" + termo + "%");

                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    pacotes.Add(new Pacotes
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

            var voo = JsonSerializer.Deserialize<SelecaoVoo>(vooJson);

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
                    SELECT 
                        id, 
                        destino_id, 
                        nome, 
                        descricao, 
                        tipo_viagem,
                        duracao_dias, 
                        data_partida, 
                        data_retorno,
                        preco_por_pessoa, 
                        vagas_disponiveis, 
                        imagem_url
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
                    SELECT 
                        id, 
                        pacote_id, 
                        nome, 
                        categoria, 
                        descricao, 
                        endereco, 
                        imagem_url,
                        checkin, 
                        checkout, 
                        cafe_incluso, 
                        wifi_incluso, 
                        estacionamento,
                        politica_cancelamento, 
                        regras_hospedagem, 
                        avaliacao, 
                        comodidades
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
                            Avaliacao = reader["avaliacao"] == DBNull.Value
                                ? null
                                : Convert.ToDecimal(reader["avaliacao"]),
                            Comodidades = reader["comodidades"]?.ToString()
                        };
                    }
                }

                if (hospedagem == null)
                {
                    return NotFound();
                }

                string sqlQuartos = @"
                    SELECT 
                        id, 
                        hospedagem_id, 
                        tipo_quarto, 
                        capacidade_adultos,
                        capacidade_criancas, 
                        preco_adicional, 
                        quantidade_disponivel,
                        comodidades, 
                        imagem_url, 
                        numero_camas, 
                        tipo_camas,
                        cafe_incluso, 
                        area_m2, 
                        descricao, 
                        politica_cancelamento
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
                            NumeroCamas = reader["numero_camas"] == DBNull.Value
                                ? null
                                : Convert.ToInt32(reader["numero_camas"]),
                            TipoCamas = reader["tipo_camas"]?.ToString(),
                            CafeIncluso = Convert.ToBoolean(reader["cafe_incluso"]),
                            AreaM2 = reader["area_m2"] == DBNull.Value
                                ? null
                                : Convert.ToDecimal(reader["area_m2"]),
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