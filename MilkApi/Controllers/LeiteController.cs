using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace MilkApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LeiteController : Controller
    {
        private const string ConnectionString = "Server=milkdatabase.cp64yi8w2sr2.us-east-2.rds.amazonaws.com;Database=BancoTccGado;User Id=Arthur;Password=Arthur-1234;TrustServerCertificate=True;";
        private readonly ILogger<LeiteController> _logger;

        public LeiteController(ILogger<LeiteController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<Leite> Get()
        {
            List<Leite> lista = new List<Leite>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = "SELECT * FROM Leite";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Leite leite = new Leite
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ID_Gado = Convert.ToInt32(reader["ID_Gado"]),
                        Data = Convert.ToDateTime(reader["Data"]),
                        Litros = Convert.ToDecimal(reader["Litros"])
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
                        Litros = Convert.ToDecimal(reader["Litros"])
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
                string query = @"INSERT INTO Leite 
                                (ID_Gado, Data, Litros) 
                                VALUES (@ID_Gado, @Data, @Litros)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID_Gado", leite.ID_Gado);
                cmd.Parameters.AddWithValue("@Data", leite.Data);
                cmd.Parameters.AddWithValue("@Litros", leite.Litros);

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
                                    Litros = @Litros
                                 WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID_Gado", leite.ID_Gado);
                cmd.Parameters.AddWithValue("@Data", leite.Data);
                cmd.Parameters.AddWithValue("@Litros", leite.Litros);
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
            string connectionString = ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // 1) Inserir Leite
                    string insertLeite = "INSERT INTO Leite (ID_Gado, Data, Litros) OUTPUT INSERTED.Id VALUES (@ID_Gado, @Data, @Litros)";
                    SqlCommand cmdLeite = new SqlCommand(insertLeite, conn, transaction);
                    cmdLeite.Parameters.AddWithValue("@ID_Gado", dto.ID_Gado);
                    cmdLeite.Parameters.AddWithValue("@Data", dto.Data);
                    cmdLeite.Parameters.AddWithValue("@Litros", dto.Litros);

                    int leiteId = (int)cmdLeite.ExecuteScalar();

                    // 2) Inserir Lote vinculado ao Leite
                    string insertLote = "INSERT INTO Lote (ID_Leite, Num) VALUES (@ID_Leite, @Num)";
                    SqlCommand cmdLote = new SqlCommand(insertLote, conn, transaction);
                    cmdLote.Parameters.AddWithValue("@ID_Leite", leiteId);
                    cmdLote.Parameters.AddWithValue("@Num", dto.Num);

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

    }
}
