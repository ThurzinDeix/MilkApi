using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MilkApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DashboardController : Controller
    {
        // mantenha sua forma de obter ConnectionString (ex: config.ConnectionString)
        private readonly string ConnectionString = config.ConnectionString;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ILogger<DashboardController> logger)
        {
            _logger = logger;
        }

        // modelos internos (fortemente tipados) para evitar problemas com dynamic/LINQ
        private class Vaca
        {
            public int Id { get; set; }
            public string Brinco { get; set; }
            public string Raca { get; set; }
            public List<Leite> Leites { get; } = new List<Leite>();
            public List<Prenhez> Prenhezes { get; } = new List<Prenhez>();
            public List<Reproducao> Reproducoes { get; } = new List<Reproducao>();
            public List<Lote> Lotes { get; } = new List<Lote>();
        }

        private class Leite
        {
            public int Id { get; set; }
            public decimal Litros { get; set; }
            public DateTime? Data { get; set; }
        }

        private class Prenhez
        {
            public DateTime? DataPrenhez { get; set; }
            public DateTime? DataTermino { get; set; }
            public DateTime? DataEsperada { get; set; }
            public string Status { get; set; }
        }

        private class Reproducao
        {
            public string Tipo { get; set; }
        }

        private class Qualidade
        {
            public decimal CCS { get; set; }
            public decimal Gordura { get; set; }
            public decimal Proteina { get; set; }
        }

        private class Lote
        {
            public int Id { get; set; }
            public string Num { get; set; }
            public Qualidade Qualidade { get; set; }
            public DateTime? LeiteDate { get; set; } // para poder relacionar qualidade ao tempo do leite
        }

        // DTOs de ponto de série para retorno
        private class ProductionPoint
        {
            public string Label { get; set; }
            public decimal TotalLeite { get; set; }
            public decimal MediaVaca { get; set; }
        }

        private class QualityPoint
        {
            public string Label { get; set; }
            public decimal MediaCCS { get; set; }
            public decimal MediaGordura { get; set; }
            public decimal MediaProteina { get; set; }
        }

        private class ReproPoint
        {
            public string Label { get; set; }
            public int VacasPrenhas { get; set; }
            public double IntervaloMedio { get; set; }
            public int InseminacaoIA { get; set; }
            public int InseminacaoMonta { get; set; }
        }

        [HttpGet("{usuarioId}")]
        public async Task<IActionResult> GetDashboard(int usuarioId)
        {
            var vacasDict = new Dictionary<int, Vaca>();

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
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int vacaId = reader["Id"] != DBNull.Value ? Convert.ToInt32(reader["Id"]) : 0;
                            if (!vacasDict.ContainsKey(vacaId))
                            {
                                var v = new Vaca
                                {
                                    Id = vacaId,
                                    Brinco = reader["Brinco"] != DBNull.Value ? reader["Brinco"].ToString() : null,
                                    Raca = reader["Raca"] != DBNull.Value ? reader["Raca"].ToString() : null
                                };
                                vacasDict[vacaId] = v;
                            }

                            var vaca = vacasDict[vacaId];

                            // Leite
                            if (reader["LeiteId"] != DBNull.Value)
                            {
                                var leite = new Leite
                                {
                                    Id = Convert.ToInt32(reader["LeiteId"]),
                                    Litros = reader["Litros"] != DBNull.Value ? Convert.ToDecimal(reader["Litros"]) : 0m,
                                    Data = reader["LeiteData"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["LeiteData"]) : null
                                };
                                // evitar duplicatas simples (mesmo Id)
                                if (!vaca.Leites.Any(x => x.Id == leite.Id))
                                    vaca.Leites.Add(leite);
                            }

                            // Prenhez
                            if (reader["PrenhezId"] != DBNull.Value)
                            {
                                var pr = new Prenhez
                                {
                                    DataPrenhez = reader["Data_Prenhez"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["Data_Prenhez"]) : null,
                                    DataTermino = reader["Data_Termino"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["Data_Termino"]) : null,
                                    DataEsperada = reader["Data_Esperada"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["Data_Esperada"]) : null,
                                    Status = reader["PrenhezStatus"] != DBNull.Value ? reader["PrenhezStatus"].ToString() : null
                                };
                                vaca.Prenhezes.Add(pr);
                            }

                            // Reprodução
                            if (reader["ReproducaoId"] != DBNull.Value)
                            {
                                var r = new Reproducao
                                {
                                    Tipo = reader["ReproTipo"] != DBNull.Value ? reader["ReproTipo"].ToString() : null
                                };
                                // evitar duplicatas simples
                                if (!vaca.Reproducoes.Any(x => x.Tipo == r.Tipo))
                                    vaca.Reproducoes.Add(r);
                            }

                            // Lote + Qualidade (associamos a data do leite para poder filtrar por intervalo)
                            if (reader["LoteId"] != DBNull.Value)
                            {
                                int loteId = Convert.ToInt32(reader["LoteId"]);
                                var lote = new Lote
                                {
                                    Id = loteId,
                                    Num = reader["LoteNum"] != DBNull.Value ? reader["LoteNum"].ToString() : null,
                                    LeiteDate = reader["LeiteData"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["LeiteData"]) : null,
                                    Qualidade = reader["QualidadeId"] != DBNull.Value ? new Qualidade
                                    {
                                        CCS = reader["CCS"] != DBNull.Value ? Convert.ToDecimal(reader["CCS"]) : 0m,
                                        Gordura = reader["Gordura"] != DBNull.Value ? Convert.ToDecimal(reader["Gordura"]) : 0m,
                                        Proteina = reader["Proteina"] != DBNull.Value ? Convert.ToDecimal(reader["Proteina"]) : 0m
                                    } : null
                                };

                                if (!vaca.Lotes.Any(x => x.Id == lote.Id))
                                    vaca.Lotes.Add(lote);
                            }
                        } // while reader
                    } // using reader
                } // using cmd
            } // using conn

            var vacas = vacasDict.Values.ToList();
            DateTime hoje = DateTime.Now.Date;

            // se não houver leite, definimos um início padrão (5 anos atrás) para 'Total'
            DateTime earliestLeite = vacas
                .SelectMany(v => v.Leites)
                .Where(l => l.Data.HasValue)
                .Select(l => l.Data.Value)
                .DefaultIfEmpty(hoje.AddYears(-5))
                .Min();

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
                { "Total", null }
            };

            periodos["5Anos"] = hoje.AddYears(-5);

            var producaoPorPeriodo = new Dictionary<string, List<ProductionPoint>>();
            var qualidadePorPeriodo = new Dictionary<string, List<QualityPoint>>();
            var reproducaoPorPeriodo = new Dictionary<string, List<ReproPoint>>();

            foreach (var p in periodos)
            {
                string periodoNome = p.Key;
                DateTime inicio = p.Value ?? earliestLeite; 

                string tipoEixo;
                switch (periodoNome)
                {
                    case "1Semana":
                        tipoEixo = "dia";
                        break;
                    case "1Mes":
                        tipoEixo = "semana";
                        break;
                    case "6Meses":
                    case "1Ano":
                        tipoEixo = "mes";
                        break;
                    case "2Anos":
                        tipoEixo = "bimestre";
                        break;
                    case "3Anos":
                        tipoEixo = "trimestre";
                        break;
                    case "4Anos":
                        tipoEixo = "quadrimestre";
                        break;
                    case "5Anos":
                        tipoEixo = "semestre";
                        break;
                    default:
                        tipoEixo = "mes";
                        break;
                }

                var pontosProducao = new List<ProductionPoint>();
                var pontosQualidade = new List<QualityPoint>();
                var pontosReproducao = new List<ReproPoint>();

                DateTime cursor = inicio.Date;
                while (cursor <= hoje)
                {
                    DateTime proximo;
                    if (tipoEixo == "dia") proximo = cursor.AddDays(1);
                    else if (tipoEixo == "semana") proximo = cursor.AddDays(7);
                    else if (tipoEixo == "mes") proximo = cursor.AddMonths(1);
                    else if (tipoEixo == "bimestre") proximo = cursor.AddMonths(2);
                    else if (tipoEixo == "trimestre") proximo = cursor.AddMonths(3);
                    else if (tipoEixo == "quadrimestre") proximo = cursor.AddMonths(4);
                    else if (tipoEixo == "semestre") proximo = cursor.AddMonths(6);
                    else proximo = cursor.AddMonths(1);

                    // Produção no intervalo
                    var leitesIntervalo = vacas
                        .SelectMany(v => v.Leites)
                        .Where(l => l.Data.HasValue && l.Data.Value >= cursor && l.Data.Value < proximo)
                        .ToList();

                    decimal litrosPeriodo = leitesIntervalo.Sum(l => l.Litros);
                    decimal mediaVacaPeriodo = vacas.Count > 0 ? litrosPeriodo / vacas.Count : 0m;

                    string label;
                    if (tipoEixo == "dia") label = cursor.ToString("dd/MM");
                    else if (tipoEixo == "semana")
                    {
                        int semanaIndex = (int)Math.Floor((cursor - inicio).TotalDays / 7.0) + 1;
                        label = $"Sem {semanaIndex}";
                    }
                    else label = cursor.ToString("MM/yyyy");

                    pontosProducao.Add(new ProductionPoint
                    {
                        Label = label,
                        TotalLeite = decimal.Round(litrosPeriodo, 2),
                        MediaVaca = decimal.Round(mediaVacaPeriodo, 2)
                    });

                    // Qualidade no intervalo (filtrada pela data do leite associada ao lote)
                    var qualList = vacas
                        .SelectMany(v => v.Lotes)
                        .Where(l => l.Qualidade != null && l.LeiteDate.HasValue && l.LeiteDate.Value >= cursor && l.LeiteDate.Value < proximo)
                        .Select(l => l.Qualidade)
                        .ToList();

                    decimal mediaCCS = qualList.Any() ? qualList.Average(q => q.CCS) : 0m;
                    decimal mediaGordura = qualList.Any() ? qualList.Average(q => q.Gordura) : 0m;
                    decimal mediaProteina = qualList.Any() ? qualList.Average(q => q.Proteina) : 0m;

                    pontosQualidade.Add(new QualityPoint
                    {
                        Label = label,
                        MediaCCS = decimal.Round(mediaCCS, 2),
                        MediaGordura = decimal.Round(mediaGordura, 2),
                        MediaProteina = decimal.Round(mediaProteina, 2)
                    });

                    // Reprodução no intervalo (baseado em datas de prenhez)
                    int vacasPrenhas = vacas.Count(v => v.Prenhezes.Any(pr =>
                        pr.DataPrenhez.HasValue &&
                        pr.DataPrenhez.Value >= cursor &&
                        pr.DataPrenhez.Value < proximo &&
                        pr.Status?.Equals("Gestante", StringComparison.OrdinalIgnoreCase) == true
                    ));

                    // intervalo médio de prenhez registrados dentro do intervalo
                    var intervalosDias = vacas
                        .SelectMany(v => v.Prenhezes)
                        .Where(pr => pr.DataPrenhez.HasValue
                                     && pr.DataTermino.HasValue
                                     && pr.DataPrenhez.Value >= cursor
                                     && pr.DataPrenhez.Value < proximo)
                        .Select(pr => (pr.DataTermino.Value - pr.DataPrenhez.Value).TotalDays)
                        .ToList();

                    double intervaloMedio = intervalosDias.Any() ? intervalosDias.Average() : 0.0;

                    // Contagem de tipos de reprodução: aproximamos contando as reproduções das vacas que tiveram prenhez no intervalo
                    int inseminacaoIA = 0;
                    int inseminacaoMonta = 0;

                    foreach (var v in vacas)
                    {
                        bool temPrenhezNoIntervalo = v.Prenhezes.Any(pr =>
                            pr.DataPrenhez.HasValue &&
                            pr.DataPrenhez.Value >= cursor &&
                            pr.DataPrenhez.Value < proximo);

                        if (!temPrenhezNoIntervalo) continue;

                        foreach (var r in v.Reproducoes)
                        {
                            var tipo = r.Tipo?.ToLower() ?? "";
                            if (tipo.Contains("insemin")) inseminacaoIA++;
                            else if (tipo.Trim() == "monta") inseminacaoMonta++;
                        }
                    }

                    pontosReproducao.Add(new ReproPoint
                    {
                        Label = label,
                        VacasPrenhas = vacasPrenhas,
                        IntervaloMedio = Math.Round(intervaloMedio, 2),
                        InseminacaoIA = inseminacaoIA,
                        InseminacaoMonta = inseminacaoMonta
                    });

                    cursor = proximo;
                } // enquanto cursor <= hoje

                producaoPorPeriodo[periodoNome] = pontosProducao;
                qualidadePorPeriodo[periodoNome] = pontosQualidade;
                reproducaoPorPeriodo[periodoNome] = pontosReproducao;
            } // foreach periodo

            return Ok(new
            {
                producao = producaoPorPeriodo,
                qualidade = qualidadePorPeriodo,
                reproducao = reproducaoPorPeriodo
            });
        }
    }
}
