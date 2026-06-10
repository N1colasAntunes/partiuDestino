using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using projectPartiuDestino.Models;
using System;
using System.Collections.Generic;

namespace projectPartiuDestino.Controllers
{
    public class DestinosController : Controller
    {
        private string conexao = "server=localhost;database=bdpartiudestino;uid=root;pwd=12345678;";

        public IActionResult Index()
        {
            List<Destinos> listaDestinos = new List<Destinos>();

            using (MySqlConnection conn = new MySqlConnection(conexao))
            {
                conn.Open();

                string sql = "SELECT id, origem_pais, origem_estado, pais, estado, imagem_url FROM destinos";

                MySqlCommand cmd = new MySqlCommand(sql, conn);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        listaDestinos.Add(new Destinos
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Pais = reader["pais"].ToString(),
                            Estado = reader["estado"].ToString(),
                            OrigemPais = reader["origem_pais"].ToString(),
                            OrigemEstado = reader["origem_estado"].ToString(),
                            ImagemUrl = reader["imagem_url"].ToString()
                        });
                    }
                }
            }

            return View(listaDestinos);
        }

        public IActionResult Detalhes()
        {
            return View();
        }
    }
}