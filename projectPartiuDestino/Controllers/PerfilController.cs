using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using projectPartiuDestino.Models;

namespace projectPartiuDestino.Controllers
{
    public class PerfilController : Controller
    {
        private string connStr = "server=localhost;database=bdpartiudestino;uid=root;pwd=12345678;";

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Index", "Login");

            var pedidos = new List<PedidoConfirmado>();

            using var conn = new MySqlConnection(connStr);
            conn.Open();

            string sql = @"
    SELECT 
        id,
        nome_item,
        data_pedido,
        preco_unitario,
        quantidade
    FROM pedidos
    WHERE usuario_id = @id
    ORDER BY id DESC";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", userId);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                pedidos.Add(new PedidoConfirmado
                {
                    Id = Convert.ToInt32(reader["id"]),
                    NomeItem = reader["nome_item"].ToString(),
                    DataPedido = Convert.ToDateTime(reader["data_pedido"]),
                    PrecoUnitario = Convert.ToDecimal(reader["preco_unitario"]),
                    Quantidade = Convert.ToInt32(reader["quantidade"])
                });
            }

            return View(pedidos);
        }
    }
}