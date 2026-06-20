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

            return View(itens);
        }

        // ============================================================
        // POST: /Carrinho/AdicionarPacote
        // ============================================================
        [HttpPost]
        public IActionResult AdicionarPacote(int pacoteId, int? quartoId)
        {
            int? usuarioId = HttpContext.Session.GetInt32("UserId");
            if (usuarioId == null)
                return RedirectToAction("Index", "Login");

            using MySqlConnection conn = new MySqlConnection(conexao);
            conn.Open();

            string sqlPacote = "SELECT nome, preco_por_pessoa FROM pacotes WHERE id = @id";
            using MySqlCommand cmdP = new MySqlCommand(sqlPacote, conn);
            cmdP.Parameters.AddWithValue("@id", pacoteId);

            string nomePacote = "";
            decimal preco = 0;

            using (MySqlDataReader r = cmdP.ExecuteReader())
            {
                if (r.Read())
                {
                    nomePacote = r["nome"].ToString()!;
                    preco = Convert.ToDecimal(r["preco_por_pessoa"]);
                }
                else
                {
                    TempData["Erro"] = "Pacote não encontrado.";
                    return RedirectToAction("Index", "Pacotes");
                }
            }

            // ── ADICIONADO: soma o valor do quarto escolhido (se houver) ──
            string nomeItem = nomePacote;
            if (quartoId.HasValue)
            {
                string sqlQuarto = "SELECT tipo_quarto, preco_adicional FROM quartos WHERE id = @id";
                using MySqlCommand cmdQ = new MySqlCommand(sqlQuarto, conn);
                cmdQ.Parameters.AddWithValue("@id", quartoId.Value);

                using MySqlDataReader rq = cmdQ.ExecuteReader();
                if (rq.Read())
                {
                    string tipoQuarto = rq["tipo_quarto"].ToString()!;
                    preco += Convert.ToDecimal(rq["preco_adicional"]);
                    nomeItem = $"{nomePacote} — Quarto: {tipoQuarto}";
                }
            }

            // Verifica se já existe esse MESMO pacote (com o mesmo quarto) no carrinho
            string sqlCheck = @"SELECT id FROM carrinho
                        WHERE usuario_id = @uid AND tipo_item = 'pacote'
                          AND item_id = @iid AND nome_item = @nome";
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
                string sqlIns = @"INSERT INTO carrinho
                          (usuario_id, tipo_item, item_id, nome_item, preco_unitario, quantidade)
                          VALUES (@uid, 'pacote', @iid, @nome, @preco, 1)";
                using MySqlCommand cmdIns = new MySqlCommand(sqlIns, conn);
                cmdIns.Parameters.AddWithValue("@uid", usuarioId);
                cmdIns.Parameters.AddWithValue("@iid", pacoteId);
                cmdIns.Parameters.AddWithValue("@nome", nomeItem);
                cmdIns.Parameters.AddWithValue("@preco", preco);
                cmdIns.ExecuteNonQuery();
            }

            TempData["Sucesso"] = $"{nomeItem} adicionado ao carrinho!";
            return RedirectToAction("Index", "Carrinho");
        }

        // ============================================================
        // POST: /Carrinho/AdicionarDestino
        // ATUALIZADO: agora salva o preco_por_pessoa do destino
        // ============================================================
        [HttpPost]
        public IActionResult AdicionarDestino(int destinoId)
        {
            int? usuarioId = HttpContext.Session.GetInt32("UserId");
            if (usuarioId == null)
                return RedirectToAction("Index", "Login");

            using MySqlConnection conn = new MySqlConnection(conexao);
            conn.Open();

            // ATUALIZADO: busca nome E preco_por_pessoa do destino
            string sqlD = "SELECT pais, estado, preco_por_pessoa FROM destinos WHERE id = @id";
            using MySqlCommand cmdD = new MySqlCommand(sqlD, conn);
            cmdD.Parameters.AddWithValue("@id", destinoId);

            string nomeDestino = "";
            decimal preco = 0;

            using (MySqlDataReader r = cmdD.ExecuteReader())
            {
                if (r.Read())
                {
                    nomeDestino = $"{r["estado"]} - {r["pais"]}";
                    preco = Convert.ToDecimal(r["preco_por_pessoa"]);  // ADICIONADO
                }
                else
                {
                    TempData["Erro"] = "Destino não encontrado.";
                    return RedirectToAction("Index", "Destinos");
                }
            }

            // Verifica se já existe no carrinho
            string sqlCheck = @"SELECT id FROM carrinho
                                WHERE usuario_id = @uid AND tipo_item = 'destino' AND item_id = @iid";
            using MySqlCommand cmdCheck = new MySqlCommand(sqlCheck, conn);
            cmdCheck.Parameters.AddWithValue("@uid", usuarioId);
            cmdCheck.Parameters.AddWithValue("@iid", destinoId);
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
                // ATUALIZADO: preco agora vem do banco em vez de 0.00
                string sqlIns = @"INSERT INTO carrinho
                                  (usuario_id, tipo_item, item_id, nome_item, preco_unitario, quantidade)
                                  VALUES (@uid, 'destino', @iid, @nome, @preco, 1)";
                using MySqlCommand cmdIns = new MySqlCommand(sqlIns, conn);
                cmdIns.Parameters.AddWithValue("@uid", usuarioId);
                cmdIns.Parameters.AddWithValue("@iid", destinoId);
                cmdIns.Parameters.AddWithValue("@nome", nomeDestino);
                cmdIns.Parameters.AddWithValue("@preco", preco);         // ATUALIZADO
                cmdIns.ExecuteNonQuery();
            }

            TempData["Sucesso"] = $"{nomeDestino} adicionado ao carrinho!";
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

            // 1. Buscar itens do carrinho
            string sql = @"SELECT tipo_item, item_id, nome_item, preco_unitario, quantidade
                   FROM carrinho
                   WHERE usuario_id = @uid";

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

            // 2. Inserir no "pedidos"
            foreach (var item in itens)
            {
                string insert = @"INSERT INTO pedidos
                          (usuario_id, tipo_item, item_id, nome_item, preco_unitario, quantidade)
                          VALUES
                          (@uid, @tipo, @itemId, @nome, @preco, @qtd)";

                using MySqlCommand cmdIns = new MySqlCommand(insert, conn);
                cmdIns.Parameters.AddWithValue("@uid", usuarioId);
                cmdIns.Parameters.AddWithValue("@tipo", item.TipoItem);
                cmdIns.Parameters.AddWithValue("@itemId", item.ItemId);
                cmdIns.Parameters.AddWithValue("@nome", item.NomeItem);
                cmdIns.Parameters.AddWithValue("@preco", item.PrecoUnitario);
                cmdIns.Parameters.AddWithValue("@qtd", item.Quantidade);

                cmdIns.ExecuteNonQuery();
            }

            // 3. Limpar carrinho
            string delete = "DELETE FROM carrinho WHERE usuario_id = @uid";
            using MySqlCommand cmdDel = new MySqlCommand(delete, conn);
            cmdDel.Parameters.AddWithValue("@uid", usuarioId);
            cmdDel.ExecuteNonQuery();

            TempData["Sucesso"] = "Pedido finalizado com sucesso!";
            return RedirectToAction("Index");
        }
    }
}