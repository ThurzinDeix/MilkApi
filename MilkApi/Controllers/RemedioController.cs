using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MilkApi;

namespace MilkApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RemedioController : Controller
    {
        private readonly string ConnectionString = config.ConnectionString;
        private readonly ILogger<RemedioController> _logger;

        public RemedioController(ILogger<RemedioController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<Remedio> Get()
        {
            List<Remedio> lista = new List<Remedio>();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = "SELECT * FROM Remedio";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lista.Add(new Remedio
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ID_Gado = Convert.ToInt32(reader["ID_Gado"]),
                        Nome = reader["Nome"]?.ToString(),
                        Date = Convert.ToDateTime(reader["Date"]),
                        Doses = Convert.ToInt32(reader["Doses"]),
                        intervalo = Convert.ToInt32(reader["intervalo"]),
                        via = reader["via"]?.ToString(),
                        ID_Usuario = Convert.ToInt32(reader["ID_Usuario"])
                    });
                }
                reader.Close();
            }
            return lista;
        }

        [HttpGet("{id}")]
        public ActionResult GetById(int id)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = "SELECT * FROM Remedio WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    Remedio r = new Remedio
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ID_Gado = Convert.ToInt32(reader["ID_Gado"]),
                        Nome = reader["Nome"]?.ToString(),
                        Date = Convert.ToDateTime(reader["Date"]),
                        Doses = Convert.ToInt32(reader["Doses"]),
                        intervalo = Convert.ToInt32(reader["intervalo"]),
                        via = reader["via"]?.ToString(),
                        ID_Usuario = Convert.ToInt32(reader["ID_Usuario"])
                    };
                    reader.Close();
                    return Ok(r);
                }
                reader.Close();
                return NotFound();
            }
        }

        [HttpPost]
        public ActionResult Create(Remedio r)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"INSERT INTO Remedio (ID_Gado, Nome, Date, Doses, intervalo, via, ID_Usuario)
                                 VALUES (@ID_Gado, @Nome, @Date, @Doses, @intervalo, @via, @ID_Usuario)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID_Gado", r.ID_Gado);
                cmd.Parameters.AddWithValue("@Nome", r.Nome ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Date", r.Date);
                cmd.Parameters.AddWithValue("@Doses", r.Doses);
                cmd.Parameters.AddWithValue("@intervalo", r.intervalo);
                cmd.Parameters.AddWithValue("@via", r.via ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ID_Usuario", r.ID_Usuario);
                conn.Open();
                int rows = cmd.ExecuteNonQuery();
                if (rows > 0) return Ok();
            }
            return BadRequest();
        }

        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] Remedio r)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"UPDATE Remedio SET 
                                    ID_Gado = @ID_Gado,
                                    Nome = @Nome,
                                    Date = @Date,
                                    Doses = @Doses,
                                    intervalo = @intervalo,
                                    via = @via,
                                    ID_Usuario = @ID_Usuario
                                 WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID_Gado", r.ID_Gado);
                cmd.Parameters.AddWithValue("@Nome", r.Nome ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Date", r.Date);
                cmd.Parameters.AddWithValue("@Doses", r.Doses);
                cmd.Parameters.AddWithValue("@intervalo", r.intervalo);
                cmd.Parameters.AddWithValue("@via", r.via ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ID_Usuario", r.ID_Usuario);
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                int rows = cmd.ExecuteNonQuery();
                if (rows > 0) return Ok();
            }
            return NotFound();
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = "DELETE FROM Remedio WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                int rows = cmd.ExecuteNonQuery();
                if (rows > 0) return Ok();
            }
            return NotFound();
        }

        [HttpGet("por-usuario")]
        public IEnumerable<Remedio> GetByUsuario(int usuarioId)
        {
            List<Remedio> lista = new List<Remedio>();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = "SELECT * FROM Remedio WHERE ID_Usuario = @ID_Usuario";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID_Usuario", usuarioId);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lista.Add(new Remedio
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ID_Gado = Convert.ToInt32(reader["ID_Gado"]),
                        Nome = reader["Nome"]?.ToString(),
                        Date = Convert.ToDateTime(reader["Date"]),
                        Doses = Convert.ToInt32(reader["Doses"]),
                        intervalo = Convert.ToInt32(reader["intervalo"]),
                        via = reader["via"]?.ToString(),
                        ID_Usuario = Convert.ToInt32(reader["ID_Usuario"])
                    });
                }
                reader.Close();
            }
            return lista;
        }

        [HttpGet("tratamentos/{idGado}")]
        public IActionResult GetTratamentosPorGado(int idGado)
        {
            try
            {
                List<object> tratamentos = new List<object>();
                using SqlConnection conn = new SqlConnection(ConnectionString);
                conn.Open();

                string sql = @"
                    SELECT r.Id, r.Nome, r.Date, r.intervalo, r.Doses, r.via,
                           COUNT(da.Id) AS DosesAplicadas
                    FROM Remedio r
                    LEFT JOIN DosesAplicadas da ON r.Id = da.ID_Remedio
                    WHERE r.ID_Gado = @idGado
                    GROUP BY r.Id, r.Nome, r.Date, r.intervalo, r.Doses, r.via
                    ORDER BY r.Date";

                using SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@idGado", idGado);

                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int dosesAplicadas = (int)reader["DosesAplicadas"];
                    int totalDoses = (int)reader["Doses"];
                    DateTime inicio = (DateTime)reader["Date"];
                    int intervalo = (int)reader["intervalo"];

                    DateTime? proximaDose = dosesAplicadas < totalDoses
                        ? inicio.AddHours(intervalo * dosesAplicadas)
                        : (DateTime?)null;

                    tratamentos.Add(new
                    {
                        IdRemedio = reader["Id"],
                        Nome = reader["Nome"],
                        Via = reader["via"],
                        TotalDoses = totalDoses,
                        DosesAplicadas = dosesAplicadas,
                        ProximaDose = proximaDose,
                        Status = dosesAplicadas >= totalDoses ? "Finalizado" : "Ativo"
                    });
                }

                return Ok(tratamentos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        [HttpPost("aplicar/{idRemedio}/{idGado}/{idUsuario}")]
        public IActionResult AplicarDose(int idRemedio, int idGado, int idUsuario)
        {
            try
            {
                using SqlConnection conn = new SqlConnection(ConnectionString);
                conn.Open();

                string insertSql = @"
                    INSERT INTO DosesAplicadas (ID_Remedio, ID_Gado, Data_Aplicacao, ID_Usuario)
                    VALUES (@idRemedio, @idGado, GETDATE(), @idUsuario)";

                using SqlCommand insertCmd = new SqlCommand(insertSql, conn);
                insertCmd.Parameters.AddWithValue("@idRemedio", idRemedio);
                insertCmd.Parameters.AddWithValue("@idGado", idGado);
                insertCmd.Parameters.AddWithValue("@idUsuario", idUsuario);
                insertCmd.ExecuteNonQuery();

                return Ok(new { mensagem = "Dose aplicada com sucesso!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        [HttpPatch("encerrar/{idRemedio}")]
        public IActionResult EncerrarTratamento(int idRemedio)
        {
            try
            {
                using SqlConnection conn = new SqlConnection(ConnectionString);
                conn.Open();

                string sql = @"
                    DECLARE @total INT = (SELECT Doses FROM Remedio WHERE Id = @idRemedio)
                    DECLARE @aplicadas INT = (SELECT COUNT(*) FROM DosesAplicadas WHERE ID_Remedio = @idRemedio)
                    DECLARE @faltando INT = @total - @aplicadas

                    IF @faltando > 0
                    BEGIN
                        DECLARE @i INT = 0
                        WHILE @i < @faltando
                        BEGIN
                            INSERT INTO DosesAplicadas (ID_Remedio, ID_Gado, Data_Aplicacao, ID_Usuario)
                            SELECT Id, ID_Gado, GETDATE(), ID_Usuario FROM Remedio WHERE Id = @idRemedio
                            SET @i = @i + 1
                        END
                    END";

                using SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@idRemedio", idRemedio);
                cmd.ExecuteNonQuery();

                return Ok(new { mensagem = "Tratamento encerrado com sucesso!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }
    }
}
