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
        public ActionResult<object> GetAlertas(int usuarioId)
        {
            try
            {
                var resultado = _service.GerarAlertas(usuarioId);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao gerar alertas: {ex.Message}");
            }
        }
    }

    public class Alerta
    {
        public string Tipo { get; set; }     
        public string Mensagem { get; set; }
        public string Origem { get; set; }
        public int? ID_Gado { get; set; }
    }

    public class Estatisticas
    {
        public double ProducaoTotalDiaria { get; set; }
        public int NumeroDeVacas { get; set; }
        public double TaxaNatalidade { get; set; }
        public double MediaMensalPorVaca { get; set; }
    }

    public class AlertaResultado
    {
        public List<Alerta> Alertas { get; set; }
        public Estatisticas Estatisticas { get; set; }
        public int TotalAlertas { get; set; }
    }

    public class AlertaService
    {
        private readonly string _connectionString;

        public AlertaService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public AlertaResultado GerarAlertas(int usuarioId)
        {
            var alertas = new List<Alerta>();
            var estatisticas = new Estatisticas();

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                var brincos = new Dictionary<int, string>();
                string sqlBrincos = "SELECT ID, Brinco FROM Gado WHERE ID_Usuario = @id";
                using (var cmd = new SqlCommand(sqlBrincos, conn))
                {
                    cmd.Parameters.AddWithValue("@id", usuarioId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(0);
                            string brinco = $"Gado {id}";
                            if (!reader.IsDBNull(1))
                            {
                                var val = reader.GetValue(1);
                                brinco = Convert.ToString(val);
                            }
                            brincos[id] = brinco;
                        }
                    }
                }


                var lotes = new Dictionary<int, int>();
                string sqlLotes = "SELECT Id, Num FROM Lote WHERE ID_Usuario = @id";
                using (var cmd = new SqlCommand(sqlLotes, conn))
                {
                    cmd.Parameters.AddWithValue("@id", usuarioId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int idLote = reader.GetInt32(0);
                            int num = reader.IsDBNull(1) ? idLote : reader.GetInt32(1);
                            lotes[idLote] = num;
                        }
                    }
                }


                string sqlProducaoDiaria = @"SELECT ISNULL(SUM(Litros), 0) 
                                             FROM Leite 
                                             WHERE ID_Usuario = @id 
                                               AND CAST(Data AS DATE) = CAST(GETDATE() AS DATE)";
                using (var cmd = new SqlCommand(sqlProducaoDiaria, conn))
                {
                    cmd.Parameters.AddWithValue("@id", usuarioId);
                    estatisticas.ProducaoTotalDiaria = Convert.ToDouble(cmd.ExecuteScalar());
                }

                string sqlVacas = @"SELECT COUNT(*) FROM Gado WHERE ID_Usuario = @id";
                using (var cmd = new SqlCommand(sqlVacas, conn))
                {
                    cmd.Parameters.AddWithValue("@id", usuarioId);
                    estatisticas.NumeroDeVacas = Convert.ToInt32(cmd.ExecuteScalar());
                }

                string sqlNatalidade = @"SELECT 
                                            (SELECT COUNT(*) FROM Prenhez WHERE ID_Usuario = @id AND Status = 'Parida') * 1.0 /
                                            NULLIF((SELECT COUNT(*) FROM Gado WHERE ID_Usuario = @id), 0)";
                using (var cmd = new SqlCommand(sqlNatalidade, conn))
                {
                    cmd.Parameters.AddWithValue("@id", usuarioId);
                    var taxa = cmd.ExecuteScalar();
                    estatisticas.TaxaNatalidade = taxa == DBNull.Value ? 0 : Math.Round(Convert.ToDouble(taxa) * 100, 2);
                }

                string sqlMediaMensal = @"
                    SELECT 
                        ISNULL(SUM(Litros) / NULLIF(COUNT(DISTINCT ID_Gado), 0), 0)
                    FROM Leite 
                    WHERE ID_Usuario = @id 
                      AND MONTH(Data) = MONTH(GETDATE())
                      AND YEAR(Data) = YEAR(GETDATE())";
                using (var cmd = new SqlCommand(sqlMediaMensal, conn))
                {
                    cmd.Parameters.AddWithValue("@id", usuarioId);
                    estatisticas.MediaMensalPorVaca = Math.Round(Convert.ToDouble(cmd.ExecuteScalar()), 2);
                }



                double mediaEsperada = 12.0;
                DateTime umaSemanaAtras = DateTime.Now.AddDays(-7);

                string sqlLeiteSemana = @"
                    SELECT ID_Gado, AVG(CAST(Litros AS float)) AS MediaLitros
                    FROM Leite
                    WHERE ID_Usuario = @id
                      AND Data >= @dataInicio
                    GROUP BY ID_Gado";

                using (var cmd = new SqlCommand(sqlLeiteSemana, conn))
                {
                    cmd.Parameters.AddWithValue("@id", usuarioId);
                    cmd.Parameters.AddWithValue("@dataInicio", umaSemanaAtras);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int idGado = reader.GetInt32(0);
                            double mediaLitros = reader.IsDBNull(1) ? 0 : reader.GetDouble(1);

                            if (mediaLitros < mediaEsperada * 0.9)
                                alertas.Add(new Alerta { Tipo = "danger", Mensagem = $"Brinco {brincos.GetValueOrDefault(idGado, idGado.ToString())}: média da última semana abaixo da esperada ({mediaLitros:F1} L)", Origem = "Leite" });
                            else if (mediaLitros < mediaEsperada)
                                alertas.Add(new Alerta { Tipo = "warning", Mensagem = $"Brinco {brincos.GetValueOrDefault(idGado, idGado.ToString())}: média da última semana próxima do mínimo ({mediaLitros:F1} L)", Origem = "Leite" });
                            else if (mediaLitros > mediaEsperada * 1.1)
                                alertas.Add(new Alerta { Tipo = "info", Mensagem = $"Brinco {brincos.GetValueOrDefault(idGado, idGado.ToString())}: média da última semana acima da esperada ({mediaLitros:F1} L)", Origem = "Leite" });
                        }
                    }
                }

                string sqlQualidade = @"SELECT ID_Lote, CCS, Gordura, Proteina FROM Qualidade WHERE ID_Usuario = @id";
                using (var cmd = new SqlCommand(sqlQualidade, conn))
                {
                    cmd.Parameters.AddWithValue("@id", usuarioId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        var lotesQualidade = new List<(int idLote, decimal ccs, decimal gordura, decimal proteina)>();
                        while (reader.Read())
                        {
                            int idLote = reader.GetInt32(0);
                            decimal ccs = reader.IsDBNull(1) ? 0m : Convert.ToDecimal(reader.GetValue(1));
                            decimal gordura = reader.IsDBNull(2) ? 0m : Convert.ToDecimal(reader.GetValue(2));
                            decimal proteina = reader.IsDBNull(3) ? 0m : Convert.ToDecimal(reader.GetValue(3));
                            lotesQualidade.Add((idLote, ccs, gordura, proteina));
                        }
                        reader.Close();

                        foreach (var lote in lotesQualidade)
                        {
                            int loteNum = lotes.GetValueOrDefault(lote.idLote, lote.idLote);

                            string sqlGados = @"SELECT ID_Gado FROM Leite WHERE Lote = @loteNum AND ID_Usuario = @id";
                            using (var cmdGado = new SqlCommand(sqlGados, conn))
                            {
                                cmdGado.Parameters.AddWithValue("@loteNum", lote.idLote.ToString());
                                cmdGado.Parameters.AddWithValue("@id", usuarioId);
                                using (var readerGado = cmdGado.ExecuteReader())
                                {
                                    var gados = new List<int>();
                                    while (readerGado.Read())
                                        gados.Add(readerGado.GetInt32(0));

                                    foreach (var idGado in gados)
                                    {
                                        string brinco = brincos.GetValueOrDefault(idGado, idGado.ToString());
                                        if (lote.ccs < 100000)
                                            alertas.Add(new Alerta { Tipo = "info", Mensagem = $"Brinco {brinco} no lote {loteNum}: CCS excelente (<100k)", Origem = "Qualidade" });
                                        else if (lote.ccs > 300000)
                                            alertas.Add(new Alerta { Tipo = "danger", Mensagem = $"Brinco {brinco} no lote {loteNum}: CCS alto (>300k)", Origem = "Qualidade" });
                                        else if (lote.ccs > 200000)
                                            alertas.Add(new Alerta { Tipo = "warning", Mensagem = $"Brinco {brinco} no lote {loteNum}: CCS elevado (200k–300k)", Origem = "Qualidade" });

                                        if (lote.gordura < 2.5m)
                                            alertas.Add(new Alerta { Tipo = "warning", Mensagem = $"Brinco {brinco} no lote {loteNum}: gordura baixa (<2.5%)", Origem = "Qualidade" });
                                        else if (lote.gordura > 4.0m)
                                            alertas.Add(new Alerta { Tipo = "warning", Mensagem = $"Brinco {brinco} no lote {loteNum}: gordura alta (>4.0%)", Origem = "Qualidade" });

                                        if (lote.proteina < 3.2m)
                                            alertas.Add(new Alerta { Tipo = "warning", Mensagem = $"Brinco {brinco} no lote {loteNum}: proteína baixa (<3.2%)", Origem = "Qualidade" });
                                        else if (lote.proteina > 4.0m)
                                            alertas.Add(new Alerta { Tipo = "warning", Mensagem = $"Brinco {brinco} no lote {loteNum}: proteína alta (>4.0%)", Origem = "Qualidade" });
                                    }
                                }
                            }
                        }
                    }
                }


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
                            string nome = reader.IsDBNull(2) ? string.Empty : Convert.ToString(reader.GetValue(2));
                            DateTime inicio = reader.GetDateTime(3);
                            int intervalo = reader.GetInt32(4);
                            int totalDoses = reader.GetInt32(5);
                            int dosesAplicadas = reader.GetInt32(6);

                            int dosesRestantes = totalDoses - dosesAplicadas;
                            DateTime? proximaDose = dosesAplicadas < totalDoses
                                ? inicio.AddHours(intervalo * dosesAplicadas)
                                : (DateTime?)null;

                            string brinco = brincos.GetValueOrDefault(idGado, idGado.ToString());

                            if (dosesRestantes > 0 && proximaDose.HasValue)
                            {
                                if (proximaDose.Value < DateTime.Now)
                                {
                                    alertas.Add(new Alerta
                                    {
                                        Tipo = "danger",
                                        Mensagem = $"Brinco {brinco} -> medicamento {nome} em atraso ({dosesRestantes} doses restantes, próxima deveria ser {proximaDose:dd/MM HH:mm})",
                                        Origem = "Remedio",
                                        ID_Gado = idGado
                                    });
                                }
                                else if (proximaDose.Value <= DateTime.Now.AddHours(6))
                                {
                                    alertas.Add(new Alerta
                                    {
                                        Tipo = "warning",
                                        Mensagem = $"Brinco {brinco} -> deve tomar {nome} em breve ({dosesRestantes} doses restantes, próxima às {proximaDose:HH:mm dd/MM})",
                                        Origem = "Remedio",
                                        ID_Gado = idGado
                                    });
                                }
                            }
                        }
                    }
                }

                string sqlSuplemento = @"SELECT ID_Gado, Nome, Date, intervalo FROM Suplemento WHERE ID_Usuario = @id";
                using (var cmd = new SqlCommand(sqlSuplemento, conn))
                {
                    cmd.Parameters.AddWithValue("@id", usuarioId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int idGado = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                            string nome = reader.IsDBNull(1) ? string.Empty : Convert.ToString(reader.GetValue(1));
                            DateTime data = reader.GetDateTime(2);
                            int intervalo = reader.GetInt32(3);
                            DateTime proxima = data.AddDays(intervalo);

                            string brinco = brincos.GetValueOrDefault(idGado, idGado.ToString());

                            if (proxima < DateTime.Now)
                                alertas.Add(new Alerta { Tipo = "danger", Mensagem = $"Brinco {brinco} -> suplemento {nome} em atraso", Origem = "Suplemento" });
                            else if (proxima <= DateTime.Now.AddDays(3))
                                alertas.Add(new Alerta { Tipo = "warning", Mensagem = $"Brinco {brinco} -> deve receber suplemento {nome} em breve", Origem = "Suplemento" });
                        }
                    }
                }

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
                            string status = reader.IsDBNull(3) ? "Aguardando Diagnóstico" : Convert.ToString(reader.GetValue(3));
                            string brinco = brincos.GetValueOrDefault(idGado, idGado.ToString());

                            if (status == "Aguardando Diagnóstico" && dataEsperada.HasValue)
                            {
                                if (dataEsperada.Value < DateTime.Now)
                                    alertas.Add(new Alerta { Tipo = "danger", Mensagem = $"Brinco {brinco} -> diagnóstico de prenhez atrasado", Origem = "Prenhez" });
                                else if (dataEsperada.Value <= DateTime.Now.AddDays(5))
                                    alertas.Add(new Alerta { Tipo = "warning", Mensagem = $"Brinco {brinco} -> diagnóstico de prenhez em breve", Origem = "Prenhez" });
                            }

                            DateTime dataParto = dataPrenhez.AddDays(280);
                            DateTime dataSecagem = dataParto.AddDays(-60);

                            if (dataSecagem <= DateTime.Now.AddDays(7) && status != "Seca")
                            {
                                if (dataSecagem < DateTime.Now)
                                    alertas.Add(new Alerta { Tipo = "danger", Mensagem = $"Brinco {brinco} -> já deveria estar seca", Origem = "Prenhez" });
                                else
                                    alertas.Add(new Alerta { Tipo = "warning", Mensagem = $"Brinco {brinco} -> deve ser seca na próxima semana", Origem = "Prenhez" });
                            }

                            if (dataParto <= DateTime.Now.AddDays(7) && status != "Parida")
                            {
                                if (dataParto < DateTime.Now)
                                    alertas.Add(new Alerta { Tipo = "danger", Mensagem = $"Brinco {brinco} -> já deveria ter parido", Origem = "Prenhez" });
                                else
                                    alertas.Add(new Alerta { Tipo = "warning", Mensagem = $"Brinco {brinco} -> prestes a parir", Origem = "Prenhez" });
                            }
                        }
                    }
                }
            }

            int totalAlertas = alertas.Count;

            return new AlertaResultado
            {
                Alertas = alertas,
                Estatisticas = estatisticas,
                TotalAlertas = totalAlertas
            };
        }
    }
}
