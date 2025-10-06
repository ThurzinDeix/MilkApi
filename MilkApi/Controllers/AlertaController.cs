using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace MilkApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AlertaController : ControllerBase
    {
        private readonly string ConnectionString = config.ConnectionString;
        private readonly AlertaService _service;

        public AlertaController()
        {
            _service = new AlertaService(ConnectionString);
        }

        [HttpGet("{usuarioId}")]
        public ActionResult<List<Alerta>> GetAlertas(int usuarioId)
        {
            try
            {
                var alertas = _service.GerarAlertas(usuarioId);
                return Ok(alertas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao gerar alertas: {ex.Message}");
            }
        }
    }

    public class Alerta
    {
        public string Tipo { get; set; }     // danger, warning, info
        public string Mensagem { get; set; }
        public string Origem { get; set; }
        public int? ID_Gado { get; set; }
    }

    public class AlertaService
    {
        private readonly string _connectionString;

        public AlertaService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Alerta> GerarAlertas(int usuarioId)
        {
            var alertas = new List<Alerta>();

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // -----------------------------
                // 1) Produção de Leite
                // -----------------------------
                double mediaEsperada = 12.0;
                string sqlLeite = @"SELECT ID_Gado, Litros FROM Leite WHERE ID_Usuario = @id";
                using (var cmd = new SqlCommand(sqlLeite, conn))
                {
                    cmd.Parameters.AddWithValue("@id", usuarioId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int idGado = reader.GetInt32(0);
                            double litros = Convert.ToDouble(reader.GetDecimal(1));

                            if (litros < mediaEsperada * 0.9)
                                alertas.Add(new Alerta { Tipo = "danger", Mensagem = $"Vaca {idGado} produzindo abaixo da média esperada", Origem = "Leite", ID_Gado = idGado });
                            else if (litros < mediaEsperada)
                                alertas.Add(new Alerta { Tipo = "warning", Mensagem = $"Vaca {idGado} próxima do limite mínimo de produção", Origem = "Leite", ID_Gado = idGado });
                            else if (litros > mediaEsperada * 1.1)
                                alertas.Add(new Alerta { Tipo = "info", Mensagem = $"Vaca {idGado} produzindo acima da média esperada", Origem = "Leite", ID_Gado = idGado });
                        }
                    }
                }

                // -----------------------------
                // 2) Qualidade do Leite por Lote
                // -----------------------------
                string sqlQualidade = @"SELECT ID_Lote, CCS, Gordura, Proteina FROM Qualidade WHERE ID_Usuario = @id";
                using (var cmd = new SqlCommand(sqlQualidade, conn))
                {
                    cmd.Parameters.AddWithValue("@id", usuarioId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        var lotes = new List<(int idLote, decimal ccs, decimal gordura, decimal proteina)>();
                        while (reader.Read())
                        {
                            int idLote = reader.GetInt32(0);
                            decimal ccs = reader.GetDecimal(1);
                            decimal gordura = reader.GetDecimal(2);
                            decimal proteina = reader.GetDecimal(3);
                            lotes.Add((idLote, ccs, gordura, proteina));
                        }
                        reader.Close();

                        foreach (var lote in lotes)
                        {
                            // Buscar todos os gados que produziram leite neste lote
                            string sqlGados = @"SELECT ID_Gado FROM Leite WHERE Lote = @loteNum AND ID_Usuario = @id";
                            using (var cmdGado = new SqlCommand(sqlGados, conn))
                            {
                                cmdGado.Parameters.AddWithValue("@loteNum", lote.idLote.ToString());
                                cmdGado.Parameters.AddWithValue("@id", usuarioId);
                                using (var readerGado = cmdGado.ExecuteReader())
                                {
                                    var gados = new List<int>();
                                    while (readerGado.Read())
                                    {
                                        gados.Add(readerGado.GetInt32(0));
                                    }

                                    foreach (var idGado in gados)
                                    {
                                        // CCS
                                        if (lote.ccs < 100000)
                                            alertas.Add(new Alerta { Tipo = "info", Mensagem = $"Vaca {idGado} no lote {lote.idLote} com CCS excelente (<100k)", Origem = "Qualidade", ID_Gado = idGado });
                                        else if (lote.ccs > 300000)
                                            alertas.Add(new Alerta { Tipo = "danger", Mensagem = $"Vaca {idGado} no lote {lote.idLote} com CCS alto (>300k)", Origem = "Qualidade", ID_Gado = idGado });
                                        else if (lote.ccs > 200000)
                                            alertas.Add(new Alerta { Tipo = "warning", Mensagem = $"Vaca {idGado} no lote {lote.idLote} com CCS elevado (200k–300k)", Origem = "Qualidade", ID_Gado = idGado });

                                        // Gordura
                                        if (lote.gordura < 2.5m)
                                            alertas.Add(new Alerta { Tipo = "warning", Mensagem = $"Vaca {idGado} no lote {lote.idLote} com gordura baixa (<2.5%)", Origem = "Qualidade", ID_Gado = idGado });
                                        else if (lote.gordura > 4.0m)
                                            alertas.Add(new Alerta { Tipo = "warning", Mensagem = $"Vaca {idGado} no lote {lote.idLote} com gordura alta (>4.0%)", Origem = "Qualidade", ID_Gado = idGado });

                                        // Proteína
                                        if (lote.proteina < 3.2m)
                                            alertas.Add(new Alerta { Tipo = "warning", Mensagem = $"Vaca {idGado} no lote {lote.idLote} com proteína baixa (<3.2%)", Origem = "Qualidade", ID_Gado = idGado });
                                        else if (lote.proteina > 4.0m)
                                            alertas.Add(new Alerta { Tipo = "warning", Mensagem = $"Vaca {idGado} no lote {lote.idLote} com proteína alta (>4.0%)", Origem = "Qualidade", ID_Gado = idGado });
                                    }
                                }
                            }
                        }
                    }
                }

                // -----------------------------
                // 3) Remédios
                // -----------------------------
                string sqlRemedio = @"
                    SELECT r.Id, r.ID_Gado, r.Nome, r.Date, r.intervalo, r.Doses,
                           COUNT(da.Id) AS DosesAplicadas
                    FROM Remedio r
                    LEFT JOIN DosesAplicadas da ON r.Id = da.ID_Remedio
                    WHERE r.ID_Usuario = @id
                    GROUP BY r.Id, r.ID_Gado, r.Nome, r.Date, r.intervalo, r.Doses";

                using (var cmd = new SqlCommand(sqlRemedio, conn))
                {
                    cmd.Parameters.AddWithValue("@id", usuarioId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int idRemedio = reader.GetInt32(0);
                            int idGado = reader.GetInt32(1);
                            string nome = reader.GetString(2);
                            DateTime inicio = reader.GetDateTime(3);
                            int intervalo = reader.GetInt32(4); // em horas
                            int totalDoses = reader.GetInt32(5);
                            int dosesAplicadas = reader.GetInt32(6);

                            int dosesRestantes = totalDoses - dosesAplicadas;

                            DateTime? proximaDose = dosesAplicadas < totalDoses
                                ? inicio.AddHours(intervalo * dosesAplicadas)
                                : (DateTime?)null;

                            if (dosesRestantes > 0 && proximaDose.HasValue)
                            {
                                if (proximaDose.Value < DateTime.Now)
                                {
                                    alertas.Add(new Alerta
                                    {
                                        Tipo = "danger",
                                        Mensagem = $"Vaca {idGado} com medicamento {nome} em atraso ({dosesRestantes} doses restantes, próxima deveria ser {proximaDose:dd/MM HH:mm})",
                                        Origem = "Remedio",
                                        ID_Gado = idGado
                                    });
                                }
                                else if (proximaDose.Value <= DateTime.Now.AddHours(6))
                                {
                                    alertas.Add(new Alerta
                                    {
                                        Tipo = "warning",
                                        Mensagem = $"Vaca {idGado} deve tomar {nome} em breve ({dosesRestantes} doses restantes, próxima às {proximaDose:HH:mm dd/MM})",
                                        Origem = "Remedio",
                                        ID_Gado = idGado
                                    });
                                }
                            }
                        }
                    }
                }


                // -----------------------------
                // 4) Suplementos
                // -----------------------------
                string sqlSuplemento = @"SELECT ID_Gado, Nome, Date, intervalo FROM Suplemento WHERE ID_Usuario = @id";
                using (var cmd = new SqlCommand(sqlSuplemento, conn))
                {
                    cmd.Parameters.AddWithValue("@id", usuarioId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int idGado = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                            string nome = reader.GetString(1);
                            DateTime data = reader.GetDateTime(2);
                            int intervalo = reader.GetInt32(3);
                            DateTime proxima = data.AddDays(intervalo);

                            if (proxima < DateTime.Now)
                                alertas.Add(new Alerta { Tipo = "danger", Mensagem = $"Vaca {idGado} com suplemento {nome} em atraso", Origem = "Suplemento", ID_Gado = idGado });
                            else if (proxima <= DateTime.Now.AddDays(3))
                                alertas.Add(new Alerta { Tipo = "warning", Mensagem = $"Vaca {idGado} deve receber suplemento {nome} em breve", Origem = "Suplemento", ID_Gado = idGado });
                        }
                    }
                }

                // -----------------------------
                // 5) Prenhez
                // -----------------------------
                string sqlPrenhez = @"SELECT ID_Gado, Data_Prenhez, Data_Esperada, Status FROM Prenhez WHERE ID_Usuario = @id";
                using (var cmd = new SqlCommand(sqlPrenhez, conn))
                {
                    cmd.Parameters.AddWithValue("@id", usuarioId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int idGado = reader.GetInt32(0);
                            DateTime dataPrenhez = reader.GetDateTime(1);
                            DateTime? dataEsperada = reader.IsDBNull(2) ? null : reader.GetDateTime(2);
                            string status = reader.IsDBNull(3) ? "Aguardando Diagnóstico" : reader.GetString(3);

                            if (status == "Aguardando Diagnóstico" && dataEsperada.HasValue)
                            {
                                if (dataEsperada.Value < DateTime.Now)
                                    alertas.Add(new Alerta { Tipo = "danger", Mensagem = $"Vaca {idGado} com diagnóstico de prenhez atrasado", Origem = "Prenhez", ID_Gado = idGado });
                                else if (dataEsperada.Value <= DateTime.Now.AddDays(5))
                                    alertas.Add(new Alerta { Tipo = "warning", Mensagem = $"Vaca {idGado} precisa de diagnóstico de prenhez em breve", Origem = "Prenhez", ID_Gado = idGado });
                            }

                            DateTime dataParto = dataPrenhez.AddDays(280);
                            DateTime dataSecagem = dataParto.AddDays(-60);

                            if (dataSecagem <= DateTime.Now.AddDays(7) && status != "Seca")
                            {
                                if (dataSecagem < DateTime.Now)
                                    alertas.Add(new Alerta { Tipo = "danger", Mensagem = $"Vaca {idGado} já deveria estar seca", Origem = "Prenhez", ID_Gado = idGado });
                                else
                                    alertas.Add(new Alerta { Tipo = "warning", Mensagem = $"Vaca {idGado} deve ser seca na próxima semana", Origem = "Prenhez", ID_Gado = idGado });
                            }

                            if (dataParto <= DateTime.Now.AddDays(7) && status != "Parida")
                            {
                                if (dataParto < DateTime.Now)
                                    alertas.Add(new Alerta { Tipo = "danger", Mensagem = $"Vaca {idGado} já deveria ter parido", Origem = "Prenhez", ID_Gado = idGado });
                                else
                                    alertas.Add(new Alerta { Tipo = "warning", Mensagem = $"Vaca {idGado} prestes a parir", Origem = "Prenhez", ID_Gado = idGado });
                            }
                        }
                    }
                }
            }

            return alertas;
        }
    }
}
