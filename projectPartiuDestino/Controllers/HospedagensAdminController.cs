using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace projectPartiuDestino.Controllers
{
    public class HospedagensAdminController : Controller
    {
        private string conexao = "server=localhost;database=bdpartiudestino;uid=root;pwd=12345678;";

        // GET: /HospedagensAdmin/Index?pacoteId=1
        public IActionResult Index(int pacoteId)
        {
            var lista = new List<Models.Hospedagem>();
            string nomePacote = "";

            using MySqlConnection conn = new MySqlConnection(conexao);
            conn.Open();

            // Nome do pacote, para o breadcrumb
            using (var cmdNome = new MySqlCommand("SELECT nome FROM pacotes WHERE id = @id", conn))
            {
                cmdNome.Parameters.AddWithValue("@id", pacoteId);
                var resultado = cmdNome.ExecuteScalar();
                nomePacote = resultado?.ToString() ?? "Pacote não encontrado";
            }

            string sql = "SELECT * FROM hospedagens WHERE pacote_id = @pacoteId ORDER BY id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@pacoteId", pacoteId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new Models.Hospedagem
                {
                    Id = Convert.ToInt32(reader["id"]),
                    PacoteId = Convert.ToInt32(reader["pacote_id"]),
                    Nome = reader["nome"].ToString()!,
                    Categoria = reader["categoria"]?.ToString(),
                    Descricao = reader["descricao"]?.ToString(),
                    Endereco = reader["endereco"]?.ToString(),
                    ImagemUrl = reader["imagem_url"]?.ToString()
                });
            }

            ViewBag.PacoteId = pacoteId;
            ViewBag.NomePacote = nomePacote;
            return View(lista);
        }

        public IActionResult Criar(int pacoteId)
        {
            ViewBag.PacoteId = pacoteId;
            return View();
        }

        [HttpPost]
        public IActionResult Criar(int pacoteId, string nome, string? categoria, string? descricao, string? endereco, string? imagemUrl)
        {
            using MySqlConnection conn = new MySqlConnection(conexao);
            conn.Open();

            string sql = @"INSERT INTO hospedagens (pacote_id, nome, categoria, descricao, endereco, imagem_url)
                            VALUES (@pacoteId, @nome, @categoria, @descricao, @endereco, @imagemUrl)";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@pacoteId", pacoteId);
            cmd.Parameters.AddWithValue("@nome", nome);
            cmd.Parameters.AddWithValue("@categoria", categoria ?? "");
            cmd.Parameters.AddWithValue("@descricao", descricao ?? "");
            cmd.Parameters.AddWithValue("@endereco", endereco ?? "");
            cmd.Parameters.AddWithValue("@imagemUrl", imagemUrl ?? "");
            cmd.ExecuteNonQuery();

            return RedirectToAction("Index", new { pacoteId });
        }

        public IActionResult Editar(int id)
        {
            using MySqlConnection conn = new MySqlConnection(conexao);
            conn.Open();

            string sql = "SELECT * FROM hospedagens WHERE id = @id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                ViewBag.Id = reader["id"];
                ViewBag.PacoteId = reader["pacote_id"];
                ViewBag.Nome = reader["nome"];
                ViewBag.Categoria = reader["categoria"];
                ViewBag.Descricao = reader["descricao"];
                ViewBag.Endereco = reader["endereco"];
                ViewBag.ImagemUrl = reader["imagem_url"];
            }

            return View();
        }

        [HttpPost]
        public IActionResult Editar(int id, int pacoteId, string nome, string? categoria, string? descricao, string? endereco, string? imagemUrl)
        {
            using MySqlConnection conn = new MySqlConnection(conexao);
            conn.Open();

            string sql = @"UPDATE hospedagens
                            SET nome = @nome, categoria = @categoria, descricao = @descricao,
                                endereco = @endereco, imagem_url = @imagemUrl
                            WHERE id = @id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@nome", nome);
            cmd.Parameters.AddWithValue("@categoria", categoria ?? "");
            cmd.Parameters.AddWithValue("@descricao", descricao ?? "");
            cmd.Parameters.AddWithValue("@endereco", endereco ?? "");
            cmd.Parameters.AddWithValue("@imagemUrl", imagemUrl ?? "");
            cmd.ExecuteNonQuery();

            return RedirectToAction("Index", new { pacoteId });
        }

        [HttpPost]
        public IActionResult Delete(int id, int pacoteId)
        {
            using MySqlConnection conn = new MySqlConnection(conexao);
            conn.Open();

            // ON DELETE CASCADE no banco já remove os quartos vinculados
            string sql = "DELETE FROM hospedagens WHERE id = @id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();

            return RedirectToAction("Index", new { pacoteId });
        }
    }
}