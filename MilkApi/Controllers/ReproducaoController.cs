using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace MilkApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReproducaoController : Controller
    {
        private const string ConnectionString = "Server=milkdatabase.cp64yi8w2sr2.us-east-2.rds.amazonaws.com;Database=BancoTccGado;User Id=Arthur;Password=Arthur-1234;TrustServerCertificate=True;";
        private readonly ILogger<ReproducaoController> _logger;

        public ReproducaoController(ILogger<ReproducaoController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<Reproducao> Get()
        {
            List<Reproducao> lista = new List<Reproducao>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = "SELECT * FROM Reproducao";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Reproducao repro = new Reproducao
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ID_Gado = Convert.ToInt32(reader["ID_Gado"]),
                        Tipo = reader["Tipo"]?.ToString(),
                        Data = Convert.ToDateTime(reader["Data"]),
                        Observacao = reader["Observacao"]?.ToString(),
                        ID_Usuario = Convert.ToInt32(reader["ID_Usuario"])
                    };
                    lista.Add(repro);
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
                string query = "SELECT * FROM Reproducao WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    Reproducao repro = new Reproducao
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ID_Gado = Convert.ToInt32(reader["ID_Gado"]),
                        Tipo = reader["Tipo"]?.ToString(),
                        Data = Convert.ToDateTime(reader["Data"]),
                        Observacao = reader["Observacao"]?.ToString(),
                        ID_Usuario = Convert.ToInt32(reader["ID_Usuario"])
                    };

                    reader.Close();
                    return Ok(repro);
                }

                reader.Close();
                return NotFound();
            }
        }

        [HttpPost]
        public ActionResult Create(Reproducao repro)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"INSERT INTO Reproducao 
                                (ID_Gado, Tipo, Data, Observacao, ID_Usuario) 
                                VALUES (@ID_Gado, @Tipo, @Data, @Observacao, @ID_Usuario)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID_Gado", repro.ID_Gado);
                cmd.Parameters.AddWithValue("@Tipo", (object?)repro.Tipo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Data", repro.Data);
                cmd.Parameters.AddWithValue("@Observacao", (object?)repro.Observacao ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ID_Usuario", repro.ID_Usuario);

                conn.Open();
                int rows = cmd.ExecuteNonQuery();

                if (rows > 0) return Ok();
            }

            return BadRequest();
        }

        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] Reproducao repro)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"UPDATE Reproducao SET 
                                    ID_Gado = @ID_Gado,
                                    Tipo = @Tipo,
                                    Data = @Data,
                                    Observacao = @Observacao,
                                    ID_Usuario = @ID_Usuario
                                 WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID_Gado", repro.ID_Gado);
                cmd.Parameters.AddWithValue("@Tipo", (object?)repro.Tipo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Data", repro.Data);
                cmd.Parameters.AddWithValue("@Observacao", (object?)repro.Observacao ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ID_Usuario", repro.ID_Usuario);
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
                string query = "DELETE FROM Reproducao WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                conn.Open();
                int rows = cmd.ExecuteNonQuery();

                if (rows > 0) return Ok();
            }

            return NotFound();
        }
    }

    // Modelo usado pelo controller
}
