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

        // =========================
        // EXCLUIR
        // =========================
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
            int destino_id,
            string nome,
            string descricao,
            string tipo_viagem,
            int duracao_dias,
            DateTime data_partida,
            DateTime data_retorno,
            decimal preco_por_pessoa,
            int vagas_disponiveis,
            string? imagem_url,          // campo URL externa do formulário
            IFormFile? imagemArquivo)    // campo upload de arquivo do formulário
        {
            // ── lógica de imagem ──────────────────────────────────────
            // Prioridade: se enviou arquivo → salva o arquivo
            //             se não enviou arquivo → usa a URL digitada
            string? caminhoImagem = null;

            if (imagemArquivo != null && imagemArquivo.Length > 0)
            {
                // 1. Pega a extensão do arquivo (.jpg, .png etc.)
                var ext = Path.GetExtension(imagemArquivo.FileName);

                // 2. Gera um nome único para não sobrescrever outros arquivos
                var fileName = $"{Guid.NewGuid()}{ext}";

                // 3. Define a pasta onde vai salvar (wwwroot/imgs/)
                var saveDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "imgs");

                // 4. Cria a pasta se não existir
                Directory.CreateDirectory(saveDir);

                // 5. Salva o arquivo no disco
                var absPath = Path.Combine(saveDir, fileName);
                using (var fs = new FileStream(absPath, FileMode.Create))
                {
                    imagemArquivo.CopyTo(fs);
                }

                // 6. Guarda o caminho relativo para salvar no banco
                caminhoImagem = "imgs/" + fileName;
            }
            else if (!string.IsNullOrWhiteSpace(imagem_url))
            {
                // Nenhum arquivo enviado → usa a URL que o admin digitou
                caminhoImagem = imagem_url;
            }
            // ─────────────────────────────────────────────────────────

            using (MySqlConnection conn = new MySqlConnection(conexao))
            {
                conn.Open();

                string sql = @"
                    INSERT INTO pacotes
                    (
                        destino_id, nome, descricao, tipo_viagem,
                        duracao_dias, data_partida, data_retorno,
                        preco_por_pessoa, vagas_disponiveis, imagem_url
                    )
                    VALUES
                    (
                        @destino_id, @nome, @descricao, @tipo_viagem,
                        @duracao_dias, @data_partida, @data_retorno,
                        @preco_por_pessoa, @vagas_disponiveis, @imagem_url
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
                string sql = "SELECT * FROM pacotes WHERE id = @id";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            ViewBag.Id = reader["id"];
                            ViewBag.DestinoId = reader["destino_id"];
                            ViewBag.Nome = reader["nome"];
                            ViewBag.Descricao = reader["descricao"];
                            ViewBag.TipoViagem = reader["tipo_viagem"];
                            ViewBag.DuracaoDias = reader["duracao_dias"];
                            ViewBag.DataPartida =
                                Convert.ToDateTime(reader["data_partida"]).ToString("yyyy-MM-dd");
                            ViewBag.DataRetorno =
                                Convert.ToDateTime(reader["data_retorno"]).ToString("yyyy-MM-dd");
                            ViewBag.Preco = reader["preco_por_pessoa"];
                            ViewBag.Vagas = reader["vagas_disponiveis"];
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
            int destino_id,
            string nome,
            string descricao,
            string tipo_viagem,
            int duracao_dias,
            DateTime data_partida,
            DateTime data_retorno,
            decimal preco_por_pessoa,
            int vagas_disponiveis,
            string? imagem_url,          // imagem atual (campo hidden da view)
            IFormFile? imagemArquivo)    // novo upload (opcional)
        {
            // ── lógica de imagem ──────────────────────────────────────
            // Começa com a imagem que já estava salva
            string? caminhoImagem = imagem_url;

            if (imagemArquivo != null && imagemArquivo.Length > 0)
            {
                // Admin enviou um arquivo novo → substitui
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
            // Se não enviou arquivo novo, mantém o caminhoImagem = imagem_url (o que já tinha)
            // ─────────────────────────────────────────────────────────

            using (MySqlConnection conn = new MySqlConnection(conexao))
            {
                conn.Open();

                string sql = @"
                    UPDATE pacotes SET
                        destino_id        = @destino_id,
                        nome              = @nome,
                        descricao         = @descricao,
                        tipo_viagem       = @tipo_viagem,
                        duracao_dias      = @duracao_dias,
                        data_partida      = @data_partida,
                        data_retorno      = @data_retorno,
                        preco_por_pessoa  = @preco_por_pessoa,
                        vagas_disponiveis = @vagas_disponiveis,
                        imagem_url        = @imagem_url
                    WHERE id = @id";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@destino_id", destino_id);
                    cmd.Parameters.AddWithValue("@nome", nome);
                    cmd.Parameters.AddWithValue("@descricao", descricao);
                    cmd.Parameters.AddWithValue("@tipo_viagem", tipo_viagem);
                    cmd.Parameters.AddWithValue("@duracao_dias", duracao_dias);
                    cmd.Parameters.AddWithValue("@data_partida", data_partida);
                    cmd.Parameters.AddWithValue("@data_retorno", data_retorno);
                    cmd.Parameters.AddWithValue("@preco_por_pessoa", preco_por_pessoa);
                    cmd.Parameters.AddWithValue("@vagas_disponiveis", vagas_disponiveis);
                    cmd.Parameters.AddWithValue("@imagem_url", (object?)caminhoImagem ?? DBNull.Value);

                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index");
        }
    }
}