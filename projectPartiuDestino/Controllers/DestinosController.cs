using System.Text.Json;
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
                    sql += " AND CONCAT(origem_pais, ' - ', origem_estado) = @origem";

                if (!string.IsNullOrEmpty(destino))
                    sql += " AND CONCAT(pais, ' - ', estado) = @destino";

                using MySqlCommand cmd = new MySqlCommand(sql, conn);
                if (!string.IsNullOrEmpty(origem))
                    cmd.Parameters.AddWithValue("@origem", origem);
                if (!string.IsNullOrEmpty(destino))
                    cmd.Parameters.AddWithValue("@destino", destino);

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
                        PrecoPorPessoa = Convert.ToDecimal(reader["preco_por_pessoa"])
                    });
                }
            }

            return View(listaDestinos);
        }

        // =========================================================
        // ETAPA 1 — PASSAGENS (classe + assentos + viajantes)
        // CORRIGIDO: antes este método tratava o id do DESTINO como
        // se fosse id de PACOTE (bug). Agora consulta direto em "destinos".
        // =========================================================
        [HttpGet]
        public IActionResult Passagem(int id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Index", "Login");

            Destinos? destino = null;
            List<string> assentosOcupados = new();

            using (MySqlConnection conn = new MySqlConnection(conexao))
            {
                conn.Open();

                string sql = @"SELECT id, origem_pais, origem_estado, pais, estado,
                                      imagem_url, preco_por_pessoa
                               FROM destinos WHERE id = @id";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        destino = new Destinos
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            OrigemPais = reader["origem_pais"].ToString(),
                            OrigemEstado = reader["origem_estado"].ToString(),
                            Pais = reader["pais"].ToString(),
                            Estado = reader["estado"].ToString(),
                            ImagemUrl = reader["imagem_url"]?.ToString() ?? "",
                            PrecoPorPessoa = Convert.ToDecimal(reader["preco_por_pessoa"])
                        };
                    }
                }

                if (destino == null)
                    return NotFound();

                string nomeDestino = $"{destino.Pais} - {destino.Estado}";
                string sqlAssentos = "SELECT nome_item FROM pedidos WHERE tipo_item = 'destino' AND nome_item LIKE @pattern";
                using (var cmdA = new MySqlCommand(sqlAssentos, conn))
                {
                    cmdA.Parameters.AddWithValue("@pattern", $"%{nomeDestino}%Assento:%");
                    using var readerA = cmdA.ExecuteReader();
                    while (readerA.Read())
                    {
                        string nomeItem = readerA["nome_item"].ToString()!;
                        var parts = nomeItem.Split("Assento:");
                        if (parts.Length > 1)
                        {
                            foreach (var a in parts[1].Split(","))
                            {
                                var limpo = a.Trim();
                                if (!string.IsNullOrEmpty(limpo))
                                    assentosOcupados.Add(limpo);
                            }
                        }
                    }
                }
            }

            ViewBag.AssentosOcupados = assentosOcupados;
            return View(destino);
        }

        [HttpPost]
        public IActionResult Passagem(
            int destinoId,
            string classeViagem,
            string numeroAssento,
            int quantidadeAdultos,
            int quantidadeCriancas)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Index", "Login");

            if (quantidadeAdultos < 1)
            {
                TempData["Erro"] = "Informe pelo menos 1 adulto para continuar.";
                return RedirectToAction("Passagem", new { id = destinoId });
            }

            if (quantidadeCriancas < 0)
            {
                TempData["Erro"] = "A quantidade de crianças não pode ser negativa.";
                return RedirectToAction("Passagem", new { id = destinoId });
            }

            int quantidadeTotal = quantidadeAdultos + quantidadeCriancas;

            if (string.IsNullOrWhiteSpace(numeroAssento))
            {
                TempData["Erro"] = "Selecione os assentos antes de continuar.";
                return RedirectToAction("Passagem", new { id = destinoId });
            }

            var assentosSelecionados = numeroAssento
                .Split(",")
                .Select(a => a.Trim())
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .Distinct()
                .ToList();

            if (assentosSelecionados.Count != quantidadeTotal)
            {
                TempData["Erro"] = $"Você informou {quantidadeTotal} viajante(s), então precisa escolher {quantidadeTotal} assento(s).";
                return RedirectToAction("Passagem", new { id = destinoId });
            }

            using (MySqlConnection conn = new MySqlConnection(conexao))
            {
                conn.Open();

                string nomeDestino;
                string sqlNome = "SELECT pais, estado FROM destinos WHERE id = @id";
                using (var cmdNome = new MySqlCommand(sqlNome, conn))
                {
                    cmdNome.Parameters.AddWithValue("@id", destinoId);
                    using var r = cmdNome.ExecuteReader();
                    if (!r.Read())
                    {
                        TempData["Erro"] = "Destino não encontrado.";
                        return RedirectToAction("Index");
                    }
                    nomeDestino = $"{r["pais"]} - {r["estado"]}";
                }

                foreach (var assento in assentosSelecionados)
                {
                    string sqlCheck = "SELECT COUNT(*) FROM pedidos WHERE tipo_item = 'destino' AND nome_item LIKE @pattern";
                    using var cmdCheck = new MySqlCommand(sqlCheck, conn);
                    cmdCheck.Parameters.AddWithValue("@pattern", $"%Assento: %{assento}%");
                    long count = Convert.ToInt64(cmdCheck.ExecuteScalar());
                    if (count > 0)
                    {
                        TempData["Erro"] = $"O assento {assento} já foi selecionado por outro usuário. Escolha outro.";
                        return RedirectToAction("Passagem", new { id = destinoId });
                    }
                }
            }

            decimal precoAdicionalPorPessoa = classeViagem switch
            {
                "Executiva" => 450.00m,
                "Primeira Classe" => 1200.00m,
                _ => 0.00m
            };

            var selecao = new SelecaoVoo
            {
                ItemId = destinoId,
                TipoItem = "destino",
                ClasseViagem = classeViagem,
                TipoAssento = "Múltiplos",
                NumeroAssento = string.Join(", ", assentosSelecionados),
                PrecoAdicional = precoAdicionalPorPessoa * quantidadeTotal,
                QuantidadeAdultos = quantidadeAdultos,
                QuantidadeCriancas = quantidadeCriancas,
                QuantidadeTotal = quantidadeTotal
            };

            HttpContext.Session.SetString(
                $"Voo_destino_{destinoId}",
                JsonSerializer.Serialize(selecao)
            );

            return RedirectToAction("Detalhes", new { id = destinoId });
        }

        // =========================================================
        // ETAPA 2 — INFORMAÇÕES E DETALHES + HOSPEDAGEM (OPCIONAL)
        // =========================================================
        public IActionResult Detalhes(int id)
        {
            if (HttpContext.Session.GetString("UserName") == null)
                return RedirectToAction("Index", "Login");

            string? vooJson = HttpContext.Session.GetString($"Voo_destino_{id}");
            if (string.IsNullOrEmpty(vooJson))
            {
                TempData["Erro"] = "Selecione primeiro sua passagem para continuar.";
                return RedirectToAction("Passagem", new { id });
            }

            Destinos? destino = null;

            using (MySqlConnection conn = new MySqlConnection(conexao))
            {
                conn.Open();

                string sql = @"SELECT id, origem_pais, origem_estado, pais, estado,
                                      imagem_url, preco_por_pessoa
                               FROM destinos WHERE id = @id";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        destino = new Destinos
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            OrigemPais = reader["origem_pais"].ToString(),
                            OrigemEstado = reader["origem_estado"].ToString(),
                            Pais = reader["pais"].ToString(),
                            Estado = reader["estado"].ToString(),
                            ImagemUrl = reader["imagem_url"]?.ToString() ?? "",
                            PrecoPorPessoa = Convert.ToDecimal(reader["preco_por_pessoa"])
                        };
                    }
                }

                if (destino == null)
                    return NotFound();

                string sqlHosp = "SELECT * FROM hospedagens WHERE destino_id = @id";
                using (var cmdH = new MySqlCommand(sqlHosp, conn))
                {
                    cmdH.Parameters.AddWithValue("@id", id);
                    using var readerH = cmdH.ExecuteReader();
                    while (readerH.Read())
                    {
                        destino.Hospedagens.Add(new Hospedagem
                        {
                            Id = Convert.ToInt32(readerH["id"]),
                            DestinoId = id,
                            Nome = readerH["nome"].ToString()!,
                            Categoria = readerH["categoria"]?.ToString(),
                            Descricao = readerH["descricao"]?.ToString(),
                            Endereco = readerH["endereco"]?.ToString(),
                            ImagemUrl = readerH["imagem_url"]?.ToString(),
                            Checkin = readerH["checkin"]?.ToString(),
                            Checkout = readerH["checkout"]?.ToString(),
                            CafeIncluso = Convert.ToBoolean(readerH["cafe_incluso"]),
                            WifiIncluso = Convert.ToBoolean(readerH["wifi_incluso"]),
                            Estacionamento = Convert.ToBoolean(readerH["estacionamento"]),
                            Avaliacao = readerH["avaliacao"] == DBNull.Value ? null : Convert.ToDecimal(readerH["avaliacao"]),
                            Comodidades = readerH["comodidades"]?.ToString()
                        });
                    }
                }
            }

            ViewBag.Voo = JsonSerializer.Deserialize<SelecaoVoo>(vooJson);
            return View(destino);
        }

        // =========================================================
        // ETAPA 3 — Quartos da hospedagem escolhida
        //           (Políticas/Avisos e Documentos ficam em MODAL aqui)
        // =========================================================
        public IActionResult DetalhesHospedagem(int destinoId, int hospedagemId)
        {
            if (HttpContext.Session.GetString("UserName") == null)
                return RedirectToAction("Index", "Login");

            string? vooJson = HttpContext.Session.GetString($"Voo_destino_{destinoId}");
            if (string.IsNullOrEmpty(vooJson))
            {
                TempData["Erro"] = "Selecione primeiro sua passagem para continuar.";
                return RedirectToAction("Passagem", new { id = destinoId });
            }

            var voo = JsonSerializer.Deserialize<SelecaoVoo>(vooJson);

            Destinos? destino = null;
            Hospedagem? hospedagem = null;

            using (MySqlConnection conn = new MySqlConnection(conexao))
            {
                conn.Open();

                string sqlDestino = "SELECT id, pais, estado, preco_por_pessoa FROM destinos WHERE id = @id";
                using (var cmd = new MySqlCommand(sqlDestino, conn))
                {
                    cmd.Parameters.AddWithValue("@id", destinoId);
                    using var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        destino = new Destinos
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Pais = reader["pais"].ToString(),
                            Estado = reader["estado"].ToString(),
                            PrecoPorPessoa = Convert.ToDecimal(reader["preco_por_pessoa"])
                        };
                    }
                }

                if (destino == null) return NotFound();

                string sqlHosp = "SELECT * FROM hospedagens WHERE id = @hid AND destino_id = @did";
                using (var cmdH = new MySqlCommand(sqlHosp, conn))
                {
                    cmdH.Parameters.AddWithValue("@hid", hospedagemId);
                    cmdH.Parameters.AddWithValue("@did", destinoId);
                    using var reader = cmdH.ExecuteReader();
                    if (reader.Read())
                    {
                        hospedagem = new Hospedagem
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            DestinoId = destinoId,
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

                if (hospedagem == null) return NotFound();

                string sqlQuartos = "SELECT * FROM quartos WHERE hospedagem_id = @hid ORDER BY preco_adicional";
                using (var cmdQ = new MySqlCommand(sqlQuartos, conn))
                {
                    cmdQ.Parameters.AddWithValue("@hid", hospedagemId);
                    using var reader = cmdQ.ExecuteReader();
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

            ViewBag.Destino = destino;
            ViewBag.Voo = voo;
            return View(hospedagem);
        }

        public IActionResult Buscar(string termo)
        {
            string conexao = "server=localhost;database=bdpartiudestino;uid=root;pwd=12345678;";
            List<Destinos> destinos = new();

            using (MySqlConnection conn = new MySqlConnection(conexao))
            {
                conn.Open();
                string sql = "SELECT * FROM destinos WHERE pais LIKE @termo OR estado LIKE @termo";
                MySqlCommand cmd = new(sql, conn);
                cmd.Parameters.AddWithValue("@termo", "%" + termo + "%");
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