using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;

namespace MilkApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EstatisticasController : ControllerBase
    {
        private readonly string ConnectionString = config.ConnectionString;

        [HttpGet("{usuarioId}")]
        public ActionResult<object> GetEstatisticas(int usuarioId)
        {
            try
            {
                using var conn = new SqlConnection(ConnectionString);
                conn.Open();

                double totalHoje = GetDouble(conn, "SELECT ISNULL(SUM(Litros), 0) FROM Leite WHERE ID_Usuario = @id AND CAST(Data AS DATE) = CAST(GETDATE() AS DATE)", usuarioId);
                double mediaPorVacaHoje = GetDouble(conn, "SELECT ISNULL(AVG(CAST(Litros AS float)), 0) FROM Leite WHERE ID_Usuario = @id AND CAST(Data AS DATE) = CAST(GETDATE() AS DATE)", usuarioId);

                double overallAvg7;
                using (var cmd = new SqlCommand(@"SELECT AVG(Media) FROM (
                                                    SELECT AVG(CAST(Litros AS float)) AS Media
                                                    FROM Leite
                                                    WHERE ID_Usuario = @id AND Data >= DATEADD(DAY, -7, GETDATE())
                                                    GROUP BY ID_Gado
                                                ) t", conn))
                {
                    cmd.Parameters.AddWithValue("@id", usuarioId);
                    var val = cmd.ExecuteScalar();
                    overallAvg7 = val == DBNull.Value ? 0 : Convert.ToDouble(val);
                }

                double percentualAbaixoMedia = 0;
                if (overallAvg7 > 0)
                {
                    long abaixoCount;
                    long totalCount;
                    using (var cmd = new SqlCommand(@"SELECT COUNT(*) FROM (
                                                        SELECT AVG(CAST(Litros AS float)) AS Media
                                                        FROM Leite
                                                        WHERE ID_Usuario = @id AND Data >= DATEADD(DAY, -7, GETDATE())
                                                        GROUP BY ID_Gado
                                                      ) t WHERE Media < @avg", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", usuarioId);
                        cmd.Parameters.AddWithValue("@avg", overallAvg7);
                        abaixoCount = Convert.ToInt64(cmd.ExecuteScalar());
                    }
                    using (var cmd = new SqlCommand(@"SELECT COUNT(*) FROM (
                                                        SELECT AVG(CAST(Litros AS float)) AS Media
                                                        FROM Leite
                                                        WHERE ID_Usuario = @id AND Data >= DATEADD(DAY, -7, GETDATE())
                                                        GROUP BY ID_Gado
                                                      ) t", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", usuarioId);
                        totalCount = Convert.ToInt64(cmd.ExecuteScalar());
                    }
                    percentualAbaixoMedia = totalCount == 0 ? 0 : Math.Round(abaixoCount * 100.0 / totalCount, 2);
                }

                string ultimaColeta = GetString(conn, "SELECT CONVERT(VARCHAR(19), MAX(Data), 120) FROM Leite WHERE ID_Usuario = @id", usuarioId);

                int totalVacas = GetInt(conn, "SELECT COUNT(*) FROM Gado WHERE ID_Usuario = @id", usuarioId);
                int nascimentosMes = GetInt(conn, "SELECT COUNT(*) FROM Prenhez WHERE ID_Usuario = @id AND Status = 'Parida' AND MONTH(Data_Termino) = MONTH(GETDATE()) AND YEAR(Data_Termino) = YEAR(GETDATE())", usuarioId);
                double percentualLactantes = GetDouble(conn, "SELECT CASE WHEN COUNT(*)=0 THEN 0 ELSE (COUNT(CASE WHEN StatusProdutivo LIKE 'Lactante%' THEN 1 END)*100.0/COUNT(*)) END FROM Gado WHERE ID_Usuario = @id", usuarioId);
                double idadeMedia = GetDouble(conn, "SELECT ISNULL(AVG(DATEDIFF(DAY, Data_Nasc, GETDATE())/30.4375), 0) FROM Gado WHERE ID_Usuario = @id AND Data_Nasc IS NOT NULL", usuarioId);

                int vacasPrenhas = GetInt(conn, "SELECT COUNT(*) FROM Prenhez WHERE ID_Usuario = @id AND Status = 'Prenha'", usuarioId);
                int proximosPartos = GetInt(conn, "SELECT COUNT(*) FROM Prenhez WHERE ID_Usuario = @id AND Data_Esperada BETWEEN GETDATE() AND DATEADD(DAY, 30, GETDATE())", usuarioId);
                double taxaSucesso = GetDouble(conn, @"SELECT CASE WHEN (SELECT COUNT(*) FROM Reproducao WHERE ID_Usuario = @id)=0 THEN 0 ELSE (SELECT COUNT(*) FROM Prenhez WHERE ID_Usuario = @id AND Status IN ('Prenha','Parida'))*100.0/(SELECT COUNT(*) FROM Reproducao WHERE ID_Usuario = @id) END", usuarioId);

                double intervaloMedio = 0;
                using (var cmd = new SqlCommand(@"
                    WITH Partos AS (
                        SELECT ID_Gado, Data_Termino,
                               ROW_NUMBER() OVER(PARTITION BY ID_Gado ORDER BY Data_Termino) rn
                        FROM Prenhez
                        WHERE ID_Usuario = @id AND Data_Termino IS NOT NULL
                    )
                    SELECT AVG(CAST(DATEDIFF(DAY, p1.Data_Termino, p2.Data_Termino) AS float))
                    FROM Partos p1
                    INNER JOIN Partos p2 ON p1.ID_Gado = p2.ID_Gado AND p2.rn = p1.rn + 1", conn))
                {
                    cmd.Parameters.AddWithValue("@id", usuarioId);
                    var val = cmd.ExecuteScalar();
                    intervaloMedio = val == DBNull.Value ? 0 : Math.Round(Convert.ToDouble(val), 0);
                }

                var resultado = new
                {
                    producao = new
                    {
                        totalHoje = Math.Round(totalHoje, 2),
                        mediaPorVaca = Math.Round(mediaPorVacaHoje, 2),
                        percentualAbaixoMedia = percentualAbaixoMedia,
                        ultimaColeta = string.IsNullOrEmpty(ultimaColeta) ? null : ultimaColeta
                    },
                    geral = new
                    {
                        totalVacas = totalVacas,
                        nascimentosMes = nascimentosMes,
                        percentualLactantes = Math.Round(percentualLactantes, 2),
                        idadeMediaMeses = Math.Round(idadeMedia, 2)
                    },
                    reproducao = new
                    {
                        vacasPrenhas = vacasPrenhas,
                        proximosPartos = proximosPartos,
                        taxaSucesso = Math.Round(taxaSucesso, 2),
                        intervaloMedio = Convert.ToInt32(intervaloMedio)
                    }
                };

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar estatísticas: {ex.Message}");
            }
        }

        private static double GetDouble(SqlConnection conn, string sql, int usuarioId)
        {
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", usuarioId);
            var result = cmd.ExecuteScalar();
            return result == DBNull.Value ? 0 : Convert.ToDouble(result);
        }

        private static int GetInt(SqlConnection conn, string sql, int usuarioId)
        {
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", usuarioId);
            var result = cmd.ExecuteScalar();
            return result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }

        private static string GetString(SqlConnection conn, string sql, int usuarioId)
        {
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", usuarioId);
            var result = cmd.ExecuteScalar();
            return result == DBNull.Value ? null : Convert.ToString(result);
        }
    }
}
