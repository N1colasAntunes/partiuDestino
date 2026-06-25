using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using projectPartiuDestino.Models;

namespace projectPartiuDestino.Controllers
{
    public class PagamentoController : Controller
    {
        private readonly string conexao = "server=localhost;database=bdpartiudestino;uid=root;pwd=12345678;";

        // ============================================================
        // GET: /Pagamento/Index
        // Mostra o resumo do carrinho antes do pagamento
        // ============================================================
        [HttpGet]
        public IActionResult Index()
        {
            int? usuarioId = HttpContext.Session.GetInt32("UserId");

            if (usuarioId == null)
                return RedirectToAction("Index", "Login");

            var itens = BuscarItensCarrinho(usuarioId.Value);

            if (!itens.Any())
            {
                TempData["Erro"] = "Seu carrinho está vazio. Escolha uma viagem antes de continuar.";
                return RedirectToAction("Index", "Carrinho");
            }

            var model = new PagamentoResumoViewModel
            {
                Itens = itens,
                Total = itens.Sum(i => i.Subtotal),
                CodigoReserva = GerarCodigoReserva(),
                FormaPagamento = "Cartao",
                Parcelas = 1
            };

            return View(model);
        }

        // ============================================================
        // POST: /Pagamento/Confirmar
        // Simula aprovação do pagamento, grava pedido e limpa carrinho
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Confirmar(PagamentoResumoViewModel model)
        {
            int? usuarioId = HttpContext.Session.GetInt32("UserId");

            if (usuarioId == null)
                return RedirectToAction("Index", "Login");

            var itens = BuscarItensCarrinho(usuarioId.Value);

            if (!itens.Any())
            {
                TempData["Erro"] = "Não há itens no carrinho para pagamento.";
                return RedirectToAction("Index", "Carrinho");
            }

            decimal total = itens.Sum(i => i.Subtotal);

            if (string.IsNullOrWhiteSpace(model.FormaPagamento))
            {
                TempData["Erro"] = "Escolha uma forma de pagamento para continuar.";
                return RedirectToAction("Index");
            }

            if (model.FormaPagamento == "Cartao")
            {
                if (string.IsNullOrWhiteSpace(model.NomeTitular) ||
                    string.IsNullOrWhiteSpace(model.DocumentoTitular) ||
                    string.IsNullOrWhiteSpace(model.NumeroCartao) ||
                    string.IsNullOrWhiteSpace(model.Vencimento) ||
                    string.IsNullOrWhiteSpace(model.Cvv))
                {
                    TempData["Erro"] = "Preencha os dados do cartão para simular o pagamento.";
                    return RedirectToAction("Index");
                }

                if (model.Parcelas < 1)
                    model.Parcelas = 1;
            }
            else
            {
                model.Parcelas = 1;
            }

            string codigoReserva = GerarCodigoReserva();
            string comprovante = GerarComprovante(model.FormaPagamento);
            string statusPagamento = DefinirStatusPagamento(model.FormaPagamento, total);

            using MySqlConnection conn = new MySqlConnection(conexao);
            conn.Open();

            using MySqlTransaction transaction = conn.BeginTransaction();

            try
            {
                foreach (var item in itens)
                {
                    string insert = @"
                        INSERT INTO pedidos
                        (
                            usuario_id,
                            codigo_reserva,
                            tipo_item,
                            item_id,
                            nome_item,
                            preco_unitario,
                            quantidade,
                            forma_pagamento,
                            status_pagamento,
                            valor_total_pedido,
                            parcelas,
                            comprovante,
                            data_pagamento
                        )
                        VALUES
                        (
                            @usuarioId,
                            @codigoReserva,
                            @tipoItem,
                            @itemId,
                            @nomeItem,
                            @precoUnitario,
                            @quantidade,
                            @formaPagamento,
                            @statusPagamento,
                            @valorTotalPedido,
                            @parcelas,
                            @comprovante,
                            NOW()
                        )";

                    using MySqlCommand cmdInsert = new MySqlCommand(insert, conn, transaction);
                    cmdInsert.Parameters.AddWithValue("@usuarioId", usuarioId.Value);
                    cmdInsert.Parameters.AddWithValue("@codigoReserva", codigoReserva);
                    cmdInsert.Parameters.AddWithValue("@tipoItem", item.TipoItem);
                    cmdInsert.Parameters.AddWithValue("@itemId", item.ItemId);
                    cmdInsert.Parameters.AddWithValue("@nomeItem", item.NomeItem);
                    cmdInsert.Parameters.AddWithValue("@precoUnitario", item.PrecoUnitario);
                    cmdInsert.Parameters.AddWithValue("@quantidade", item.Quantidade);
                    cmdInsert.Parameters.AddWithValue("@formaPagamento", model.FormaPagamento);
                    cmdInsert.Parameters.AddWithValue("@statusPagamento", statusPagamento);
                    cmdInsert.Parameters.AddWithValue("@valorTotalPedido", total);
                    cmdInsert.Parameters.AddWithValue("@parcelas", model.Parcelas);
                    cmdInsert.Parameters.AddWithValue("@comprovante", comprovante);
                    cmdInsert.ExecuteNonQuery();
                }

                string deleteCarrinho = "DELETE FROM carrinho WHERE usuario_id = @usuarioId";

                using MySqlCommand cmdDelete = new MySqlCommand(deleteCarrinho, conn, transaction);
                cmdDelete.Parameters.AddWithValue("@usuarioId", usuarioId.Value);
                cmdDelete.ExecuteNonQuery();

                transaction.Commit();

                TempData["Sucesso"] = $"Reserva {codigoReserva} confirmada com sucesso.";
                TempData["CodigoReserva"] = codigoReserva;

                var resumoFinal = itens.Select(i => new ResumoPedidoItem
                {
                    NomeItem = i.NomeItem,
                    TipoItem = i.TipoItem,
                    Quantidade = i.Quantidade,
                    PrecoUnitario = i.PrecoUnitario,
                    Subtotal = i.Subtotal
                }).ToList();

                HttpContext.Session.SetString(
                    "UltimoPedidoResumo",
                    System.Text.Json.JsonSerializer.Serialize(resumoFinal)
                );

                TempData["Sucesso"] = $"Reserva {codigoReserva} confirmada com sucesso.";
                TempData["CodigoReserva"] = codigoReserva;

                return RedirectToAction("PedidoConfirmado", "Carrinho");
            }
            catch
            {
                transaction.Rollback();

                TempData["Erro"] = "Não foi possível confirmar o pagamento. Tente novamente.";
                return RedirectToAction("Index");
            }
        }

        // ============================================================
        // Métodos auxiliares
        // ============================================================
        private List<CarrinhoItem> BuscarItensCarrinho(int usuarioId)
        {
            List<CarrinhoItem> itens = new();

            using MySqlConnection conn = new MySqlConnection(conexao);
            conn.Open();

            string sql = @"
                SELECT 
                    id,
                    usuario_id,
                    tipo_item,
                    item_id,
                    nome_item,
                    preco_unitario,
                    quantidade,
                    data_adicionado
                FROM carrinho
                WHERE usuario_id = @usuarioId
                ORDER BY data_adicionado DESC";

            using MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@usuarioId", usuarioId);

            using MySqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                itens.Add(new CarrinhoItem
                {
                    Id = Convert.ToInt32(reader["id"]),
                    UsuarioId = Convert.ToInt32(reader["usuario_id"]),
                    TipoItem = reader["tipo_item"].ToString() ?? "",
                    ItemId = Convert.ToInt32(reader["item_id"]),
                    NomeItem = reader["nome_item"].ToString() ?? "",
                    PrecoUnitario = Convert.ToDecimal(reader["preco_unitario"]),
                    Quantidade = Convert.ToInt32(reader["quantidade"]),
                    DataAdicionado = Convert.ToDateTime(reader["data_adicionado"])
                });
            }

            return itens;
        }

        private string GerarCodigoReserva()
        {
            string data = DateTime.Now.ToString("yyyyMMdd");
            string aleatorio = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();

            return $"PD-{data}-{aleatorio}";
        }

        private string GerarComprovante(string formaPagamento)
        {
            string prefixo = formaPagamento switch
            {
                "Pix" => "PIX",
                "Boleto" => "BOL",
                _ => "CARD"
            };

            return $"{prefixo}-{DateTime.Now:yyyyMMddHHmmss}";
        }

        private string DefinirStatusPagamento(string formaPagamento, decimal total)
        {
            if (total <= 0)
                return "Solicitação recebida";

            return formaPagamento switch
            {
                "Pix" => "Pagamento aprovado via Pix",
                "Boleto" => "Boleto gerado",
                _ => "Pagamento aprovado"
            };
        }
    }
}