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
        private readonly string ConnectionString = config.ConnectionString;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ILogger<DashboardController> logger)
        {
            _logger = logger;
        }

        private class Vaca
        {
            public int Id { get; set; }
            public string Brinco { get; set; }
            public string Raca { get; set; }
            public DateTime DataEntrada { get; set; }
            public List<Leite> Leites { get; } = new List<Leite>();
            public List<Prenhez> Prenhezes { get; } = new List<Prenhez>();
            public List<Reproducao> Reproducoes { get; } = new List<Reproducao>();
            public List<Lote> Lotes { get; } = new List<Lote>();
        }

        private class StatusPoint
        {
            public string Label { get; set; }
            public int LactanteGestante { get; set; }
            public int Gestante { get; set; }
            public int LactanteVazia { get; set; }
            public int Seca { get; set; }
            public int Novilha { get; set; }
            public int VaziaNaoLactante { get; set; }
            public int TotalVacas { get; set; }
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
            public DateTime? LeiteDate { get; set; }
        }

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

        private string CalcularStatusProdutivoComData(Vaca vaca, DateTime referencia)
        {
            bool temLeite = vaca.Leites.Any(l => l.Data.HasValue && l.Data.Value <= referencia);

            var ultimaPrenhezFinalizada = vaca.Prenhezes
                .Where(p => p.DataTermino.HasValue && p.DataTermino.Value <= referencia)
                .OrderByDescending(p => p.DataTermino)
                .FirstOrDefault();

            var prenhezAtiva = vaca.Prenhezes.FirstOrDefault(p =>
                (!p.DataTermino.HasValue || p.DataTermino.Value > referencia) &&
                p.Status != null &&
                p.Status.Equals("Prenha", StringComparison.OrdinalIgnoreCase)
            );

            if (ultimaPrenhezFinalizada == null)
            {
                if (prenhezAtiva != null)
                    return temLeite ? "Lactante Gestante" : "Gestante";
                else
                    return temLeite ? "Lactante Vazia" : "Novilha";
            }

            var diasPosParto = (referencia - ultimaPrenhezFinalizada.DataTermino.Value).TotalDays;
            var statusUltimaPrenhez = ultimaPrenhezFinalizada.Status?.ToLower();

            if (statusUltimaPrenhez == "pariu")
            {
                if (prenhezAtiva != null)
                {
                    if (temLeite || diasPosParto <= 305)
                        return "Lactante Gestante";
                    else
                        return "Gestante";
                }
                else
                {
                    if (temLeite || diasPosParto <= 305)
                        return "Lactante Vazia";
                    else
                        return "Vazia Não Lactante";
                }
            }
            else if (statusUltimaPrenhez == "aborto")
            {
                if (prenhezAtiva != null)
                    return "Gestante";
                else
                    return temLeite ? "Lactante Vazia" : "Vazia Não Lactante";
            }
            else
            {
                if (prenhezAtiva != null)
                {
                    if (temLeite || diasPosParto <= 305)
                        return "Lactante Gestante";

                    if (prenhezAtiva.DataEsperada.HasValue &&
                        (prenhezAtiva.DataEsperada.Value - referencia).TotalDays <= 60)
                        return "Seca";

                    return "Gestante";
                }
                else
                {
                    if (temLeite || diasPosParto <= 305)
                        return "Lactante Vazia";
                    else
                        return "Vazia Não Lactante";
                }
            }
        }

        [HttpGet("{usuarioId}")]
        public async Task<IActionResult> GetDashboard(int usuarioId)
        {
            var vacasDict = new Dictionary<int, Vaca>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                await conn.OpenAsync();

                var query = @"
                    SELECT g.Id, g.Brinco, g.Raca, g.Data_Entrada,
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
                            int vacaId = Convert.ToInt32(reader["Id"]);
                            if (!vacasDict.ContainsKey(vacaId))
                            {
                                var v = new Vaca
                                {
                                    Id = vacaId,
                                    Brinco = reader["Brinco"].ToString(),
                                    Raca = reader["Raca"].ToString(),
                                    DataEntrada = Convert.ToDateTime(reader["Data_Entrada"])
                                };
                                vacasDict[vacaId] = v;
                            }

                            var vaca = vacasDict[vacaId];

                            if (reader["LeiteId"] != DBNull.Value)
                            {
                                var leite = new Leite
                                {
                                    Id = Convert.ToInt32(reader["LeiteId"]),
                                    Litros = reader["Litros"] != DBNull.Value ? Convert.ToDecimal(reader["Litros"]) : 0m,
                                    Data = reader["LeiteData"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["LeiteData"]) : null
                                };
                                if (!vaca.Leites.Any(x => x.Id == leite.Id))
                                    vaca.Leites.Add(leite);
                            }

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

                            if (reader["ReproducaoId"] != DBNull.Value)
                            {
                                var r = new Reproducao
                                {
                                    Tipo = reader["ReproTipo"] != DBNull.Value ? reader["ReproTipo"].ToString() : null
                                };
                                if (!vaca.Reproducoes.Any(x => x.Tipo == r.Tipo))
                                    vaca.Reproducoes.Add(r);
                            }

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
                        }
                    }
                }
            }

            var vacas = vacasDict.Values.ToList();
            DateTime hoje = DateTime.Now.Date;

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

            var producaoPorPeriodo = new Dictionary<string, List<ProductionPoint>>();
            var qualidadePorPeriodo = new Dictionary<string, List<QualityPoint>>();
            var reproducaoPorPeriodo = new Dictionary<string, List<ReproPoint>>();
            var statusPorPeriodo = new Dictionary<string, List<StatusPoint>>();

            foreach (var p in periodos)
            {
                string periodoNome = p.Key;
                DateTime inicio = p.Value ?? earliestLeite;

                string tipoEixo;
                switch (periodoNome)
                {
                    case "1Semana": tipoEixo = "dia"; break;
                    case "1Mes": tipoEixo = "semana"; break;
                    case "6Meses":
                    case "1Ano": tipoEixo = "mes"; break;
                    case "2Anos": tipoEixo = "bimestre"; break;
                    case "3Anos": tipoEixo = "trimestre"; break;
                    case "4Anos": tipoEixo = "quadrimestre"; break;
                    case "5Anos": tipoEixo = "semestre"; break;
                    default: tipoEixo = "mes"; break;
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

                    int vacasPrenhas = vacas.Count(v => v.Prenhezes.Any(pr =>
                        pr.DataPrenhez.HasValue &&
                        pr.DataEsperada.HasValue &&
                        pr.DataPrenhez.Value < proximo &&
                        pr.DataEsperada.Value > cursor
                    ));

                    var intervalosDias = vacas
                        .SelectMany(v => v.Prenhezes)
                        .Where(pr => pr.DataPrenhez.HasValue
                                     && pr.DataTermino.HasValue
                                     && pr.DataPrenhez.Value >= cursor
                                     && pr.DataPrenhez.Value < proximo)
                        .Select(pr => (pr.DataTermino.Value - pr.DataPrenhez.Value).TotalDays)
                        .ToList();

                    double intervaloMedio = intervalosDias.Any() ? intervalosDias.Average() : 0.0;

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

                    var pontosStatus = new List<StatusPoint>();

                    DateTime cursorStatus = inicio.Date;
                    while (cursorStatus <= hoje)
                    {
                        DateTime proximoStatus;
                        if (tipoEixo == "dia") proximoStatus = cursorStatus.AddDays(1);
                        else if (tipoEixo == "semana") proximoStatus = cursorStatus.AddDays(7);
                        else if (tipoEixo == "mes") proximoStatus = cursorStatus.AddMonths(1);
                        else if (tipoEixo == "bimestre") proximoStatus = cursorStatus.AddMonths(2);
                        else if (tipoEixo == "trimestre") proximoStatus = cursorStatus.AddMonths(3);
                        else if (tipoEixo == "quadrimestre") proximoStatus = cursorStatus.AddMonths(4);
                        else if (tipoEixo == "semestre") proximoStatus = cursorStatus.AddMonths(6);
                        else proximoStatus = cursorStatus.AddMonths(1);

                        string label1;
                        if (tipoEixo == "dia") label1 = cursorStatus.ToString("dd/MM");
                        else if (tipoEixo == "semana")
                        {
                            int semanaIndex = (int)Math.Floor((cursorStatus - inicio).TotalDays / 7.0) + 1;
                            label1 = $"Sem {semanaIndex}";
                        }
                        else label1 = cursorStatus.ToString("MM/yyyy");

                        var statusPoint = new StatusPoint { Label = label1 };

                        var vacasAtivas = vacas.Where(v => v.DataEntrada <= cursorStatus).ToList();
                        foreach (var v in vacasAtivas)
                        {
                            var status = CalcularStatusProdutivoComData(v, cursorStatus);
                            switch (status.Replace(" ", ""))
                            {
                                case "LactanteGestante": statusPoint.LactanteGestante++; break;
                                case "Gestante": statusPoint.Gestante++; break;
                                case "LactanteVazia": statusPoint.LactanteVazia++; break;
                                case "Seca": statusPoint.Seca++; break;
                                case "Novilha": statusPoint.Novilha++; break;
                                case "VaziaNaoLactante": statusPoint.VaziaNaoLactante++; break;
                            }
                        }

                        statusPoint.TotalVacas = vacasAtivas.Count;

                        pontosStatus.Add(statusPoint);
                        cursorStatus = proximoStatus;
                    }

                    statusPorPeriodo[periodoNome] = pontosStatus;

                    cursor = proximo;
                }

                producaoPorPeriodo[periodoNome] = pontosProducao;
                qualidadePorPeriodo[periodoNome] = pontosQualidade;
                reproducaoPorPeriodo[periodoNome] = pontosReproducao;
            }

            return Ok(new
            {
                producao = producaoPorPeriodo,
                qualidade = qualidadePorPeriodo,
                reproducao = reproducaoPorPeriodo,
                status = statusPorPeriodo
            });
        }

        [HttpGet("PorGado/{gadoId}")]
        public async Task<IActionResult> GetDashboardPorGado(int gadoId)
        {
            var vaca = new Vaca();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                await conn.OpenAsync();

                var query = @"
            SELECT g.Id, g.Brinco, g.Raca, g.Data_Entrada,
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
            WHERE g.Id = @GadoId";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@GadoId", gadoId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            if (vaca.Id == 0)
                            {
                                vaca.Id = Convert.ToInt32(reader["Id"]);
                                vaca.Brinco = reader["Brinco"].ToString();
                                vaca.Raca = reader["Raca"].ToString();
                                vaca.DataEntrada = Convert.ToDateTime(reader["Data_Entrada"]);
                            }

                            if (reader["LeiteId"] != DBNull.Value)
                            {
                                var leite = new Leite
                                {
                                    Id = Convert.ToInt32(reader["LeiteId"]),
                                    Litros = reader["Litros"] != DBNull.Value ? Convert.ToDecimal(reader["Litros"]) : 0m,
                                    Data = reader["LeiteData"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["LeiteData"]) : null
                                };
                                if (!vaca.Leites.Any(x => x.Id == leite.Id))
                                    vaca.Leites.Add(leite);
                            }

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

                            if (reader["ReproducaoId"] != DBNull.Value)
                            {
                                var r = new Reproducao
                                {
                                    Tipo = reader["ReproTipo"] != DBNull.Value ? reader["ReproTipo"].ToString() : null
                                };
                                if (!vaca.Reproducoes.Any(x => x.Tipo == r.Tipo))
                                    vaca.Reproducoes.Add(r);
                            }

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
                        }
                    }
                }
            }

            if (vaca.Id == 0)
                return NotFound("Gado não encontrado.");

            DateTime hoje = DateTime.Now.Date;
            DateTime earliestLeite = vaca.Leites.Where(l => l.Data.HasValue).Select(l => l.Data.Value).DefaultIfEmpty(hoje.AddYears(-2)).Min();

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
                { "Total", earliestLeite }
            };


            var producaoPorPeriodo = new Dictionary<string, List<ProductionPoint>>();
            var qualidadePorPeriodo = new Dictionary<string, List<QualityPoint>>();
            var reproducaoPorPeriodo = new Dictionary<string, List<ReproPoint>>();
            var statusPorPeriodo = new Dictionary<string, List<StatusPoint>>();

            foreach (var p in periodos)
            {
                string periodoNome = p.Key;
                DateTime inicio = p.Value ?? earliestLeite;

                string tipoEixo = periodoNome switch
                {
                    "1Semana" => "dia",
                    "1Mes" => "semana",
                    "6Meses" => "mes",
                    "1Ano" => "mes",
                    "2Anos" => "bimestre",
                    "3Anos" => "trimestre",
                    "4Anos" => "quadrimestre",
                    "5Anos" => "semestre",
                    "Total" => "mes",
                    _ => "mes"
                };

                var pontosProducao = new List<ProductionPoint>();
                var pontosQualidade = new List<QualityPoint>();
                var pontosReproducao = new List<ReproPoint>();
                var pontosStatus = new List<StatusPoint>();

                DateTime cursor = inicio.Date;
                while (cursor <= hoje)
                {
                    DateTime proximo = tipoEixo == "dia" ? cursor.AddDays(1) :
                                       tipoEixo == "semana" ? cursor.AddDays(7) :
                                       cursor.AddMonths(1);

                    string label = tipoEixo == "dia" ? cursor.ToString("dd/MM") :
                                   tipoEixo == "semana" ? $"Sem {(int)Math.Floor((cursor - inicio).TotalDays / 7.0) + 1}" :
                                   cursor.ToString("MM/yyyy");

                    var leitesIntervalo = vaca.Leites.Where(l => l.Data.HasValue && l.Data.Value >= cursor && l.Data.Value < proximo).ToList();
                    decimal litrosPeriodo = leitesIntervalo.Sum(l => l.Litros);

                    pontosProducao.Add(new ProductionPoint
                    {
                        Label = label,
                        TotalLeite = litrosPeriodo,
                        MediaVaca = litrosPeriodo // só 1 vaca, então média = total
                    });

                    var qualList = vaca.Lotes.Where(l => l.Qualidade != null && l.LeiteDate.HasValue && l.LeiteDate.Value >= cursor && l.LeiteDate.Value < proximo)
                        .Select(l => l.Qualidade).ToList();

                    decimal mediaCCS = qualList.Any() ? qualList.Average(q => q.CCS) : 0m;
                    decimal mediaGordura = qualList.Any() ? qualList.Average(q => q.Gordura) : 0m;
                    decimal mediaProteina = qualList.Any() ? qualList.Average(q => q.Proteina) : 0m;

                    pontosQualidade.Add(new QualityPoint
                    {
                        Label = label,
                        MediaCCS = mediaCCS,
                        MediaGordura = mediaGordura,
                        MediaProteina = mediaProteina
                    });

                    int vacasPrenhas = vaca.Prenhezes.Count(pr =>
                        pr.DataPrenhez.HasValue && pr.DataEsperada.HasValue &&
                        pr.DataPrenhez.Value < proximo && pr.DataEsperada.Value > cursor);

                    double intervaloMedio = vaca.Prenhezes
                        .Where(pr => pr.DataPrenhez.HasValue && pr.DataTermino.HasValue && pr.DataPrenhez.Value >= cursor && pr.DataPrenhez.Value < proximo)
                        .Select(pr => (pr.DataTermino.Value - pr.DataPrenhez.Value).TotalDays)
                        .DefaultIfEmpty(0)
                        .Average();

                    int inseminacaoIA = vaca.Reproducoes.Count(r => r.Tipo?.ToLower().Contains("insemin") == true);
                    int inseminacaoMonta = vaca.Reproducoes.Count(r => r.Tipo?.ToLower() == "monta");

                    pontosReproducao.Add(new ReproPoint
                    {
                        Label = label,
                        VacasPrenhas = vacasPrenhas,
                        IntervaloMedio = Math.Round(intervaloMedio, 2),
                        InseminacaoIA = inseminacaoIA,
                        InseminacaoMonta = inseminacaoMonta
                    });

                    var statusPoint = new StatusPoint { Label = label };
                    string status = CalcularStatusProdutivoComData(vaca, cursor);
                    switch (status.Replace(" ", ""))
                    {
                        case "LactanteGestante": statusPoint.LactanteGestante++; break;
                        case "Gestante": statusPoint.Gestante++; break;
                        case "LactanteVazia": statusPoint.LactanteVazia++; break;
                        case "Seca": statusPoint.Seca++; break;
                        case "Novilha": statusPoint.Novilha++; break;
                        case "VaziaNaoLactante": statusPoint.VaziaNaoLactante++; break;
                    }
                    statusPoint.TotalVacas = 1;
                    pontosStatus.Add(statusPoint);

                    cursor = proximo;
                }

                producaoPorPeriodo[periodoNome] = pontosProducao;
                qualidadePorPeriodo[periodoNome] = pontosQualidade;
                reproducaoPorPeriodo[periodoNome] = pontosReproducao;
                statusPorPeriodo[periodoNome] = pontosStatus;
            }

            return Ok(new
            {
                Gado = new { vaca.Id, vaca.Brinco, vaca.Raca },
                producao = producaoPorPeriodo,
                qualidade = qualidadePorPeriodo,
                reproducao = reproducaoPorPeriodo,
                status = statusPorPeriodo
            });
        }

    }
}
