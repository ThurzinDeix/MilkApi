using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MilkApi;

namespace MilkApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PrenhezController : Controller
    {
        private const string ConnectionString = "Server=milkdatabase.cp64yi8w2sr2.us-east-2.rds.amazonaws.com;Database=BancoTccGado;User Id=Arthur;Password=Arthur-1234;TrustServerCertificate=True;";
        private readonly ILogger<PrenhezController> _logger;

        public PrenhezController(ILogger<PrenhezController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<Prenhez> Get()
        {
            List<Prenhez> lista = new List<Prenhez>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = "SELECT * FROM Prenhez";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Prenhez p = new Prenhez
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ID_Gado = Convert.ToInt32(reader["ID_Gado"]),
                        Data_Prenhez = Convert.ToDateTime(reader["Data_Prenhez"]),
                        Data_Termino = reader["Data_Termino"] == DBNull.Value ? null : Convert.ToDateTime(reader["Data_Termino"]),
                        Data_Esperada = reader["Data_Esperada"] == DBNull.Value ? null : Convert.ToDateTime(reader["Data_Esperada"]),
                        ID_Usuario = Convert.ToInt32(reader["ID_Usuario"]),
                        Status = reader["Status"]?.ToString()
                    };
                    lista.Add(p);
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
                string query = "SELECT * FROM Prenhez WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    Prenhez p = new Prenhez
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ID_Gado = Convert.ToInt32(reader["ID_Gado"]),
                        Data_Prenhez = Convert.ToDateTime(reader["Data_Prenhez"]),
                        Data_Termino = reader["Data_Termino"] == DBNull.Value ? null : Convert.ToDateTime(reader["Data_Termino"]),
                        Data_Esperada = reader["Data_Esperada"] == DBNull.Value ? null : Convert.ToDateTime(reader["Data_Esperada"]),
                        ID_Usuario = Convert.ToInt32(reader["ID_Usuario"]),
                        Status = reader["Status"]?.ToString()
                    };

                    reader.Close();
                    return Ok(p);
                }

                reader.Close();
                return NotFound();
            }
        }

        [HttpPost]
        public ActionResult Create(Prenhez p)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"INSERT INTO Prenhez (ID_Gado, Data_Prenhez, Data_Termino, Data_Esperada, ID_Usuario, Status)
                                 VALUES (@ID_Gado, @Data_Prenhez, @Data_Termino, @Data_Esperada, @ID_Usuario, @Status)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID_Gado", p.ID_Gado);
                cmd.Parameters.AddWithValue("@Data_Prenhez", p.Data_Prenhez);
                cmd.Parameters.AddWithValue("@Data_Termino", (object?)p.Data_Termino ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Data_Esperada", (object?)p.Data_Esperada ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ID_Usuario", p.ID_Usuario);
                cmd.Parameters.AddWithValue("@Status", p.Status ?? (object)DBNull.Value);

                conn.Open();
                int rows = cmd.ExecuteNonQuery();

                if (rows > 0) return Ok();
            }

            return BadRequest();
        }

        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] Prenhez p)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"UPDATE Prenhez SET 
                                    ID_Gado = @ID_Gado,
                                    Data_Prenhez = @Data_Prenhez,
                                    Data_Termino = @Data_Termino,
                                    Data_Esperada = @Data_Esperada,
                                    ID_Usuario = @ID_Usuario,
                                    Status = @Status
                                 WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID_Gado", p.ID_Gado);
                cmd.Parameters.AddWithValue("@Data_Prenhez", p.Data_Prenhez);
                cmd.Parameters.AddWithValue("@Data_Termino", (object?)p.Data_Termino ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Data_Esperada", (object?)p.Data_Esperada ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ID_Usuario", p.ID_Usuario);
                cmd.Parameters.AddWithValue("@Status", p.Status ?? (object)DBNull.Value);
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
                string query = "DELETE FROM Prenhez WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                conn.Open();
                int rows = cmd.ExecuteNonQuery();

                if (rows > 0) return Ok();
            }

            return NotFound();
        }

        // GET /prenhez/por-usuario?usuarioId=123
        [HttpGet("por-usuario")]
        public IEnumerable<Prenhez> GetByUsuario(int usuarioId)
        {
            List<Prenhez> lista = new List<Prenhez>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = "SELECT * FROM Prenhez WHERE ID_Usuario = @ID_Usuario";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID_Usuario", usuarioId);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lista.Add(new Prenhez
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ID_Gado = Convert.ToInt32(reader["ID_Gado"]),
                        Data_Prenhez = Convert.ToDateTime(reader["Data_Prenhez"]),
                        Data_Termino = reader["Data_Termino"] == DBNull.Value ? null : Convert.ToDateTime(reader["Data_Termino"]),
                        Data_Esperada = reader["Data_Esperada"] == DBNull.Value ? null : Convert.ToDateTime(reader["Data_Esperada"]),
                        ID_Usuario = Convert.ToInt32(reader["ID_Usuario"]),
                        Status = reader["Status"]?.ToString()
                    });
                }
                reader.Close();
            }

            return lista;
        }
    }
}
