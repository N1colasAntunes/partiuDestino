using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using projectPartiuDestino.Models;

namespace projectPartiuDestino.Controllers
{
    public class CarrinhoController : Controller
    {
        private string conexao = "server=localhost;database=bdpartiudestino;uid=root;pwd=12345678;";

        // ============================================================
        // GET: /Carrinho/Index
        // ============================================================
        public IActionResult Index()
        {
            int? usuarioId = HttpContext.Session.GetInt32("UserId");
            if (usuarioId == null)
                return RedirectToAction("Index", "Login");

            List<CarrinhoItem> itens = new();

            using (MySqlConnection conn = new MySqlConnection(conexao))
            {
                conn.Open();

                string sql = @"SELECT id, usuario_id, tipo_item, item_id,
                                      nome_item, preco_unitario, quantidade, data_adicionado
                               FROM carrinho
                               WHERE usuario_id = @uid
                               ORDER BY data_adicionado DESC";

                using MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@uid", usuarioId);

                using MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    itens.Add(new CarrinhoItem
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        UsuarioId = Convert.ToInt32(reader["usuario_id"]),
                        TipoItem = reader["tipo_item"].ToString()!,
                        ItemId = Convert.ToInt32(reader["item_id"]),
                        NomeItem = reader["nome_item"].ToString()!,
                        PrecoUnitario = Convert.ToDecimal(reader["preco_unitario"]),
                        Quantidade = Convert.ToInt32(reader["quantidade"]),
                        DataAdicionado = Convert.ToDateTime(reader["data_adicionado"])
                    });
                }
            }

            if (TempData["Sucesso"] != null)
                ViewBag.Sucesso = TempData["Sucesso"].ToString();

            if (TempData["Erro"] != null)
                ViewBag.Erro = TempData["Erro"].ToString();

            return View(itens);
        }
        // POST: /Carrinho/AdicionarPacote
        // ============================================================
        [HttpPost]
        public IActionResult AdicionarPacote(
    int pacoteId,
    int? quartoId,
    bool aceitouTermos = false,
    int quantidadeAdultos = 1,
    int quantidadeCriancas = 0,
    int quantidadeTotal = 1,
    List<string>? nomesViajantes = null,
    List<string>? documentosViajantes = null,
    List<string>? nascimentosViajantes = null,
    List<string>? tiposViajantes = null)
        {
            int? usuarioId = HttpContext.Session.GetInt32("UserId");

            if (usuarioId == null)
                return RedirectToAction("Index", "Login");

            if (HttpContext.Session.GetString("UserRole") == "admin")
            {
                TempData["Erro"] = "Administradores não podem realizar compras.";
                return RedirectToAction("Index", "Carrinho");
            }

            if (!aceitouTermos)
            {
                TempData["Erro"] = "Você precisa aceitar os termos da reserva para continuar.";
                return RedirectToAction("Detalhes", "Pacotes", new { id = pacoteId });
            }

            if (quartoId == null || quartoId <= 0)
            {
                TempData["Erro"] = "Escolha um quarto antes de adicionar o pacote ao carrinho.";
                return RedirectToAction("Detalhes", "Pacotes", new { id = pacoteId });
            }

            string? vooJson = HttpContext.Session.GetString($"Voo_pacote_{pacoteId}");

            if (string.IsNullOrEmpty(vooJson))
            {
                TempData["Erro"] = "Escolha a passagem antes de continuar.";
                return RedirectToAction("Passagem", "Pacotes", new { id = pacoteId });
            }

            var voo = System.Text.Json.JsonSerializer.Deserialize<projectPartiuDestino.Models.SelecaoVoo>(vooJson);

            if (voo == null)
            {
                TempData["Erro"] = "Não foi possível recuperar os dados da passagem.";
                return RedirectToAction("Passagem", "Pacotes", new { id = pacoteId });
            }

            quantidadeAdultos = voo.QuantidadeAdultos;
            quantidadeCriancas = voo.QuantidadeCriancas;
            quantidadeTotal = voo.QuantidadeTotal > 0 ? voo.QuantidadeTotal : 1;

            if (nomesViajantes == null ||
                documentosViajantes == null ||
                nascimentosViajantes == null ||
                tiposViajantes == null)
            {
                TempData["Erro"] = "Preencha os dados dos viajantes antes de continuar.";
                return RedirectToAction("Detalhes", "Pacotes", new { id = pacoteId });
            }

            if (nomesViajantes.Count != quantidadeTotal ||
                documentosViajantes.Count != quantidadeTotal ||
                nascimentosViajantes.Count != quantidadeTotal ||
                tiposViajantes.Count != quantidadeTotal)
            {
                TempData["Erro"] = "A quantidade de viajantes preenchidos não bate com a quantidade selecionada.";
                return RedirectToAction("Detalhes", "Pacotes", new { id = pacoteId });
            }

            for (int i = 0; i < quantidadeTotal; i++)
            {
                if (string.IsNullOrWhiteSpace(nomesViajantes[i]) ||
                    string.IsNullOrWhiteSpace(documentosViajantes[i]) ||
                    string.IsNullOrWhiteSpace(nascimentosViajantes[i]))
                {
                    TempData["Erro"] = "Todos os viajantes precisam ter nome, documento e data de nascimento.";
                    return RedirectToAction("Detalhes", "Pacotes", new { id = pacoteId });
                }
            }

            int totalAdultosInformados = tiposViajantes.Count(t => t == "Adulto");
            int totalCriancasInformadas = tiposViajantes.Count(t => t == "Criança");

            if (totalAdultosInformados != quantidadeAdultos ||
                totalCriancasInformadas != quantidadeCriancas)
            {
                TempData["Erro"] = "A quantidade de adultos e crianças preenchidos não bate com a passagem.";
                return RedirectToAction("Detalhes", "Pacotes", new { id = pacoteId });
            }

            using MySqlConnection conn = new MySqlConnection(conexao);
            conn.Open();

            string nomePacote = "";
            decimal precoBasePacote = 0;

            string sqlPacote = "SELECT nome, preco_por_pessoa FROM pacotes WHERE id = @id";

            using (MySqlCommand cmdP = new MySqlCommand(sqlPacote, conn))
            {
                cmdP.Parameters.AddWithValue("@id", pacoteId);

                using MySqlDataReader r = cmdP.ExecuteReader();

                if (r.Read())
                {
                    nomePacote = r["nome"].ToString()!;
                    precoBasePacote = Convert.ToDecimal(r["preco_por_pessoa"]);
                }
                else
                {
                    TempData["Erro"] = "Pacote não encontrado.";
                    return RedirectToAction("Index", "Pacotes");
                }
            }

            string tipoQuarto = "";
            decimal precoAdicionalQuarto = 0;
            int capacidadeAdultos = 0;
            int capacidadeCriancas = 0;
            int quantidadeDisponivel = 0;

            string sqlQuarto = @"
        SELECT 
            q.tipo_quarto,
            q.preco_adicional,
            q.capacidade_adultos,
            q.capacidade_criancas,
            q.quantidade_disponivel
        FROM quartos q
        INNER JOIN hospedagens h ON h.id = q.hospedagem_id
        WHERE q.id = @quartoId
          AND h.pacote_id = @pacoteId";

            using (MySqlCommand cmdQ = new MySqlCommand(sqlQuarto, conn))
            {
                cmdQ.Parameters.AddWithValue("@quartoId", quartoId.Value);
                cmdQ.Parameters.AddWithValue("@pacoteId", pacoteId);

                using MySqlDataReader rq = cmdQ.ExecuteReader();

                if (rq.Read())
                {
                    tipoQuarto = rq["tipo_quarto"].ToString()!;
                    precoAdicionalQuarto = Convert.ToDecimal(rq["preco_adicional"]);
                    capacidadeAdultos = Convert.ToInt32(rq["capacidade_adultos"]);
                    capacidadeCriancas = Convert.ToInt32(rq["capacidade_criancas"]);
                    quantidadeDisponivel = Convert.ToInt32(rq["quantidade_disponivel"]);
                }
                else
                {
                    TempData["Erro"] = "Quarto inválido para este pacote.";
                    return RedirectToAction("Detalhes", "Pacotes", new { id = pacoteId });
                }
            }

            if (quantidadeDisponivel <= 0)
            {
                TempData["Erro"] = "Este quarto está indisponível.";
                return RedirectToAction("Detalhes", "Pacotes", new { id = pacoteId });
            }

            if (quantidadeAdultos > capacidadeAdultos || quantidadeCriancas > capacidadeCriancas)
            {
                TempData["Erro"] = "O quarto escolhido não comporta a quantidade de viajantes selecionada.";
                return RedirectToAction("Detalhes", "Pacotes", new { id = pacoteId });
            }

            decimal precoTotal = precoBasePacote * quantidadeTotal;

            precoTotal += voo.PrecoAdicional;
            precoTotal += precoAdicionalQuarto;

            string nomeItem = nomePacote;

            if (!string.IsNullOrEmpty(voo.CompanhiaAerea))
            {
                nomeItem += $" — Companhia: {voo.CompanhiaAerea}";
            }

            if (!string.IsNullOrEmpty(voo.TituloVoo))
            {
                nomeItem += $" — Voo: {voo.TituloVoo}";
            }

            if (voo.ClasseViagem != "Econômica")
            {
                nomeItem += $" — Classe {voo.ClasseViagem}";
            }

            if (!string.IsNullOrEmpty(voo.AeroportoOrigem) || !string.IsNullOrEmpty(voo.AeroportoDestino))
            {
                nomeItem += $" — Rota: {voo.AeroportoOrigem} → {voo.AeroportoDestino}";
            }

            if (!string.IsNullOrEmpty(voo.HorarioIda) || !string.IsNullOrEmpty(voo.HorarioVolta))
            {
                nomeItem += $" — Ida: {voo.HorarioIda} / Volta: {voo.HorarioVolta}";
            }

            nomeItem += $" — Viajantes: {quantidadeTotal}";

            nomeItem += $" ({quantidadeAdultos} adulto(s), {quantidadeCriancas} criança(s))";

            if (!string.IsNullOrEmpty(voo.NumeroAssento))
            {
                nomeItem += $" — Assento: {voo.NumeroAssento}";
            }

            nomeItem += $" — Quarto: {tipoQuarto}";

            string sqlCheck = @"
        SELECT id 
        FROM carrinho
        WHERE usuario_id = @uid 
          AND tipo_item = 'pacote'
          AND item_id = @iid 
          AND nome_item = @nome";

            using MySqlCommand cmdCheck = new MySqlCommand(sqlCheck, conn);
            cmdCheck.Parameters.AddWithValue("@uid", usuarioId);
            cmdCheck.Parameters.AddWithValue("@iid", pacoteId);
            cmdCheck.Parameters.AddWithValue("@nome", nomeItem);

            object? existente = cmdCheck.ExecuteScalar();

            if (existente != null)
            {
                string sqlUp = "UPDATE carrinho SET quantidade = quantidade + 1 WHERE id = @cid";

                using MySqlCommand cmdUp = new MySqlCommand(sqlUp, conn);
                cmdUp.Parameters.AddWithValue("@cid", Convert.ToInt32(existente));
                cmdUp.ExecuteNonQuery();
            }
            else
            {
                string sqlIns = @"
            INSERT INTO carrinho
            (usuario_id, tipo_item, item_id, nome_item, preco_unitario, quantidade)
            VALUES 
            (@uid, 'pacote', @iid, @nome, @preco, 1)";

                using MySqlCommand cmdIns = new MySqlCommand(sqlIns, conn);
                cmdIns.Parameters.AddWithValue("@uid", usuarioId);
                cmdIns.Parameters.AddWithValue("@iid", pacoteId);
                cmdIns.Parameters.AddWithValue("@nome", nomeItem);
                cmdIns.Parameters.AddWithValue("@preco", precoTotal);
                cmdIns.ExecuteNonQuery();
            }

            HttpContext.Session.Remove($"Voo_pacote_{pacoteId}");

            TempData["Sucesso"] = $"{nomeItem} adicionado ao carrinho!";
            return RedirectToAction("Index", "Carrinho");
        }

        // ============================================================
        // POST: /Carrinho/AdicionarDestino
        // ATUALIZADO: agora salva o preco_por_pessoa do destino
        // ============================================================
        [HttpPost]
        public IActionResult AdicionarDestino(
    int destinoId,
    int? quartoId,
    bool aceitouTermos = false,
    int quantidadeAdultos = 1,
    int quantidadeCriancas = 0,
    int quantidadeTotal = 1,
    List<string>? nomesViajantes = null,
    List<string>? documentosViajantes = null,
    List<string>? nascimentosViajantes = null,
    List<string>? tiposViajantes = null)
        {
            int? usuarioId = HttpContext.Session.GetInt32("UserId");
            if (usuarioId == null)
                return RedirectToAction("Index", "Login");

            if (HttpContext.Session.GetString("UserRole") == "admin")
            {
                TempData["Erro"] = "Administradores não podem realizar compras.";
                return RedirectToAction("Index", "Carrinho");
            }

            if (!aceitouTermos)
            {
                TempData["Erro"] = "Você precisa aceitar os termos da reserva para continuar.";
                return RedirectToAction("Detalhes", "Destinos", new { id = destinoId });
            }

            string? vooJson = HttpContext.Session.GetString($"Voo_destino_{destinoId}");
            if (string.IsNullOrEmpty(vooJson))
            {
                TempData["Erro"] = "Escolha a passagem antes de continuar.";
                return RedirectToAction("Passagem", "Destinos", new { id = destinoId });
            }

            var voo = System.Text.Json.JsonSerializer.Deserialize<projectPartiuDestino.Models.SelecaoVoo>(vooJson);
            if (voo == null)
            {
                TempData["Erro"] = "Não foi possível recuperar os dados da passagem.";
                return RedirectToAction("Passagem", "Destinos", new { id = destinoId });
            }

            quantidadeAdultos = voo.QuantidadeAdultos;
            quantidadeCriancas = voo.QuantidadeCriancas;
            quantidadeTotal = voo.QuantidadeTotal > 0 ? voo.QuantidadeTotal : 1;

            if (nomesViajantes == null || documentosViajantes == null ||
                nascimentosViajantes == null || tiposViajantes == null)
            {
                TempData["Erro"] = "Preencha os dados dos viajantes antes de continuar.";
                return RedirectToAction("Detalhes", "Destinos", new { id = destinoId });
            }

            if (nomesViajantes.Count != quantidadeTotal ||
                documentosViajantes.Count != quantidadeTotal ||
                nascimentosViajantes.Count != quantidadeTotal ||
                tiposViajantes.Count != quantidadeTotal)
            {
                TempData["Erro"] = "A quantidade de viajantes preenchidos não bate com a quantidade selecionada.";
                return RedirectToAction("Detalhes", "Destinos", new { id = destinoId });
            }

            for (int i = 0; i < quantidadeTotal; i++)
            {
                if (string.IsNullOrWhiteSpace(nomesViajantes[i]) ||
                    string.IsNullOrWhiteSpace(documentosViajantes[i]) ||
                    string.IsNullOrWhiteSpace(nascimentosViajantes[i]))
                {
                    TempData["Erro"] = "Todos os viajantes precisam ter nome, documento e data de nascimento.";
                    return RedirectToAction("Detalhes", "Destinos", new { id = destinoId });
                }
            }

            using MySqlConnection conn = new MySqlConnection(conexao);
            conn.Open();

            string nomeDestino;
            decimal precoBaseDestino;

            string sqlDestino = "SELECT pais, estado, preco_por_pessoa FROM destinos WHERE id = @id";
            using (var cmdD = new MySqlCommand(sqlDestino, conn))
            {
                cmdD.Parameters.AddWithValue("@id", destinoId);
                using var r = cmdD.ExecuteReader();
                if (r.Read())
                {
                    nomeDestino = $"{r["pais"]} - {r["estado"]}";
                    precoBaseDestino = Convert.ToDecimal(r["preco_por_pessoa"]);
                }
                else
                {
                    TempData["Erro"] = "Destino não encontrado.";
                    return RedirectToAction("Index", "Destinos");
                }
            }

            string tipoQuarto = "Sem hospedagem";
            decimal precoAdicionalQuarto = 0;

            // Hospedagem é OPCIONAL no fluxo de Passagens
            if (quartoId.HasValue && quartoId > 0)
            {
                int capacidadeAdultos = 0, capacidadeCriancas = 0, quantidadeDisponivel = 0;

                string sqlQuarto = @"
            SELECT q.tipo_quarto, q.preco_adicional, q.capacidade_adultos,
                   q.capacidade_criancas, q.quantidade_disponivel
            FROM quartos q
            INNER JOIN hospedagens h ON h.id = q.hospedagem_id
            WHERE q.id = @quartoId AND h.destino_id = @destinoId";

                using (var cmdQ = new MySqlCommand(sqlQuarto, conn))
                {
                    cmdQ.Parameters.AddWithValue("@quartoId", quartoId.Value);
                    cmdQ.Parameters.AddWithValue("@destinoId", destinoId);

                    using var rq = cmdQ.ExecuteReader();
                    if (rq.Read())
                    {
                        tipoQuarto = rq["tipo_quarto"].ToString()!;
                        precoAdicionalQuarto = Convert.ToDecimal(rq["preco_adicional"]);
                        capacidadeAdultos = Convert.ToInt32(rq["capacidade_adultos"]);
                        capacidadeCriancas = Convert.ToInt32(rq["capacidade_criancas"]);
                        quantidadeDisponivel = Convert.ToInt32(rq["quantidade_disponivel"]);
                    }
                    else
                    {
                        TempData["Erro"] = "Quarto inválido para esta passagem.";
                        return RedirectToAction("Detalhes", "Destinos", new { id = destinoId });
                    }
                }

                if (quantidadeDisponivel <= 0)
                {
                    TempData["Erro"] = "Este quarto está indisponível.";
                    return RedirectToAction("Detalhes", "Destinos", new { id = destinoId });
                }

                if (quantidadeAdultos > capacidadeAdultos || quantidadeCriancas > capacidadeCriancas)
                {
                    TempData["Erro"] = "O quarto escolhido não comporta a quantidade de viajantes selecionada.";
                    return RedirectToAction("Detalhes", "Destinos", new { id = destinoId });
                }
            }

            decimal precoTotal = precoBaseDestino * quantidadeTotal;
            precoTotal += voo.PrecoAdicional;
            precoTotal += precoAdicionalQuarto; // mesma regra do fluxo de Pacotes: adicional fixo por reserva

            string nomeItem = nomeDestino;

            if (voo.ClasseViagem != "Econômica")
                nomeItem += $" — Classe {voo.ClasseViagem}";

            nomeItem += $" — Viajantes: {quantidadeTotal} ({quantidadeAdultos} adulto(s), {quantidadeCriancas} criança(s))";

            if (!string.IsNullOrEmpty(voo.NumeroAssento))
                nomeItem += $" — Assento: {voo.NumeroAssento}";

            nomeItem += $" — Hospedagem: {tipoQuarto}";

            string sqlCheck = @"
        SELECT id FROM carrinho
        WHERE usuario_id = @uid AND tipo_item = 'destino' AND item_id = @iid AND nome_item = @nome";

            using MySqlCommand cmdCheck = new MySqlCommand(sqlCheck, conn);
            cmdCheck.Parameters.AddWithValue("@uid", usuarioId);
            cmdCheck.Parameters.AddWithValue("@iid", destinoId);
            cmdCheck.Parameters.AddWithValue("@nome", nomeItem);
            object? existente = cmdCheck.ExecuteScalar();

            if (existente != null)
            {
                string sqlUp = "UPDATE carrinho SET quantidade = quantidade + 1 WHERE id = @cid";
                using MySqlCommand cmdUp = new MySqlCommand(sqlUp, conn);
                cmdUp.Parameters.AddWithValue("@cid", Convert.ToInt32(existente));
                cmdUp.ExecuteNonQuery();
            }
            else
            {
                string sqlIns = @"
            INSERT INTO carrinho (usuario_id, tipo_item, item_id, nome_item, preco_unitario, quantidade)
            VALUES (@uid, 'destino', @iid, @nome, @preco, 1)";

                using MySqlCommand cmdIns = new MySqlCommand(sqlIns, conn);
                cmdIns.Parameters.AddWithValue("@uid", usuarioId);
                cmdIns.Parameters.AddWithValue("@iid", destinoId);
                cmdIns.Parameters.AddWithValue("@nome", nomeItem);
                cmdIns.Parameters.AddWithValue("@preco", precoTotal);
                cmdIns.ExecuteNonQuery();
            }

            HttpContext.Session.Remove($"Voo_destino_{destinoId}");

            TempData["Sucesso"] = $"{nomeItem} adicionado ao carrinho!";
            return RedirectToAction("Index", "Carrinho");
        }

        // ============================================================
        // POST: /Carrinho/AdicionarPersonalizada
        // ============================================================
        [HttpPost]
        public IActionResult AdicionarPersonalizada(int viagemId)
        {
            int? usuarioId = HttpContext.Session.GetInt32("UserId");
            if (usuarioId == null)
                return RedirectToAction("Index", "Login");

            if (HttpContext.Session.GetString("UserRole") == "admin")
            {
                TempData["Erro"] = "Administradores não podem realizar compras.";
                return RedirectToAction("Index", "Carrinho");
            }

            using MySqlConnection conn = new MySqlConnection(conexao);
            conn.Open();

            string sqlV = "SELECT destino FROM viagem_personalizada WHERE id = @id AND usuario_id = @uid";
            using MySqlCommand cmdV = new MySqlCommand(sqlV, conn);
            cmdV.Parameters.AddWithValue("@id", viagemId);
            cmdV.Parameters.AddWithValue("@uid", usuarioId);

            string nomeViagem = "";

            using (MySqlDataReader r = cmdV.ExecuteReader())
            {
                if (r.Read())
                    nomeViagem = $"Viagem personalizada - {r["destino"]}";
                else
                {
                    TempData["Erro"] = "Viagem personalizada não encontrada.";
                    return RedirectToAction("Index");
                }
            }

            string sqlCheck = @"SELECT id FROM carrinho
                                WHERE usuario_id = @uid AND tipo_item = 'viagem_personalizada' AND item_id = @iid";
            using MySqlCommand cmdCheck = new MySqlCommand(sqlCheck, conn);
            cmdCheck.Parameters.AddWithValue("@uid", usuarioId);
            cmdCheck.Parameters.AddWithValue("@iid", viagemId);
            object? existente = cmdCheck.ExecuteScalar();

            if (existente == null)
            {
                string sqlIns = @"INSERT INTO carrinho
                                  (usuario_id, tipo_item, item_id, nome_item, preco_unitario, quantidade)
                                  VALUES (@uid, 'viagem_personalizada', @iid, @nome, 0.00, 1)";
                using MySqlCommand cmdIns = new MySqlCommand(sqlIns, conn);
                cmdIns.Parameters.AddWithValue("@uid", usuarioId);
                cmdIns.Parameters.AddWithValue("@iid", viagemId);
                cmdIns.Parameters.AddWithValue("@nome", nomeViagem);
                cmdIns.ExecuteNonQuery();
            }

            TempData["Sucesso"] = "Viagem personalizada adicionada ao carrinho!";
            return RedirectToAction("Index", "Carrinho");
        }

        // ============================================================
        // POST: /Carrinho/Remover
        // ============================================================
        [HttpPost]
        public IActionResult Remover(int id)
        {
            int? usuarioId = HttpContext.Session.GetInt32("UserId");
            if (usuarioId == null)
                return RedirectToAction("Index", "Login");

            using MySqlConnection conn = new MySqlConnection(conexao);
            conn.Open();

            string sql = "DELETE FROM carrinho WHERE id = @id AND usuario_id = @uid";
            using MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@uid", usuarioId);
            cmd.ExecuteNonQuery();

            TempData["Sucesso"] = "Item removido do carrinho.";
            return RedirectToAction("Index");
        }

        // ============================================================
        // POST: /Carrinho/AlterarQuantidade  (delta = +1 ou -1)
        // ============================================================
        [HttpPost]
        public IActionResult AlterarQuantidade(int id, int delta)
        {
            int? usuarioId = HttpContext.Session.GetInt32("UserId");
            if (usuarioId == null)
                return RedirectToAction("Index", "Login");

            using MySqlConnection conn = new MySqlConnection(conexao);
            conn.Open();

            string sqlQ = "SELECT quantidade FROM carrinho WHERE id = @id AND usuario_id = @uid";
            using MySqlCommand cmdQ = new MySqlCommand(sqlQ, conn);
            cmdQ.Parameters.AddWithValue("@id", id);
            cmdQ.Parameters.AddWithValue("@uid", usuarioId);
            object? res = cmdQ.ExecuteScalar();

            if (res != null)
            {
                int novaQtd = Convert.ToInt32(res) + delta;

                if (novaQtd <= 0)
                {
                    string sqlDel = "DELETE FROM carrinho WHERE id = @id AND usuario_id = @uid";
                    using MySqlCommand cmdDel = new MySqlCommand(sqlDel, conn);
                    cmdDel.Parameters.AddWithValue("@id", id);
                    cmdDel.Parameters.AddWithValue("@uid", usuarioId);
                    cmdDel.ExecuteNonQuery();
                }
                else
                {
                    string sqlUp = "UPDATE carrinho SET quantidade = @q WHERE id = @id AND usuario_id = @uid";
                    using MySqlCommand cmdUp = new MySqlCommand(sqlUp, conn);
                    cmdUp.Parameters.AddWithValue("@q", novaQtd);
                    cmdUp.Parameters.AddWithValue("@id", id);
                    cmdUp.Parameters.AddWithValue("@uid", usuarioId);
                    cmdUp.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index");
        }

        // ============================================================
        // POST: /Carrinho/Limpar
        // ============================================================
        [HttpPost]
        public IActionResult Limpar()
        {
            int? usuarioId = HttpContext.Session.GetInt32("UserId");
            if (usuarioId == null)
                return RedirectToAction("Index", "Login");

            using MySqlConnection conn = new MySqlConnection(conexao);
            conn.Open();

            string sql = "DELETE FROM carrinho WHERE usuario_id = @uid";
            using MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uid", usuarioId);
            cmd.ExecuteNonQuery();

            TempData["Sucesso"] = "Carrinho esvaziado com sucesso.";
            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult FinalizarPedido()
        {
            int? usuarioId = HttpContext.Session.GetInt32("UserId");
            if (usuarioId == null)
                return RedirectToAction("Index", "Login");

            using MySqlConnection conn = new MySqlConnection(conexao);
            conn.Open();

            string sql = @"SELECT tipo_item, item_id, nome_item, preco_unitario, quantidade
                   FROM carrinho WHERE usuario_id = @uid";

            List<CarrinhoItem> itens = new();

            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@uid", usuarioId);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        itens.Add(new CarrinhoItem
                        {
                            TipoItem = reader["tipo_item"].ToString()!,
                            ItemId = Convert.ToInt32(reader["item_id"]),
                            NomeItem = reader["nome_item"].ToString()!,
                            PrecoUnitario = Convert.ToDecimal(reader["preco_unitario"]),
                            Quantidade = Convert.ToInt32(reader["quantidade"])
                        });
                    }
                }
            }

            if (!itens.Any())
            {
                TempData["Erro"] = "Seu carrinho está vazio.";
                return RedirectToAction("Index");
            }

            // Guarda o resumo ANTES de limpar, pra mostrar na tela de confirmação ("Resumo Final")
            var resumoFinal = itens.Select(i => new ResumoPedidoItem
            {
                NomeItem = i.NomeItem,
                TipoItem = i.TipoItem,
                Quantidade = i.Quantidade,
                PrecoUnitario = i.PrecoUnitario,
                Subtotal = i.Subtotal
            }).ToList();

            HttpContext.Session.SetString("UltimoPedidoResumo", System.Text.Json.JsonSerializer.Serialize(resumoFinal));

            foreach (var item in itens)
            {
                string insert = @"INSERT INTO pedidos
                  (usuario_id, tipo_item, item_id, nome_item, preco_unitario, quantidade)
                  VALUES (@uid, @tipo, @itemId, @nome, @preco, @qtd)";

                using MySqlCommand cmdIns = new MySqlCommand(insert, conn);
                cmdIns.Parameters.AddWithValue("@uid", usuarioId);
                cmdIns.Parameters.AddWithValue("@tipo", item.TipoItem);
                cmdIns.Parameters.AddWithValue("@itemId", item.ItemId);
                cmdIns.Parameters.AddWithValue("@nome", item.NomeItem);
                cmdIns.Parameters.AddWithValue("@preco", item.PrecoUnitario);
                cmdIns.Parameters.AddWithValue("@qtd", item.Quantidade);
                cmdIns.ExecuteNonQuery();
            }

            string delete = "DELETE FROM carrinho WHERE usuario_id = @uid";
            using MySqlCommand cmdDel = new MySqlCommand(delete, conn);
            cmdDel.Parameters.AddWithValue("@uid", usuarioId);
            cmdDel.ExecuteNonQuery();

            return RedirectToAction("PedidoConfirmado");
        }

        // ETAPA FINAL — RESUMO FINAL (depois de "pagamento e carrinho")
        [HttpGet]
        public IActionResult PedidoConfirmado()
        {
            var json = HttpContext.Session.GetString("UltimoPedidoResumo");
            if (string.IsNullOrEmpty(json))
                return RedirectToAction("Index");

            var itens = System.Text.Json.JsonSerializer.Deserialize<List<ResumoPedidoItem>>(json) ?? new();
            HttpContext.Session.Remove("UltimoPedidoResumo");

            return View(itens);
        }
    }
}