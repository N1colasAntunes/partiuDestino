using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using projectPartiuDestino.Models;

namespace projectPartiuDestino.Controllers
{
    public class PedidosFinalizadosController : Controller
    {
        private string conexao = "server=localhost;database=bdpartiudestino;uid=root;pwd=12345678;";

        public IActionResult Index()
        {
            List<Pedidos> lista = new();

            using (MySqlConnection conn = new MySqlConnection(conexao))
            {
                conn.Open();

                string sql = @"SELECT p.*, u.nome AS nome_usuario
                               FROM pedidos p
                               INNER JOIN usuarios u ON u.id = p.usuario_id
                               ORDER BY p.data_pedido DESC";

                using MySqlCommand cmd = new MySqlCommand(sql, conn);

                using MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new Pedidos
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        UsuarioId = Convert.ToInt32(reader["usuario_id"]),
                        NomeUsuario = reader["nome_usuario"].ToString()!,
                        TipoItem = reader["tipo_item"].ToString()!,
                        NomeItem = reader["nome_item"].ToString()!,
                        Quantidade = Convert.ToInt32(reader["quantidade"]),
                        PrecoUnitario = Convert.ToDecimal(reader["preco_unitario"]),
                        DataPedido = Convert.ToDateTime(reader["data_pedido"])
                    });
                }
            }

            return View(lista);
        }
    }
}