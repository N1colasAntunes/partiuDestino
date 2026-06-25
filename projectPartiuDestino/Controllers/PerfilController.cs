using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using projectPartiuDestino.Models;

namespace projectPartiuDestino.Controllers
{
    public class PerfilController : Controller
    {
        private string connStr = "server=localhost;database=bdpartiudestino;uid=root;pwd=12345678;";

        // -------------------------------------------------------
        // INDEX — exibe perfil + pedidos
        // -------------------------------------------------------
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Index", "Login");

            using var conn = new MySqlConnection(connStr);
            conn.Open();

            using (var cmdU = new MySqlCommand(
                "SELECT nome, email, telefone, documento, data_nascimento, foto_perfil, tipo FROM usuarios WHERE id = @id",
                conn))
            {
                cmdU.Parameters.AddWithValue("@id", userId);
                using var readerU = cmdU.ExecuteReader();
                if (readerU.Read())
                {
                    ViewBag.Nome = readerU["nome"].ToString();
                    ViewBag.Email = readerU["email"].ToString();
                    ViewBag.Telefone = readerU["telefone"]?.ToString();
                    ViewBag.Documento = readerU["documento"]?.ToString();
                    ViewBag.DataNascimento = readerU["data_nascimento"] == DBNull.Value
                                                ? (DateTime?)null
                                                : Convert.ToDateTime(readerU["data_nascimento"]);
                    ViewBag.FotoPerfil = readerU["foto_perfil"]?.ToString();
                    ViewBag.Tipo = readerU["tipo"].ToString();
                }
            }

            var pedidos = new List<PedidoConfirmado>();
            using var cmdP = new MySqlCommand(
                @"SELECT 
      id,
      codigo_reserva,
      nome_item,
      data_pedido,
      preco_unitario,
      quantidade,
      tipo_item,
      forma_pagamento,
      status_pagamento,
      valor_total_pedido,
      parcelas,
      comprovante,
      data_pagamento
  FROM pedidos
  WHERE usuario_id = @id
  ORDER BY id DESC",
                conn);
            cmdP.Parameters.AddWithValue("@id", userId);
            using var readerP = cmdP.ExecuteReader();
            while (readerP.Read())
            {
                pedidos.Add(new PedidoConfirmado
                {
                    Id = Convert.ToInt32(readerP["id"]),
                    CodigoReserva = readerP["codigo_reserva"]?.ToString(),
                    NomeItem = readerP["nome_item"].ToString() ?? "",
                    DataPedido = Convert.ToDateTime(readerP["data_pedido"]),
                    PrecoUnitario = Convert.ToDecimal(readerP["preco_unitario"]),
                    Quantidade = Convert.ToInt32(readerP["quantidade"]),
                    TipoItem = readerP["tipo_item"].ToString() ?? "",
                    FormaPagamento = readerP["forma_pagamento"]?.ToString(),
                    StatusPagamento = readerP["status_pagamento"]?.ToString(),
                    ValorTotalPedido = readerP["valor_total_pedido"] == DBNull.Value ? 0 : Convert.ToDecimal(readerP["valor_total_pedido"]),
                    Parcelas = readerP["parcelas"] == DBNull.Value ? 1 : Convert.ToInt32(readerP["parcelas"]),
                    Comprovante = readerP["comprovante"]?.ToString(),
                    DataPagamento = readerP["data_pagamento"] == DBNull.Value ? null : Convert.ToDateTime(readerP["data_pagamento"])
                });
            }

            if (TempData["Sucesso"] != null)
                ViewBag.Sucesso = TempData["Sucesso"].ToString();

            if (TempData["Erro"] != null)
                ViewBag.Erro = TempData["Erro"].ToString();

            return View(pedidos);
        }

        // -------------------------------------------------------
        // EDITAR — GET: carrega dados + tipo para a view decidir quais campos mostrar
        // -------------------------------------------------------
        public IActionResult Editar()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Index", "Login");

            using var conn = new MySqlConnection(connStr);
            conn.Open();

            using var cmd = new MySqlCommand(
                "SELECT nome, email, telefone, documento, data_nascimento, foto_perfil, tipo FROM usuarios WHERE id = @id",
                conn);
            cmd.Parameters.AddWithValue("@id", userId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                ViewBag.Nome = reader["nome"].ToString();
                ViewBag.Email = reader["email"].ToString();
                ViewBag.Telefone = reader["telefone"]?.ToString();
                ViewBag.Documento = reader["documento"]?.ToString();
                ViewBag.DataNascimento = reader["data_nascimento"] == DBNull.Value
                                            ? ""
                                            : Convert.ToDateTime(reader["data_nascimento"]).ToString("yyyy-MM-dd");
                ViewBag.FotoPerfil = reader["foto_perfil"]?.ToString();
                ViewBag.Tipo = reader["tipo"].ToString(); // ← passado para a view
            }

            return View();
        }

        // -------------------------------------------------------
        // EDITAR — POST: lógica separada por tipo de usuário
        // -------------------------------------------------------
        [HttpPost]
        public IActionResult Editar(string nome, string novaSenha, string confirmarSenha,
                                    string telefone, string documento, string dataNascimento)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Index", "Login");

            // lê o tipo diretamente do banco (não da sessão, por segurança)
            string tipoUsuario = "";
            using var conn = new MySqlConnection(connStr);
            conn.Open();

            using (var cmdTipo = new MySqlCommand("SELECT tipo FROM usuarios WHERE id = @id", conn))
            {
                cmdTipo.Parameters.AddWithValue("@id", userId);
                tipoUsuario = cmdTipo.ExecuteScalar()?.ToString() ?? "usuario";
            }

            bool isAdmin = tipoUsuario == "admin";

            // ── validação comum ──
            if (string.IsNullOrWhiteSpace(nome))
            {
                ViewBag.Erro = "O nome não pode ficar em branco.";
                ViewBag.Tipo = tipoUsuario;
                ViewBag.Nome = nome;
                return View();
            }

            // ── validação de senha (só admin altera senha) ──
            string? novaHashSenha = null;
            if (isAdmin && !string.IsNullOrWhiteSpace(novaSenha))
            {
                if (novaSenha.Length < 6)
                {
                    ViewBag.Erro = "A senha deve ter pelo menos 6 caracteres.";
                    ViewBag.Tipo = tipoUsuario;
                    ViewBag.Nome = nome;
                    return View();
                }
                if (novaSenha != confirmarSenha)
                {
                    ViewBag.Erro = "As senhas não coincidem.";
                    ViewBag.Tipo = tipoUsuario;
                    ViewBag.Nome = nome;
                    return View();
                }
                novaHashSenha = BCrypt.Net.BCrypt.HashPassword(novaSenha);
            }

            // ── validação de data (só usuário comum) ──
            DateTime? dataNasc = null;
            if (!isAdmin && !string.IsNullOrWhiteSpace(dataNascimento))
            {
                if (DateTime.TryParse(dataNascimento, out var dt))
                    dataNasc = dt;
                else
                {
                    ViewBag.Erro = "Data de nascimento inválida.";
                    ViewBag.Tipo = tipoUsuario;
                    ViewBag.Nome = nome;
                    return View();
                }
            }

            // ── monta UPDATE conforme o tipo ──
            if (isAdmin)
            {
                // admin: atualiza nome e, opcionalmente, a senha
                string sql = novaHashSenha != null
                    ? "UPDATE usuarios SET nome = @nome, senha = @senha WHERE id = @id"
                    : "UPDATE usuarios SET nome = @nome WHERE id = @id";

                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@nome", nome.Trim());
                if (novaHashSenha != null)
                    cmd.Parameters.AddWithValue("@senha", novaHashSenha);
                cmd.Parameters.AddWithValue("@id", userId);
                cmd.ExecuteNonQuery();
            }
            else
            {
                // usuário comum: atualiza todos os campos pessoais (sem senha)
                using var cmd = new MySqlCommand(
                    @"UPDATE usuarios
                      SET nome            = @nome,
                          telefone        = @telefone,
                          documento       = @documento,
                          data_nascimento = @dataNasc
                      WHERE id = @id",
                    conn);
                cmd.Parameters.AddWithValue("@nome", nome.Trim());
                cmd.Parameters.AddWithValue("@telefone", string.IsNullOrWhiteSpace(telefone) ? DBNull.Value : (object)telefone.Trim());
                cmd.Parameters.AddWithValue("@documento", string.IsNullOrWhiteSpace(documento) ? DBNull.Value : (object)documento.Trim());
                cmd.Parameters.AddWithValue("@dataNasc", dataNasc.HasValue ? (object)dataNasc.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@id", userId);
                cmd.ExecuteNonQuery();
            }

            HttpContext.Session.SetString("UserName", nome.Trim());
            TempData["Sucesso"] = "Perfil atualizado com sucesso!";
            return RedirectToAction("Index");
        }

        // -------------------------------------------------------
        // ALTERAR FOTO — POST
        // -------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> AlterarFoto(IFormFile foto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Index", "Login");

            if (foto == null || foto.Length == 0)
            {
                TempData["Sucesso"] = "Nenhuma foto selecionada.";
                return RedirectToAction("Index");
            }

            var extensoesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(foto.FileName).ToLower();
            if (!extensoesPermitidas.Contains(ext))
            {
                TempData["Erro"] = "Formato inválido. Use JPG, PNG ou WEBP.";
                return RedirectToAction("Index");
            }

            if (foto.Length > 2 * 1024 * 1024)
            {
                TempData["Erro"] = "A foto deve ter no máximo 2 MB.";
                return RedirectToAction("Index");
            }

            var pastaUpload = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "perfis");
            Directory.CreateDirectory(pastaUpload);

            var nomeArquivo = $"user_{userId}_{DateTime.Now:yyyyMMddHHmmss}{ext}";
            var caminhoFisico = Path.Combine(pastaUpload, nomeArquivo);

            using (var stream = new FileStream(caminhoFisico, FileMode.Create))
                await foto.CopyToAsync(stream);

            var caminhoRelativo = $"/uploads/perfis/{nomeArquivo}";

            using var conn = new MySqlConnection(connStr);
            conn.Open();

            using var cmd = new MySqlCommand(
                "UPDATE usuarios SET foto_perfil = @foto WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@foto", caminhoRelativo);
            cmd.Parameters.AddWithValue("@id", userId);
            cmd.ExecuteNonQuery();

            TempData["Sucesso"] = "Foto de perfil atualizada com sucesso!";
            return RedirectToAction("Index");
        }
    }
}