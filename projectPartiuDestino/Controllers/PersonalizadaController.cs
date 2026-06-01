using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using projectPartiuDestino.Autenticacao;

namespace projectPartiuDestino.Controllers
{
    public class PersonalizadaController : Controller
    {
        private string conexao = "server=localhost;database=bdpartiudestino;uid=root;pwd=12345678;";

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(
            string nomeCompleto,
            string cpf,
            string email,
            string whatsapp,
            string destino,
            string hospedagem,
            DateTime dataPartida,
            int duracaoDias,
            string climaViagem,
            string orcamento,
            int adultos,
            int criancas,
            string desejosEspeciais)
        {
            int? usuarioId = HttpContext.Session.GetInt32("UserId");

            if (usuarioId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            string conexao = "server=localhost;database=bdpartiudestino;uid=root;pwd=12345678;";

            using (MySqlConnection conn = new MySqlConnection(conexao))
            {
                conn.Open();

                string sql = @"
        INSERT INTO viagem_personalizada
        (
            usuario_id,
            nome_completo,
            cpf,
            email,
            whatsapp,
            destino,
            hospedagem,
            data_partida,
            duracao_dias,
            clima_viagem,
            orcamento,
            adultos,
            criancas,
            desejos_especiais
        )
        VALUES
        (
            @usuario_id,
            @nome_completo,
            @cpf,
            @email,
            @whatsapp,
            @destino,
            @hospedagem,
            @data_partida,
            @duracao_dias,
            @clima_viagem,
            @orcamento,
            @adultos,
            @criancas,
            @desejos_especiais
        )";

                MySqlCommand cmd = new MySqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@usuario_id", usuarioId);

                cmd.Parameters.AddWithValue("@nome_completo", nomeCompleto);
                cmd.Parameters.AddWithValue("@cpf", cpf);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@whatsapp", whatsapp);

                cmd.Parameters.AddWithValue("@destino", destino);
                cmd.Parameters.AddWithValue("@hospedagem", hospedagem);
                cmd.Parameters.AddWithValue("@data_partida", dataPartida);
                cmd.Parameters.AddWithValue("@duracao_dias", duracaoDias);

                cmd.Parameters.AddWithValue("@clima_viagem", climaViagem);
                cmd.Parameters.AddWithValue("@orcamento", orcamento);

                cmd.Parameters.AddWithValue("@adultos", adultos);
                cmd.Parameters.AddWithValue("@criancas", criancas);

                cmd.Parameters.AddWithValue("@desejos_especiais", desejosEspeciais);

                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index", "Home");
        }
    }
}