using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System;

namespace projectPartiuDestino.Controllers
{
    public class PacotesAdminController : Controller
    {
        private string conexao = "server=localhost;database=bdpartiudestino;uid=root;pwd=12345678;";

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            using (MySqlConnection conn = new MySqlConnection(conexao))
            {
                conn.Open();

                string sql = "DELETE FROM pacotes WHERE id = @id";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index");
        }
        public IActionResult Criar()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Criar(
    int destino_id,
    string nome,
    string descricao,
    string tipo_viagem,
    int duracao_dias,
    DateTime data_partida,
    DateTime data_retorno,
    decimal preco_por_pessoa,
    int vagas_disponiveis)
        {
            using (MySqlConnection conn = new MySqlConnection(conexao))
            {
                conn.Open();

                string sql = @"
            INSERT INTO pacotes
            (
                destino_id,
                nome,
                descricao,
                tipo_viagem,
                duracao_dias,
                data_partida,
                data_retorno,
                preco_por_pessoa,
                vagas_disponiveis
            )
            VALUES
            (
                @destino_id,
                @nome,
                @descricao,
                @tipo_viagem,
                @duracao_dias,
                @data_partida,
                @data_retorno,
                @preco_por_pessoa,
                @vagas_disponiveis
            )";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@destino_id", destino_id);
                    cmd.Parameters.AddWithValue("@nome", nome);
                    cmd.Parameters.AddWithValue("@descricao", descricao);
                    cmd.Parameters.AddWithValue("@tipo_viagem", tipo_viagem);
                    cmd.Parameters.AddWithValue("@duracao_dias", duracao_dias);
                    cmd.Parameters.AddWithValue("@data_partida", data_partida);
                    cmd.Parameters.AddWithValue("@data_retorno", data_retorno);
                    cmd.Parameters.AddWithValue("@preco_por_pessoa", preco_por_pessoa);
                    cmd.Parameters.AddWithValue("@vagas_disponiveis", vagas_disponiveis);

                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index");
        }
    }
}