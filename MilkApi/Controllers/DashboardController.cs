using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace MilkApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DashboardController : Controller
    {
        private readonly string ConnectionString = config.ConnectionString; // Substitua com sua configuração
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ILogger<DashboardController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{usuarioId}")]
        public async Task<IActionResult> GetDashboard(int usuarioId)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                await conn.OpenAsync();

                var query = @"
                    SELECT g.Id, g.Brinco, g.Raca,
                           l.Id AS LeiteId, l.Litros, l.Data AS LeiteData,
                           p.Id AS PrenhezId, p.Data_Prenhez, p.Data_Termino, p.Data_Esperada, p.Status AS PrenhezStatus,
                           rep.Id AS ReproducaoId, rep.Tipo AS ReproTipo,
                           ll.Id AS LoteLeiteId, lo.Id AS LoteId, lo.Num AS LoteNum,
                           q.Id AS QualidadeId, q.CCS, q.Gordura, q.Proteina
                    FROM Gado g
                    LEFT JOIN Leite l ON l.ID_Gado = g.Id
                    LEFT JOIN Prenhez p ON p.ID_Gado = g.Id
                    LEFT JOIN Reproducao rep ON rep.ID_Gado = g.Id
                    LEFT JOIN LoteLeite ll ON ll.ID_Leite = l.Id
                    LEFT JOIN Lote lo ON lo.Id = ll.ID_Lote
                    LEFT JOIN Qualidade q ON q.ID_Lote = lo.Id
                    WHERE g.ID_Usuario = @UsuarioId";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);
                    var reader = await cmd.ExecuteReaderAsync();

                    var vacasDict = new Dictionary<int, dynamic>();

                    while (await reader.ReadAsync())
                    {
                        int vacaId = Convert.ToInt32(reader["Id"]);
                        if (!vacasDict.ContainsKey(vacaId))
                        {
                            vacasDict[vacaId] = new
                            {
                                Leites = new List<dynamic>(),
                                Prenhezes = new List<dynamic>(),
                                Reproducoes = new List<dynamic>(),
                                Lotes = new List<dynamic>()
                            };
                        }

                        var vaca = vacasDict[vacaId];

                        // Leite
                        if (reader["LeiteId"] != DBNull.Value)
                        {
                            vaca.Leites.Add(new
                            {
                                litros = Convert.ToDecimal(reader["Litros"]),
                                data = Convert.ToDateTime(reader["LeiteData"])
                            });
                        }

                        // Prenhez
                        if (reader["PrenhezId"] != DBNull.Value)
                        {
                            DateTime? dataPrenhez = reader["Data_Prenhez"] != DBNull.Value
                                ? Convert.ToDateTime(reader["Data_Prenhez"])
                                : (DateTime?)null;

                            DateTime? dataTermino = reader["Data_Termino"] != DBNull.Value
                                ? Convert.ToDateTime(reader["Data_Termino"])
                                : (DateTime?)null;

                            DateTime? dataEsperada = reader["Data_Esperada"] != DBNull.Value
                                ? Convert.ToDateTime(reader["Data_Esperada"])
                                : (DateTime?)null;

                            vaca.Prenhezes.Add(new
                            {
                                dataPrenhez,
                                dataTermino,
                                dataEsperada,
                                status = reader["PrenhezStatus"]?.ToString()
                            });
                        }

                        // Reprodução
                        if (reader["ReproducaoId"] != DBNull.Value)
                        {
                            vaca.Reproducoes.Add(new
                            {
                                tipo = reader["ReproTipo"]?.ToString()
                            });
                        }

                        // Lotes e qualidade
                        if (reader["LoteId"] != DBNull.Value)
                        {
                            var lote = new
                            {
                                Id = Convert.ToInt32(reader["LoteId"]),
                                Num = reader["LoteNum"]?.ToString(),
                                qualidade = reader["QualidadeId"] != DBNull.Value ? new
                                {
                                    CCS = Convert.ToDecimal(reader["CCS"]),
                                    Gordura = Convert.ToDecimal(reader["Gordura"]),
                                    Proteina = Convert.ToDecimal(reader["Proteina"])
                                } : null
                            };
                            vaca.Lotes.Add(lote);
                        }
                    }

                    reader.Close();

                    var vacas = vacasDict.Values.ToList();
                    DateTime hoje = DateTime.Now.Date;

                    // Períodos
                    var periodos = new Dictionary<string, DateTime?>
                    {
                        { "1Semana", hoje.AddDays(-7) },
                        { "1Mes", hoje.AddMonths(-1) },
                        { "6Meses", hoje.AddMonths(-6) },
                        { "1Ano", hoje.AddYears(-1) },
                        { "2Anos", hoje.AddYears(-2) },
                        { "3Anos", hoje.AddYears(-3) },
                        { "4Anos", hoje.AddYears(-4) },
                        { "5Anos", hoje.AddYears(-5) },
                        { "Total", null } // Sem limite
                    };

                    // Inicializar dicionários de estatísticas
                    var producaoPorPeriodo = new Dictionary<string, dynamic>();
                    var qualidadePorPeriodo = new Dictionary<string, dynamic>();
                    var reproducaoPorPeriodo = new Dictionary<string, dynamic>();

                    foreach (var p in periodos)
                    {
                        DateTime? limite = p.Value;

                        // Produção
                        decimal totalLeite = 0;
                        decimal somaVaca = 0;
                        int totalVacasPeriodo = vacas.Count;
                        foreach (var v in vacas)
                        {
                            decimal somaV = 0;
                            foreach (var l in v.Leites)
                            {
                                if (limite == null || l.data >= limite)
                                    somaV += l.litros;
                            }
                            somaVaca += somaV;
                            totalLeite += somaV;
                        }

                        decimal mediaVaca = totalVacasPeriodo > 0 ? somaVaca / totalVacasPeriodo : 0;

                        producaoPorPeriodo[p.Key] = new
                        {
                            totalLeite,
                            mediaVaca
                        };

                        // Qualidade
                        var ccsList = new List<decimal>();
                        var gorduraList = new List<decimal>();
                        var proteinaList = new List<decimal>();
                        foreach (var v in vacas)
                        {
                            foreach (var l in v.Lotes)
                            {
                                if (l.qualidade != null)
                                {
                                    ccsList.Add(l.qualidade.CCS);
                                    gorduraList.Add(l.qualidade.Gordura);
                                    proteinaList.Add(l.qualidade.Proteina);
                                }
                            }
                        }
                        qualidadePorPeriodo[p.Key] = new
                        {
                            mediaCCS = ccsList.Count > 0 ? ccsList.Average() : 0,
                            mediaGordura = gorduraList.Count > 0 ? gorduraList.Average() : 0,
                            mediaProteina = proteinaList.Count > 0 ? proteinaList.Average() : 0
                        };

                        // Reprodução
                        int vacasPrenhas = 0;
                        var intervalos = new List<double>();
                        int inseminacaoIA = 0;
                        int inseminacaoMonta = 0;

                        foreach (var v in vacas)
                        {
                            bool temGestante = false;
                            foreach (var pr in v.Prenhezes)
                            {
                                if (pr.dataPrenhez != null && (limite == null || pr.dataPrenhez >= limite))
                                {
                                    if (!string.IsNullOrEmpty(pr.status) && pr.status.Equals("Gestante", StringComparison.OrdinalIgnoreCase))
                                        temGestante = true;

                                    if (pr.dataPrenhez != null && pr.dataTermino != null)
                                        intervalos.Add((pr.dataTermino - pr.dataPrenhez).Value.TotalDays);
                                }

                                if (pr.dataEsperada != null && (limite == null || pr.dataEsperada >= limite))
                                {
                                    // Próximos partos podem ser calculados se quiser
                                }
                            }

                            if (temGestante) vacasPrenhas++;

                            foreach (var r in v.Reproducoes)
                            {
                                string tipo = r.tipo?.ToString()?.Trim();
                                if (!string.IsNullOrEmpty(tipo))
                                {
                                    if (tipo.IndexOf("insemin", StringComparison.OrdinalIgnoreCase) >= 0)
                                        inseminacaoIA++;
                                    else if (tipo.Equals("monta", StringComparison.OrdinalIgnoreCase))
                                        inseminacaoMonta++;
                                }
                            }
                        }

                        int taxaSucesso = totalVacasPeriodo > 0 ? (int)Math.Round((double)vacasPrenhas * 100.0 / totalVacasPeriodo) : 0;
                        double intervaloMedio = intervalos.Count > 0 ? intervalos.Average() : 0.0;

                        reproducaoPorPeriodo[p.Key] = new
                        {
                            vacasPrenhas,
                            taxaSucesso,
                            intervaloMedio,
                            inseminacaoIA,
                            inseminacaoMonta
                        };
                    }

                    return Ok(new
                    {
                        producao = producaoPorPeriodo,
                        qualidade = qualidadePorPeriodo,
                        reproducao = reproducaoPorPeriodo
                    });
                }
            }
        }
    }
}
