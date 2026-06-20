using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace projectPartiuDestino.Controllers
{
    public class QuartosAdminController : Controller
    {
        private string conexao = "server=localhost;database=bdpartiudestino;uid=root;pwd=12345678;";

        // GET: /QuartosAdmin/Index?hospedagemId=1
        public IActionResult Index(int hospedagemId)
        {
            var lista = new List<Models.Quarto>();
            string nomeHospedagem = "";
            int pacoteId = 0;

            using MySqlConnection conn = new MySqlConnection(conexao);
            conn.Open();

            using (var cmdNome = new MySqlCommand("SELECT nome, pacote_id FROM hospedagens WHERE id = @id", conn))
            {
                cmdNome.Parameters.AddWithValue("@id", hospedagemId);
                using var r = cmdNome.ExecuteReader();
                if (r.Read())
                {
                    nomeHospedagem = r["nome"].ToString()!;
                    pacoteId = Convert.ToInt32(r["pacote_id"]);
                }
            }

            string sql = "SELECT * FROM quartos WHERE hospedagem_id = @hospedagemId ORDER BY id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@hospedagemId", hospedagemId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new Models.Quarto
                {
                    Id = Convert.ToInt32(reader["id"]),
                    HospedagemId = Convert.ToInt32(reader["hospedagem_id"]),
                    TipoQuarto = reader["tipo_quarto"].ToString()!,
                    CapacidadeAdultos = Convert.ToInt32(reader["capacidade_adultos"]),
                    CapacidadeCriancas = Convert.ToInt32(reader["capacidade_criancas"]),
                    PrecoAdicional = Convert.ToDecimal(reader["preco_adicional"]),
                    QuantidadeDisponivel = Convert.ToInt32(reader["quantidade_disponivel"]),
                    Comodidades = reader["comodidades"]?.ToString(),
                    ImagemUrl = reader["imagem_url"]?.ToString()
                });
            }

            ViewBag.HospedagemId = hospedagemId;
            ViewBag.NomeHospedagem = nomeHospedagem;
            ViewBag.PacoteId = pacoteId;
            return View(lista);
        }

        public IActionResult Criar(int hospedagemId)
        {
            ViewBag.HospedagemId = hospedagemId;
            return View();
        }

        [HttpPost]
        public IActionResult Criar(int hospedagemId, string tipoQuarto, int capacidadeAdultos,
            int capacidadeCriancas, decimal precoAdicional, int quantidadeDisponivel,
            string? comodidades, string? imagemUrl)
        {
            using MySqlConnection conn = new MySqlConnection(conexao);
            conn.Open();

            string sql = @"INSERT INTO quartos
                (hospedagem_id, tipo_quarto, capacidade_adultos, capacidade_criancas,
                 preco_adicional, quantidade_disponivel, comodidades, imagem_url)
                VALUES
                (@hospedagemId, @tipoQuarto, @capAdultos, @capCriancas,
                 @preco, @qtd, @comodidades, @imagemUrl)";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@hospedagemId", hospedagemId);
            cmd.Parameters.AddWithValue("@tipoQuarto", tipoQuarto);
            cmd.Parameters.AddWithValue("@capAdultos", capacidadeAdultos);
            cmd.Parameters.AddWithValue("@capCriancas", capacidadeCriancas);
            cmd.Parameters.AddWithValue("@preco", precoAdicional);
            cmd.Parameters.AddWithValue("@qtd", quantidadeDisponivel);
            cmd.Parameters.AddWithValue("@comodidades", comodidades ?? "");
            cmd.Parameters.AddWithValue("@imagemUrl", imagemUrl ?? "");
            cmd.ExecuteNonQuery();

            return RedirectToAction("Index", new { hospedagemId });
        }

        public IActionResult Editar(int id)
        {
            using MySqlConnection conn = new MySqlConnection(conexao);
            conn.Open();

            string sql = "SELECT * FROM quartos WHERE id = @id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                ViewBag.Id = reader["id"];
                ViewBag.HospedagemId = reader["hospedagem_id"];
                ViewBag.TipoQuarto = reader["tipo_quarto"];
                ViewBag.CapacidadeAdultos = reader["capacidade_adultos"];
                ViewBag.CapacidadeCriancas = reader["capacidade_criancas"];
                ViewBag.PrecoAdicional = reader["preco_adicional"];
                ViewBag.QuantidadeDisponivel = reader["quantidade_disponivel"];
                ViewBag.Comodidades = reader["comodidades"];
                ViewBag.ImagemUrl = reader["imagem_url"];
            }

            return View();
        }

        [HttpPost]
        public IActionResult Editar(int id, int hospedagemId, string tipoQuarto, int capacidadeAdultos,
            int capacidadeCriancas, decimal precoAdicional, int quantidadeDisponivel,
            string? comodidades, string? imagemUrl)
        {
            using MySqlConnection conn = new MySqlConnection(conexao);
            conn.Open();

            string sql = @"UPDATE quartos SET
                tipo_quarto = @tipoQuarto,
                capacidade_adultos = @capAdultos,
                capacidade_criancas = @capCriancas,
                preco_adicional = @preco,
                quantidade_disponivel = @qtd,
                comodidades = @comodidades,
                imagem_url = @imagemUrl
                WHERE id = @id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@tipoQuarto", tipoQuarto);
            cmd.Parameters.AddWithValue("@capAdultos", capacidadeAdultos);
            cmd.Parameters.AddWithValue("@capCriancas", capacidadeCriancas);
            cmd.Parameters.AddWithValue("@preco", precoAdicional);
            cmd.Parameters.AddWithValue("@qtd", quantidadeDisponivel);
            cmd.Parameters.AddWithValue("@comodidades", comodidades ?? "");
            cmd.Parameters.AddWithValue("@imagemUrl", imagemUrl ?? "");
            cmd.ExecuteNonQuery();

            return RedirectToAction("Index", new { hospedagemId });
        }

        [HttpPost]
        public IActionResult Delete(int id, int hospedagemId)
        {
            using MySqlConnection conn = new MySqlConnection(conexao);
            conn.Open();

            string sql = "DELETE FROM quartos WHERE id = @id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();

            return RedirectToAction("Index", new { hospedagemId });
        }
    }
}