using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace MilkApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LeiteController : Controller
    {
        private readonly string ConnectionString = config.ConnectionString;
        private readonly ILogger<LeiteController> _logger;

        public LeiteController(ILogger<LeiteController> logger)
        {
            _logger = logger;
        }

        // 🔹 Ajustado para filtrar opcionalmente pelo usuarioId
        [HttpGet]
        public IEnumerable<Leite> Get([FromQuery] int? usuarioId)
        {
            List<Leite> lista = new List<Leite>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = "SELECT * FROM Leite";

                if (usuarioId.HasValue)
                    query += " WHERE ID_Usuario = @usuarioId";

                SqlCommand cmd = new SqlCommand(query, conn);

                if (usuarioId.HasValue)
                    cmd.Parameters.AddWithValue("@usuarioId", usuarioId.Value);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Leite leite = new Leite
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ID_Gado = Convert.ToInt32(reader["ID_Gado"]),
                        Data = Convert.ToDateTime(reader["Data"]),
                        Litros = Convert.ToDecimal(reader["Litros"]),
                        ID_Usuario = Convert.ToInt32(reader["ID_Usuario"])
                    };
                    lista.Add(leite);
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
                string query = "SELECT * FROM Leite WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    Leite leite = new Leite
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ID_Gado = Convert.ToInt32(reader["ID_Gado"]),
                        Data = Convert.ToDateTime(reader["Data"]),
                        Litros = Convert.ToDecimal(reader["Litros"]),
                        ID_Usuario = Convert.ToInt32(reader["ID_Usuario"])
                    };

                    reader.Close();
                    return Ok(leite);
                }

                reader.Close();
                return NotFound();
            }
        }

        [HttpPost]
        public ActionResult Create(Leite leite)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"INSERT INTO Leite (ID_Gado, Data, Litros, ID_Usuario) 
                                 VALUES (@ID_Gado, @Data, @Litros, @ID_Usuario)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID_Gado", leite.ID_Gado);
                cmd.Parameters.AddWithValue("@Data", leite.Data);
                cmd.Parameters.AddWithValue("@Litros", leite.Litros);
                cmd.Parameters.AddWithValue("@ID_Usuario", leite.ID_Usuario);

                conn.Open();
                int rows = cmd.ExecuteNonQuery();

                if (rows > 0) return Ok();
            }

            return BadRequest();
        }

        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] Leite leite)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"UPDATE Leite SET 
                                    ID_Gado = @ID_Gado,
                                    Data = @Data,
                                    Litros = @Litros,
                                    ID_Usuario = @ID_Usuario
                                 WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID_Gado", leite.ID_Gado);
                cmd.Parameters.AddWithValue("@Data", leite.Data);
                cmd.Parameters.AddWithValue("@Litros", leite.Litros);
                cmd.Parameters.AddWithValue("@ID_Usuario", leite.ID_Usuario);
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
                string query = "DELETE FROM Leite WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                conn.Open();
                int rows = cmd.ExecuteNonQuery();

                if (rows > 0) return Ok();
            }

            return NotFound();
        }

        [HttpPost("leite-com-lote")]
        public IActionResult CriarLeiteComLote([FromBody] LeiteComLoteDTO dto)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // 1) Inserir Leite
                    string insertLeite = "INSERT INTO Leite (ID_Gado, Data, Litros, ID_Usuario) OUTPUT INSERTED.Id VALUES (@ID_Gado, @Data, @Litros, @ID_Usuario)";
                    SqlCommand cmdLeite = new SqlCommand(insertLeite, conn, transaction);
                    cmdLeite.Parameters.AddWithValue("@ID_Gado", dto.ID_Gado);
                    cmdLeite.Parameters.AddWithValue("@Data", dto.Data);
                    cmdLeite.Parameters.AddWithValue("@Litros", dto.Litros);
                    cmdLeite.Parameters.AddWithValue("@ID_Usuario", dto.ID_Usuario);

                    int leiteId = (int)cmdLeite.ExecuteScalar();

                    // 2) Inserir Lote vinculado ao Leite
                    string insertLote = "INSERT INTO Lote (ID_Leite, Num, ID_Usuario) VALUES (@ID_Leite, @Num, @ID_Usuario)";
                    SqlCommand cmdLote = new SqlCommand(insertLote, conn, transaction);
                    cmdLote.Parameters.AddWithValue("@ID_Leite", leiteId);
                    cmdLote.Parameters.AddWithValue("@Num", dto.Num);
                    cmdLote.Parameters.AddWithValue("@ID_Usuario", dto.ID_Usuario);

                    cmdLote.ExecuteNonQuery();

                    transaction.Commit();

                    return Ok(new { leiteId = leiteId, loteNum = dto.Num });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return StatusCode(500, $"Erro ao criar Leite e Lote: {ex.Message}");
                }
            }
        }

        [HttpGet("por-usuario")]
        public IEnumerable<Leite> GetPorUsuario(int usuarioId)
        {
            List<Leite> lista = new List<Leite>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = "SELECT * FROM Leite WHERE ID_Usuario = @UsuarioId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Leite leite = new Leite
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ID_Gado = Convert.ToInt32(reader["ID_Gado"]),
                        Data = Convert.ToDateTime(reader["Data"]),
                        Litros = Convert.ToDecimal(reader["Litros"]),
                        ID_Usuario = Convert.ToInt32(reader["ID_Usuario"])
                    };
                    lista.Add(leite);
                }

                reader.Close();
            }

            return lista;
        }


    }
}
