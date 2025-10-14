using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace MilkApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VacaController : Controller
    {
        private readonly string ConnectionString = config.ConnectionString;
        private readonly ILogger<VacaController> _logger;

        public VacaController(ILogger<VacaController> logger)
        {
            _logger = logger;
        }

        [HttpGet("ResumoVaca/{id}")]
        public async Task<ActionResult<ResumoVacaDTO>> GetResumoVaca(int id)
        {
            var resumo = new ResumoVacaDTO();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                await conn.OpenAsync();

                var queryGado = "SELECT Id, Brinco, Raca FROM Gado WHERE Id = @Id";
                using (var cmd = new SqlCommand(queryGado, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    var reader = await cmd.ExecuteReaderAsync();
                    if (!await reader.ReadAsync()) return NotFound($"Gado {id} não encontrado");

                    resumo.Id = Convert.ToInt32(reader["Id"]);
                    resumo.Brinco = reader["Brinco"].ToString()!;
                    resumo.Raca = reader["Raca"].ToString()!;
                    reader.Close();
                }

                var queryLeite = "SELECT * FROM Leite WHERE ID_Gado = @Id";
                using (var cmd = new SqlCommand(queryLeite, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        resumo.Leites.Add(new Leite
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            ID_Gado = id,
                            Data = Convert.ToDateTime(reader["Data"]),
                            Litros = Convert.ToDecimal(reader["Litros"]),
                            ID_Usuario = Convert.ToInt32(reader["ID_Usuario"])
                        });
                    }
                    reader.Close();
                }

                var queryManejo = "SELECT * FROM ManejoGeral WHERE ID_Gado = @Id";
                using (var cmd = new SqlCommand(queryManejo, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        resumo.Manejos.Add(new ManejoGeral
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            ID_Gado = id,
                            Tipo_Manejo = reader["Tipo_Manejo"].ToString()!,
                            Data_Manejo = Convert.ToDateTime(reader["Data_Manejo"]),
                            Observacoes = reader["Observacoes"].ToString()!,
                            ID_Usuario = Convert.ToInt32(reader["ID_Usuario"])
                        });
                    }
                    reader.Close();
                }

                var queryPrenhez = "SELECT * FROM Prenhez WHERE ID_Gado = @Id";
                using (var cmd = new SqlCommand(queryPrenhez, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        resumo.Prenhezes.Add(new Prenhez
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            ID_Gado = id,
                            Data_Prenhez = Convert.ToDateTime(reader["Data_Prenhez"]),
                            Data_Termino = reader["Data_Termino"] as DateTime?,
                            Data_Esperada = reader["Data_Esperada"] as DateTime?,
                            Status = reader["Status"].ToString()!,
                            ID_Usuario = Convert.ToInt32(reader["ID_Usuario"])
                        });
                    }
                    reader.Close();
                }

                var queryRemedio = "SELECT * FROM Remedio WHERE ID_Gado = @Id";
                using (var cmd = new SqlCommand(queryRemedio, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        resumo.Remedios.Add(new Remedio
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            ID_Gado = id,
                            Nome = reader["Nome"].ToString()!,
                            Date = Convert.ToDateTime(reader["Date"]),
                            Doses = Convert.ToInt32(reader["Doses"]),
                            intervalo = Convert.ToInt32(reader["intervalo"]),
                            via = reader["via"].ToString()!,
                            ID_Usuario = Convert.ToInt32(reader["ID_Usuario"])
                        });
                    }
                    reader.Close();
                }

                var queryRepro = "SELECT * FROM Reproducao WHERE ID_Gado = @Id";
                using (var cmd = new SqlCommand(queryRepro, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        resumo.Reproducoes.Add(new Reproducao
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            ID_Gado = id,
                            Tipo = reader["Tipo"].ToString()!,
                            Data = Convert.ToDateTime(reader["Data"]),
                            Observacao = reader["Observacao"].ToString()!,
                            ID_Usuario = Convert.ToInt32(reader["ID_Usuario"])
                        });
                    }
                    reader.Close();
                }

                var querySup = "SELECT * FROM Suplemento WHERE ID_Gado = @Id";
                using (var cmd = new SqlCommand(querySup, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        resumo.Suplementos.Add(new Suplemento
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            ID_Gado = id,
                            Tipo = reader["Tipo"].ToString()!,
                            Nome = reader["Nome"].ToString()!,
                            Date = Convert.ToDateTime(reader["Date"]),
                            intervalo = Convert.ToInt32(reader["intervalo"]),
                            ID_Usuario = Convert.ToInt32(reader["ID_Usuario"])
                        });
                    }
                    reader.Close();
                }

                var queryAlertas = "SELECT * FROM Alertas WHERE ID_Gado = @Id";
                using (var cmd = new SqlCommand(queryAlertas, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        resumo.Alertas.Add(new Alertas
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            ID_Gado = id,
                            Data_Prevista = Convert.ToDateTime(reader["Data_Prevista"]),
                            Status = reader["Status"].ToString()!,
                            ID_Usuario = Convert.ToInt32(reader["ID_Usuario"])
                        });
                    }
                    reader.Close();
                }

                var queryLotes = @"
                SELECT 
                    l.Id AS LoteId,
                    l.Num,
                    l.ID_Usuario,
        
                    ll.ID_Leite,
        
                    le.Litros,
                    le.Data,
                    le.ID_Gado AS LeiteGadoId,
        
                    q.Id AS QualidadeId,
                    q.CCS,
                    q.Gordura,
                    q.Proteina
                FROM Lote l
                LEFT JOIN LoteLeite ll ON l.Id = ll.ID_Lote
                LEFT JOIN Leite le ON ll.ID_Leite = le.Id
                LEFT JOIN Qualidade q ON l.Id = q.ID_Lote
                WHERE le.ID_Gado = @Id 
                   OR l.ID_Usuario = (SELECT ID_Usuario FROM Gado WHERE Id = @Id)";

                using (var cmd = new SqlCommand(queryLotes, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    var reader = await cmd.ExecuteReaderAsync();

                    var lotesDict = new Dictionary<int, Lote>();

                    while (await reader.ReadAsync())
                    {
                        int loteId = Convert.ToInt32(reader["LoteId"]);
                        if (!lotesDict.ContainsKey(loteId))
                        {
                            lotesDict[loteId] = new Lote
                            {
                                Id = loteId,
                                Num = Convert.ToInt32(reader["Num"]),
                                ID_Usuario = Convert.ToInt32(reader["ID_Usuario"]),
                                leites = new List<Leite>(),
                                qualidade = null
                            };
                        }

                        var lote = lotesDict[loteId];

                        if (reader["ID_Leite"] != DBNull.Value)
                        {
                            lote.leites.Add(new Leite
                            {
                                Id = Convert.ToInt32(reader["ID_Leite"]),
                                ID_Gado = reader["LeiteGadoId"] != DBNull.Value ? Convert.ToInt32(reader["LeiteGadoId"]) : 0,
                                Data = reader["Data"] != DBNull.Value ? Convert.ToDateTime(reader["Data"]) : DateTime.MinValue,
                                Litros = reader["Litros"] != DBNull.Value ? Convert.ToDecimal(reader["Litros"]) : 0,
                                ID_Usuario = Convert.ToInt32(reader["ID_Usuario"])
                            });
                        }

                        if (reader["QualidadeId"] != DBNull.Value && lote.qualidade == null)
                        {
                            lote.qualidade = new Qualidade
                            {
                                Id = Convert.ToInt32(reader["QualidadeId"]),
                                ID_Lote = loteId,
                                CCS = Convert.ToInt32(reader["CCS"]),
                                Gordura = Convert.ToDecimal(reader["Gordura"]),
                                Proteina = Convert.ToDecimal(reader["Proteina"]),
                                ID_Usuario = Convert.ToInt32(reader["ID_Usuario"])
                            };
                        }
                    }

                    resumo.Lotes = lotesDict.Values.ToList();
                    reader.Close();
                }
            }

            return Ok(resumo);
        }


        [HttpGet("ResumoUsuario/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<VacaResumoUsuarioDTO>>> GetResumoVacasUsuario(int usuarioId)
        {
            var vacasResumo = new List<VacaResumoUsuarioDTO>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                await conn.OpenAsync();

                var query = @"
        SELECT 
            g.Id,
            CAST(g.Brinco AS VARCHAR) AS Brinco,
            g.Raca,
            g.Sexo,
            (
                ISNULL(l.qtd,0) + ISNULL(m.qtd,0) + ISNULL(p.qtd,0) +
                ISNULL(r.qtd,0) + ISNULL(re.qtd,0) + ISNULL(s.qtd,0) +
                ISNULL(a.qtd,0) + ISNULL(lo.qtd,0) + ISNULL(q.qtd,0)
            ) as TotalRegistros,
            IIF(ISNULL(l.qtd,0) > 0, CAST(1 AS BIT), CAST(0 AS BIT)) as TemLeite,
            IIF(ISNULL(m.qtd,0) > 0, CAST(1 AS BIT), CAST(0 AS BIT)) as TemManejo,
            IIF(ISNULL(p.qtd,0) > 0, CAST(1 AS BIT), CAST(0 AS BIT)) as TemPrenhez,
            IIF(ISNULL(r.qtd,0) > 0, CAST(1 AS BIT), CAST(0 AS BIT)) as TemRemedio,
            IIF(ISNULL(re.qtd,0) > 0, CAST(1 AS BIT), CAST(0 AS BIT)) as TemReproducao,
            IIF(ISNULL(s.qtd,0) > 0, CAST(1 AS BIT), CAST(0 AS BIT)) as TemSuplemento,
            IIF(ISNULL(a.qtd,0) > 0, CAST(1 AS BIT), CAST(0 AS BIT)) as TemAlerta,
            IIF(ISNULL(lo.qtd,0) > 0, CAST(1 AS BIT), CAST(0 AS BIT)) as TemLote,
            IIF(ISNULL(q.qtd,0) > 0, CAST(1 AS BIT), CAST(0 AS BIT)) as TemQualidade
        FROM Gado g
        LEFT JOIN (SELECT ID_Gado, COUNT(*) as qtd FROM Leite GROUP BY ID_Gado) l ON g.Id = l.ID_Gado
        LEFT JOIN (SELECT ID_Gado, COUNT(*) as qtd FROM ManejoGeral GROUP BY ID_Gado) m ON g.Id = m.ID_Gado
        LEFT JOIN (SELECT ID_Gado, COUNT(*) as qtd FROM Prenhez GROUP BY ID_Gado) p ON g.Id = p.ID_Gado
        LEFT JOIN (SELECT ID_Gado, COUNT(*) as qtd FROM Remedio GROUP BY ID_Gado) r ON g.Id = r.ID_Gado
        LEFT JOIN (SELECT ID_Gado, COUNT(*) as qtd FROM Reproducao GROUP BY ID_Gado) re ON g.Id = re.ID_Gado
        LEFT JOIN (SELECT ID_Gado, COUNT(*) as qtd FROM Suplemento GROUP BY ID_Gado) s ON g.Id = s.ID_Gado
        LEFT JOIN (SELECT ID_Gado, COUNT(*) as qtd FROM Alertas GROUP BY ID_Gado) a ON g.Id = a.ID_Gado
        LEFT JOIN (
            SELECT le.ID_Gado, COUNT(DISTINCT ll.ID_Lote) as qtd
            FROM LoteLeite ll
            INNER JOIN Leite le ON ll.ID_Leite = le.Id
            GROUP BY le.ID_Gado
        ) lo ON g.Id = lo.ID_Gado
        LEFT JOIN (
            SELECT g.Id AS ID_Gado, COUNT(q.Id) AS qtd
            FROM Gado g
            LEFT JOIN Lote l ON l.ID_Usuario = g.ID_Usuario
            LEFT JOIN Qualidade q ON q.ID_Lote = l.Id
            GROUP BY g.Id
        ) q ON g.Id = q.ID_Gado
        WHERE g.ID_Usuario = @UsuarioId;
        ";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);

                    var reader = await cmd.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        vacasResumo.Add(new VacaResumoUsuarioDTO
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Brinco = reader["Brinco"].ToString()!,
                            Raca = reader["Raca"].ToString()!,
                            Sexo = reader["Sexo"].ToString()!,
                            TotalRegistros = Convert.ToInt32(reader["TotalRegistros"]),
                            TemLeite = Convert.ToBoolean(reader["TemLeite"]),
                            TemManejo = Convert.ToBoolean(reader["TemManejo"]),
                            TemPrenhez = Convert.ToBoolean(reader["TemPrenhez"]),
                            TemRemedio = Convert.ToBoolean(reader["TemRemedio"]),
                            TemReproducao = Convert.ToBoolean(reader["TemReproducao"]),
                            TemSuplemento = Convert.ToBoolean(reader["TemSuplemento"]),
                            TemAlerta = Convert.ToBoolean(reader["TemAlerta"]),
                            TemLote = Convert.ToBoolean(reader["TemLote"]),
                            TemQualidade = Convert.ToBoolean(reader["TemQualidade"])
                        });
                    }

                    reader.Close();
                }
            }

            return Ok(vacasResumo);
        }





    }
}
