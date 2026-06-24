using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System;

namespace projectPartiuDestino.Controllers
{
    public class DestinosAdminController : Controller
    {
        private string conexao = "server=localhost;database=bdpartiudestino;uid=root;pwd=12345678;";

        public IActionResult Index()
        {
            return View();
        }

        // =========================
        // EXCLUIR
        // =========================
        [HttpPost]
        public IActionResult Delete(int id)
        {
            using (MySqlConnection conn = new MySqlConnection(conexao))
            {
                conn.Open();
                string sql = "DELETE FROM destinos WHERE id = @id";
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Index");
        }

        // =========================
        // CRIAR — GET
        // =========================
        public IActionResult Criar()
        {
            return View();
        }

        // =========================
        // CRIAR — POST
        // =========================
        [HttpPost]
        public IActionResult Criar(
            string origem_pais,
            string origem_estado,
            string pais,
            string estado,
            decimal preco_por_pessoa,
            string? imagem_url,        // URL externa digitada pelo admin
            IFormFile? imagemArquivo)  // arquivo enviado pelo admin
        {
            // ── lógica de imagem ──────────────────────────────────────
            string? caminhoImagem = null;

            if (imagemArquivo != null && imagemArquivo.Length > 0)
            {
                var ext = Path.GetExtension(imagemArquivo.FileName);
                var fileName = $"{Guid.NewGuid()}{ext}";
                var saveDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "imgs");
                Directory.CreateDirectory(saveDir);
                var absPath = Path.Combine(saveDir, fileName);
                using (var fs = new FileStream(absPath, FileMode.Create))
                {
                    imagemArquivo.CopyTo(fs);
                }
                caminhoImagem = "imgs/" + fileName;
            }
            else if (!string.IsNullOrWhiteSpace(imagem_url))
            {
                caminhoImagem = imagem_url;
            }
            // ─────────────────────────────────────────────────────────

            using (MySqlConnection conn = new MySqlConnection(conexao))
            {
                conn.Open();

                string sql = @"
                    INSERT INTO destinos
                    (origem_pais, origem_estado, pais, estado, preco_por_pessoa, imagem_url)
                    VALUES
                    (@origem_pais, @origem_estado, @pais, @estado, @preco_por_pessoa, @imagem_url)";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@origem_pais", origem_pais);
                    cmd.Parameters.AddWithValue("@origem_estado", origem_estado);
                    cmd.Parameters.AddWithValue("@pais", pais);
                    cmd.Parameters.AddWithValue("@estado", estado);
                    cmd.Parameters.AddWithValue("@preco_por_pessoa", preco_por_pessoa);
                    cmd.Parameters.AddWithValue("@imagem_url", (object?)caminhoImagem ?? DBNull.Value);

                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index");
        }

        // =========================
        // EDITAR — GET
        // =========================
        public IActionResult Editar(int id)
        {
            using (MySqlConnection conn = new MySqlConnection(conexao))
            {
                conn.Open();
                string sql = "SELECT * FROM destinos WHERE id = @id";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            ViewBag.Id = id;
                            ViewBag.OrigemPais = reader["origem_pais"].ToString();
                            ViewBag.OrigemEstado = reader["origem_estado"].ToString();
                            ViewBag.Pais = reader["pais"].ToString();
                            ViewBag.Estado = reader["estado"].ToString();
                            ViewBag.Preco = reader["preco_por_pessoa"];
                            // ← carrega a imagem atual para mostrar na tela
                            ViewBag.ImagemUrl = reader["imagem_url"]?.ToString();
                        }
                    }
                }
            }

            return View();
        }

        // =========================
        // EDITAR — POST
        // =========================
        [HttpPost]
        public IActionResult Editar(
            int id,
            string origem_pais,
            string origem_estado,
            string pais,
            string estado,
            decimal preco_por_pessoa,
            string? imagem_url,        // imagem atual (campo hidden da view)
            IFormFile? imagemArquivo)  // novo upload (opcional)
        {
            // ── lógica de imagem ──────────────────────────────────────
            string? caminhoImagem = imagem_url; // começa com a que já estava

            if (imagemArquivo != null && imagemArquivo.Length > 0)
            {
                var ext = Path.GetExtension(imagemArquivo.FileName);
                var fileName = $"{Guid.NewGuid()}{ext}";
                var saveDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "imgs");
                Directory.CreateDirectory(saveDir);
                var absPath = Path.Combine(saveDir, fileName);
                using (var fs = new FileStream(absPath, FileMode.Create))
                {
                    imagemArquivo.CopyTo(fs);
                }
                caminhoImagem = "imgs/" + fileName;
            }
            // ─────────────────────────────────────────────────────────

            using (MySqlConnection conn = new MySqlConnection(conexao))
            {
                conn.Open();

                string sql = @"
                    UPDATE destinos SET
                        origem_pais      = @origem_pais,
                        origem_estado    = @origem_estado,
                        pais             = @pais,
                        estado           = @estado,
                        preco_por_pessoa = @preco_por_pessoa,
                        imagem_url       = @imagem_url
                    WHERE id = @id";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@origem_pais", origem_pais);
                    cmd.Parameters.AddWithValue("@origem_estado", origem_estado);
                    cmd.Parameters.AddWithValue("@pais", pais);
                    cmd.Parameters.AddWithValue("@estado", estado);
                    cmd.Parameters.AddWithValue("@preco_por_pessoa", preco_por_pessoa);
                    cmd.Parameters.AddWithValue("@imagem_url", (object?)caminhoImagem ?? DBNull.Value);

                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index");
        }
    }
}