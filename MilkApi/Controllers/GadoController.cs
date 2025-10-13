using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MilkApi;

namespace MilkApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GadoController : Controller
    {
        private readonly string ConnectionString = config.ConnectionString;
        private readonly ILogger<GadoController> _logger;

        public GadoController(ILogger<GadoController> logger)
        {
            _logger = logger;
        }

        private string CalcularStatusProdutivo(Gado vaca, List<Prenhez> prenhezes, List<Leite> historicoLeite)
        {
            if (vaca.StatusManual) return vaca.StatusProdutivo ?? "Novilha";

            bool temLeite = historicoLeite.Any(l => l.ID_Gado == vaca.Id);

            var ultimoParto = prenhezes
                .Where(p => p.Data_Termino.HasValue)
                .OrderByDescending(p => p.Data_Termino)
                .FirstOrDefault();

            var prenhezAtiva = prenhezes.FirstOrDefault(p => !p.Data_Termino.HasValue);

            if (ultimoParto == null)
            {
                if (prenhezAtiva != null)
                    return temLeite ? "Lactante Gestante" : "Gestante";
                else
                    return temLeite ? "Lactante Vazia" : "Novilha";
            }

            var diasPosParto = (DateTime.Now - ultimoParto.Data_Termino.Value).TotalDays;

            if (prenhezAtiva != null)
            {
                if (temLeite || diasPosParto <= 305)
                    return "Lactante Gestante";

                if (prenhezAtiva.Data_Esperada.HasValue &&
                    (prenhezAtiva.Data_Esperada.Value - DateTime.Now).TotalDays <= 60)
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

        [HttpGet]
        public IEnumerable<Gado> Get([FromQuery] int? usuarioId)
        {
            var lista = new List<Gado>();

            using (var conn = new SqlConnection(ConnectionString))
            {
                string query = "SELECT * FROM Gado";
                if (usuarioId.HasValue) query += " WHERE ID_Usuario = @ID_Usuario";

                var cmd = new SqlCommand(query, conn);
                if (usuarioId.HasValue) cmd.Parameters.AddWithValue("@ID_Usuario", usuarioId.Value);

                conn.Open();
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lista.Add(new Gado
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ID_Usuario = Convert.ToInt32(reader["ID_Usuario"]),
                        Data_Nasc = Convert.ToDateTime(reader["Data_Nasc"]),
                        Raca = reader["Raca"]?.ToString(),
                        Peso = Convert.ToSingle(reader["Peso"]),
                        Sexo = reader["Sexo"]?.ToString(),
                        Brinco = Convert.ToInt32(reader["Brinco"]),
                        Observacao = reader["Observacao"]?.ToString(),
                        StatusProdutivo = reader["StatusProdutivo"]?.ToString()
                    });
                }
                reader.Close();
            }

            return lista;
        }

        [HttpGet("{id}")]
        public ActionResult GetById(int id)
        {
            using var conn = new SqlConnection(ConnectionString);
            var cmd = new SqlCommand("SELECT * FROM Gado WHERE Id = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            conn.Open();

            var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var gado = new Gado
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    ID_Usuario = Convert.ToInt32(reader["ID_Usuario"]),
                    Data_Nasc = Convert.ToDateTime(reader["Data_Nasc"]),
                    Raca = reader["Raca"]?.ToString(),
                    Peso = Convert.ToSingle(reader["Peso"]),
                    Sexo = reader["Sexo"]?.ToString(),
                    Brinco = Convert.ToInt32(reader["Brinco"]),
                    Observacao = reader["Observacao"]?.ToString(),
                    StatusProdutivo = reader["StatusProdutivo"]?.ToString()
                };
                reader.Close();
                return Ok(gado);
            }
            reader.Close();
            return NotFound();
        }

        [HttpPost]
        public ActionResult Create(Gado gado)
        {
            using var conn = new SqlConnection(ConnectionString);
            var query = @"INSERT INTO Gado 
                          (ID_Usuario, Data_Nasc, Raca, Peso, Sexo, Brinco, Observacao, StatusProdutivo)
                          VALUES (@ID_Usuario, @Data_Nasc, @Raca, @Peso, @Sexo, @Brinco, @Observacao, @StatusProdutivo)";

            var status = "Novilha";

            var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@ID_Usuario", gado.ID_Usuario);
            cmd.Parameters.AddWithValue("@Data_Nasc", gado.Data_Nasc);
            cmd.Parameters.AddWithValue("@Raca", gado.Raca ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Peso", gado.Peso);
            cmd.Parameters.AddWithValue("@Sexo", gado.Sexo ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Brinco", gado.Brinco);
            cmd.Parameters.AddWithValue("@Observacao", gado.Observacao ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@StatusProdutivo", status);

            conn.Open();
            int rows = cmd.ExecuteNonQuery();
            return rows > 0 ? Ok() : BadRequest();
        }

        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] Gado gado)
        {
            using var conn = new SqlConnection(ConnectionString);

            var prenhezes = new List<Prenhez>();
            using (var cmdPrenhez = new SqlCommand("SELECT * FROM Prenhez WHERE ID_Gado = @ID_Gado", conn))
            {
                cmdPrenhez.Parameters.AddWithValue("@ID_Gado", id);
                conn.Open();
                var reader = cmdPrenhez.ExecuteReader();
                while (reader.Read())
                {
                    prenhezes.Add(new Prenhez
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ID_Gado = id,
                        Data_Prenhez = Convert.ToDateTime(reader["Data_Prenhez"]),
                        Data_Termino = reader["Data_Termino"] as DateTime?,
                        Data_Esperada = reader["Data_Esperada"] as DateTime?,
                        Status = reader["Status"]?.ToString(),
                        ID_Usuario = Convert.ToInt32(reader["ID_Usuario"])
                    });
                }
                reader.Close();
            }

            var historicoLeite = new List<Leite>();
            using (var cmdLeite = new SqlCommand("SELECT * FROM Leite WHERE ID_Gado = @ID_Gado", conn))
            {
                cmdLeite.Parameters.AddWithValue("@ID_Gado", id);
                var readerLeite = cmdLeite.ExecuteReader();
                while (readerLeite.Read())
                {
                    historicoLeite.Add(new Leite
                    {
                        Id = Convert.ToInt32(readerLeite["Id"]),
                        ID_Gado = id,
                        Data = Convert.ToDateTime(readerLeite["Data"]),
                        Litros = Convert.ToDecimal(readerLeite["Litros"]),
                        ID_Usuario = Convert.ToInt32(readerLeite["ID_Usuario"])
                    });
                }
                readerLeite.Close();
            }

            var statusCalculado = CalcularStatusProdutivo(gado, prenhezes, historicoLeite);

            var query = @"UPDATE Gado SET 
                          ID_Usuario = @ID_Usuario, Data_Nasc = @Data_Nasc, Raca = @Raca,
                          Peso = @Peso, Sexo = @Sexo, Brinco = @Brinco, Observacao = @Observacao,
                          StatusProdutivo = @StatusProdutivo, StatusManual = @StatusManual
                          WHERE Id = @Id";

            var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@ID_Usuario", gado.ID_Usuario);
            cmd.Parameters.AddWithValue("@Data_Nasc", gado.Data_Nasc);
            cmd.Parameters.AddWithValue("@Raca", gado.Raca ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Peso", gado.Peso);
            cmd.Parameters.AddWithValue("@Sexo", gado.Sexo ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Brinco", gado.Brinco);
            cmd.Parameters.AddWithValue("@Observacao", gado.Observacao ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@StatusProdutivo", statusCalculado);
            cmd.Parameters.AddWithValue("@StatusManual", gado.StatusManual);
            cmd.Parameters.AddWithValue("@Id", id);

            int rows = cmd.ExecuteNonQuery();
            return rows > 0 ? Ok() : NotFound();
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            using var conn = new SqlConnection(ConnectionString);
            var cmd = new SqlCommand("DELETE FROM Gado WHERE Id = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            conn.Open();
            int rows = cmd.ExecuteNonQuery();
            return rows > 0 ? Ok() : NotFound();
        }

        [HttpGet("por-brinco")]
        public ActionResult GetByBrinco([FromQuery] int brinco, [FromQuery] int usuarioId)
        {
            try
            {
                using var conn = new SqlConnection(ConnectionString);
                var query = "SELECT * FROM Gado WHERE Brinco = @Brinco AND ID_Usuario = @UsuarioId";
                var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Brinco", brinco);
                cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);
                conn.Open();

                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    var gado = new Gado
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ID_Usuario = Convert.ToInt32(reader["ID_Usuario"]),
                        Data_Nasc = Convert.ToDateTime(reader["Data_Nasc"]),
                        Raca = reader["Raca"]?.ToString(),
                        Peso = Convert.ToSingle(reader["Peso"]),
                        Sexo = reader["Sexo"]?.ToString(),
                        Brinco = Convert.ToInt32(reader["Brinco"]),
                        Observacao = reader["Observacao"]?.ToString(),
                        StatusProdutivo = reader["StatusProdutivo"]?.ToString()
                    };
                    reader.Close();
                    return Ok(gado);
                }
                reader.Close();
                return NotFound("Gado não encontrado");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        [HttpGet("Resumo/{userId}")]
        public async Task<IActionResult> ObterResumoVacasDoUsuario(int userId)
        {
            using var conn = new SqlConnection(ConnectionString);
            await conn.OpenAsync();

            var sql = @"
                SELECT g.Id, g.Brinco, g.Raca,
                    (SELECT COUNT(*) FROM Leite l WHERE l.ID_Gado = g.Id AND l.ID_Usuario = @UserId) AS RegistrosLeite,
                    (SELECT COUNT(*) FROM ManejoGeral m WHERE m.ID_Gado = g.Id AND m.ID_Usuario = @UserId) AS RegistrosManejo,
                    (SELECT COUNT(*) FROM Prenhez p WHERE p.ID_Gado = g.Id AND p.ID_Usuario = @UserId) AS RegistrosPrenhez,
                    (SELECT COUNT(*) FROM Remedio r WHERE r.ID_Gado = g.Id AND r.ID_Usuario = @UserId) AS RegistrosRemedio,
                    (SELECT COUNT(*) FROM Reproducao rep WHERE rep.ID_Gado = g.Id AND rep.ID_Usuario = @UserId) AS RegistrosReproducao,
                    (SELECT COUNT(*) FROM Suplemento s WHERE s.ID_Gado = g.Id AND s.ID_Usuario = @UserId) AS RegistrosSuplemento,
                    (SELECT COUNT(*) FROM Alertas a WHERE a.ID_Gado = g.Id AND a.ID_Usuario = @UserId) AS RegistrosAlerta,
                    (SELECT COUNT(DISTINCT ll.ID_Lote) 
                     FROM LoteLeite ll
                     INNER JOIN Leite l2 ON l2.Id = ll.ID_Leite
                     WHERE l2.ID_Gado = g.Id AND ll.ID_Usuario = @UserId) AS RegistrosLote,
                    (SELECT COUNT(DISTINCT q.Id)
                     FROM Qualidade q
                     INNER JOIN LoteLeite ll2 ON ll2.ID_Lote = q.ID_Lote
                     INNER JOIN Leite l3 ON l3.Id = ll2.ID_Leite
                     WHERE l3.ID_Gado = g.Id AND q.ID_Usuario = @UserId) AS RegistrosQualidade
                FROM Gado g
                WHERE g.ID_Usuario = @UserId";

            var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);

            var reader = await cmd.ExecuteReaderAsync();
            var lista = new List<object>();

            while (await reader.ReadAsync())
            {
                lista.Add(new
                {
                    id = reader["Id"],
                    brinco = reader["Brinco"]?.ToString(),
                    raca = reader["Raca"]?.ToString(),
                    registrosLeite = Convert.ToInt32(reader["RegistrosLeite"]),
                    registrosManejo = Convert.ToInt32(reader["RegistrosManejo"]),
                    registrosPrenhez = Convert.ToInt32(reader["RegistrosPrenhez"]),
                    registrosRemedio = Convert.ToInt32(reader["RegistrosRemedio"]),
                    registrosReproducao = Convert.ToInt32(reader["RegistrosReproducao"]),
                    registrosSuplemento = Convert.ToInt32(reader["RegistrosSuplemento"]),
                    registrosAlerta = Convert.ToInt32(reader["RegistrosAlerta"]),
                    registrosLote = Convert.ToInt32(reader["RegistrosLote"]),
                    registrosQualidade = Convert.ToInt32(reader["RegistrosQualidade"])
                });
            }

            reader.Close();
            return Ok(lista);
        }

        [HttpGet("AtualizarStatusProdutivo")]
        public async Task<IActionResult> AtualizarStatusProdutivo()
        {
            using var conn = new SqlConnection(ConnectionString);
            await conn.OpenAsync();

            var vacas = new List<Gado>();
            using (var cmd = new SqlCommand("SELECT * FROM Gado", conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    vacas.Add(new Gado
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ID_Usuario = Convert.ToInt32(reader["ID_Usuario"]),
                        Data_Nasc = Convert.ToDateTime(reader["Data_Nasc"]),
                        Brinco = Convert.ToInt32(reader["Brinco"]),
                        Raca = reader["Raca"]?.ToString(),
                        Sexo = reader["Sexo"]?.ToString(),
                        Peso = Convert.ToSingle(reader["Peso"]),
                        Observacao = reader["Observacao"]?.ToString(),
                        StatusProdutivo = reader["StatusProdutivo"]?.ToString()
                    });
                }
            }

            foreach (var vaca in vacas)
            {
                var prenhezes = new List<Prenhez>();
                using (var conn2 = new SqlConnection(ConnectionString))
                {
                    await conn2.OpenAsync();

                    using (var cmdPrenhez = new SqlCommand("SELECT * FROM Prenhez WHERE ID_Gado = @ID_Gado", conn2))
                    {
                        cmdPrenhez.Parameters.AddWithValue("@ID_Gado", vaca.Id);
                        using var reader = await cmdPrenhez.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            prenhezes.Add(new Prenhez
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                ID_Gado = vaca.Id,
                                Data_Prenhez = Convert.ToDateTime(reader["Data_Prenhez"]),
                                Data_Termino = reader["Data_Termino"] as DateTime?,
                                Data_Esperada = reader["Data_Esperada"] as DateTime?,
                                Status = reader["Status"]?.ToString(),
                                ID_Usuario = Convert.ToInt32(reader["ID_Usuario"])
                            });
                        }
                    }

                    var historicoLeite = new List<Leite>();
                    using (var cmdLeite = new SqlCommand("SELECT * FROM Leite WHERE ID_Gado = @ID_GADO", conn2))
                    {
                        cmdLeite.Parameters.AddWithValue("@ID_GADO", vaca.Id);
                        using var readerLeite = await cmdLeite.ExecuteReaderAsync();
                        while (await readerLeite.ReadAsync())
                        {
                            historicoLeite.Add(new Leite
                            {
                                Id = Convert.ToInt32(readerLeite["Id"]),
                                ID_Gado = vaca.Id,
                                Data = Convert.ToDateTime(readerLeite["Data"]),
                                Litros = Convert.ToDecimal(readerLeite["Litros"]),
                                ID_Usuario = Convert.ToInt32(readerLeite["ID_Usuario"])
                            });
                        }
                    }

                    var status = CalcularStatusProdutivo(vaca, prenhezes, historicoLeite);

                    using (var cmdUpdate = new SqlCommand(
                        "UPDATE Gado SET StatusProdutivo = @StatusProdutivo WHERE Id = @Id", conn2))
                    {
                        cmdUpdate.Parameters.AddWithValue("@StatusProdutivo", status);
                        cmdUpdate.Parameters.AddWithValue("@Id", vaca.Id);
                        await cmdUpdate.ExecuteNonQueryAsync();
                    }
                }
            }

            return Ok("Status produtivo atualizado com sucesso!");
        }
    }
}
