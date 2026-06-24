using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using projectPartiuDestino.Models;
using Microsoft.AspNetCore.Http;

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
    ViagemPersonalizada viagem,
    string[] PreferenciasHospedagem)
        {
            int? usuarioId = HttpContext.Session.GetInt32("UserId");

            if (usuarioId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            viagem.PreferenciasHospedagem = string.Join(", ", PreferenciasHospedagem);

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

            origem,
            destino,
            regiao_interesse,
            data_partida,
            duracao_dias,
            transporte,

            tipo_hospedagem,
            categoria_hospedagem,
            preferencias_hospedagem,

            adultos,
            criancas,
            idosos,
            tipo_grupo,

            objetivo_viagem,
            ritmo_viagem,
            clima_viagem,

            faixa_orcamento,

            desejos_especiais
        )
        VALUES
        (
            @usuario_id,
            @nome_completo,
            @cpf,
            @email,
            @whatsapp,

            @origem,
            @destino,
            @regiao_interesse,
            @data_partida,
            @duracao_dias,
            @transporte,

            @tipo_hospedagem,
            @categoria_hospedagem,
            @preferencias_hospedagem,

            @adultos,
            @criancas,
            @idosos,
            @tipo_grupo,

            @objetivo_viagem,
            @ritmo_viagem,
            @clima_viagem,

            @faixa_orcamento,

            @desejos_especiais
        )";

                MySqlCommand cmd = new MySqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@usuario_id", usuarioId);

                cmd.Parameters.AddWithValue("@nome_completo", viagem.NomeCompleto);
                cmd.Parameters.AddWithValue("@cpf", viagem.Cpf);
                cmd.Parameters.AddWithValue("@email", viagem.Email);
                cmd.Parameters.AddWithValue("@whatsapp", viagem.Whatsapp);

                cmd.Parameters.AddWithValue("@origem", viagem.Origem);
                cmd.Parameters.AddWithValue("@destino", viagem.Destino);
                cmd.Parameters.AddWithValue("@regiao_interesse", viagem.RegiaoInteresse);
                cmd.Parameters.AddWithValue("@data_partida", viagem.DataPartida);
                cmd.Parameters.AddWithValue("@duracao_dias", viagem.DuracaoDias);
                cmd.Parameters.AddWithValue("@transporte", viagem.Transporte);

                cmd.Parameters.AddWithValue("@tipo_hospedagem", viagem.TipoHospedagem);
                cmd.Parameters.AddWithValue("@categoria_hospedagem", viagem.CategoriaHospedagem);
                cmd.Parameters.AddWithValue("@preferencias_hospedagem", viagem.PreferenciasHospedagem);

                cmd.Parameters.AddWithValue("@adultos", viagem.Adultos);
                cmd.Parameters.AddWithValue("@criancas", viagem.Criancas);
                cmd.Parameters.AddWithValue("@idosos", viagem.Idosos);
                cmd.Parameters.AddWithValue("@tipo_grupo", viagem.TipoGrupo);

                cmd.Parameters.AddWithValue("@objetivo_viagem", viagem.ObjetivoViagem);
                cmd.Parameters.AddWithValue("@ritmo_viagem", viagem.RitmoViagem);
                cmd.Parameters.AddWithValue("@clima_viagem", viagem.ClimaViagem);

                cmd.Parameters.AddWithValue("@faixa_orcamento", viagem.FaixaOrcamento);

                cmd.Parameters.AddWithValue("@desejos_especiais", viagem.DesejosEspeciais);

                cmd.ExecuteNonQuery();
            }

            TempData["Sucesso"] = "Solicitação enviada com sucesso!";

            return RedirectToAction("Index", "Home");
        }
    }
}