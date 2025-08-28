using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace MilkApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AlertasController : Controller
    {
        private const string ConnectionString = "Server=milkdatabase.cp64yi8w2sr2.us-east-2.rds.amazonaws.com;Database=BancoTccGado;User Id=Arthur;Password=Arthur-1234;TrustServerCertificate=True;";
        private readonly ILogger<AlertasController> _logger;

        public AlertasController(ILogger<AlertasController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<Alertas> Get()
        {
            List<Alertas> lista = new List<Alertas>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = "SELECT * FROM Alertas";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Alertas alerta = new Alertas
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ID_Gado = Convert.ToInt32(reader["ID_Gado"]),
                        Data_Prevista = Convert.ToDateTime(reader["Data_Prevista"]),
                        Status = reader["Status"]?.ToString(),
                        ID_Usuario = Convert.ToInt32(reader["ID_Usuario"])
                    };
                    lista.Add(alerta);
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
                string query = "SELECT * FROM Alertas WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    Alertas alerta = new Alertas
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ID_Gado = Convert.ToInt32(reader["ID_Gado"]),
                        Data_Prevista = Convert.ToDateTime(reader["Data_Prevista"]),
                        Status = reader["Status"]?.ToString(),
                        ID_Usuario = Convert.ToInt32(reader["ID_Usuario"])
                    };

                    reader.Close();
                    return Ok(alerta);
                }

                reader.Close();
                return NotFound();
            }
        }

        [HttpPost]
        public ActionResult Create(Alertas alerta)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"INSERT INTO Alertas (ID_Gado, Data_Prevista, Status, ID_Usuario) 
                                 VALUES (@ID_Gado, @Data_Prevista, @Status, @ID_Usuario)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID_Gado", alerta.ID_Gado);
                cmd.Parameters.AddWithValue("@Data_Prevista", alerta.Data_Prevista);
                cmd.Parameters.AddWithValue("@Status", alerta.Status ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ID_Usuario", alerta.ID_Usuario);

                conn.Open();
                int rows = cmd.ExecuteNonQuery();

                if (rows > 0) return Ok();
            }

            return BadRequest();
        }

        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] Alertas alerta)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"UPDATE Alertas SET 
                                    ID_Gado = @ID_Gado,
                                    Data_Prevista = @Data_Prevista,
                                    Status = @Status,
                                    ID_Usuario = @ID_Usuario
                                 WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID_Gado", alerta.ID_Gado);
                cmd.Parameters.AddWithValue("@Data_Prevista", alerta.Data_Prevista);
                cmd.Parameters.AddWithValue("@Status", alerta.Status ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ID_Usuario", alerta.ID_Usuario);
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
                string query = "DELETE FROM Alertas WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                conn.Open();
                int rows = cmd.ExecuteNonQuery();

                if (rows > 0) return Ok();
            }

            return NotFound();
        }
    }
}
