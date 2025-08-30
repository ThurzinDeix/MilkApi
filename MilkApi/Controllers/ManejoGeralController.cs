using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace MilkApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ManejoGeralController : Controller
    {
        private const string ConnectionString = "Server=milkdatabase.cp64yi8w2sr2.us-east-2.rds.amazonaws.com;Database=BancoTccGado;User Id=Arthur;Password=Arthur-1234;TrustServerCertificate=True;";
        private readonly ILogger<ManejoGeralController> _logger;

        public ManejoGeralController(ILogger<ManejoGeralController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<ManejoGeral> Get()
        {
            List<ManejoGeral> lista = new List<ManejoGeral>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = "SELECT * FROM ManejoGeral";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    ManejoGeral manejo = new ManejoGeral
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ID_Gado = Convert.ToInt32(reader["ID_Gado"]),
                        Tipo_Manejo = reader["Tipo_Manejo"]?.ToString(),
                        Data_Manejo = Convert.ToDateTime(reader["Data_Manejo"]),
                        Observacoes = reader["Observacoes"]?.ToString(),
                        ID_Usuario = Convert.ToInt32(reader["ID_Usuario"])
                    };
                    lista.Add(manejo);
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
                string query = "SELECT * FROM ManejoGeral WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    ManejoGeral manejo = new ManejoGeral
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ID_Gado = Convert.ToInt32(reader["ID_Gado"]),
                        Tipo_Manejo = reader["Tipo_Manejo"]?.ToString(),
                        Data_Manejo = Convert.ToDateTime(reader["Data_Manejo"]),
                        Observacoes = reader["Observacoes"]?.ToString(),
                        ID_Usuario = Convert.ToInt32(reader["ID_Usuario"])
                    };

                    reader.Close();
                    return Ok(manejo);
                }

                reader.Close();
                return NotFound();
            }
        }

        [HttpPost]
        public ActionResult Create(ManejoGeral manejo)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"INSERT INTO ManejoGeral 
                                (ID_Gado, Tipo_Manejo, Data_Manejo, Observacoes, ID_Usuario) 
                                VALUES (@ID_Gado, @Tipo_Manejo, @Data_Manejo, @Observacoes, @ID_Usuario)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID_Gado", manejo.ID_Gado);
                cmd.Parameters.AddWithValue("@Tipo_Manejo", (object?)manejo.Tipo_Manejo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Data_Manejo", manejo.Data_Manejo);
                cmd.Parameters.AddWithValue("@Observacoes", (object?)manejo.Observacoes ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ID_Usuario", manejo.ID_Usuario);

                conn.Open();
                int rows = cmd.ExecuteNonQuery();

                if (rows > 0) return Ok();
            }

            return BadRequest();
        }

        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] ManejoGeral manejo)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"UPDATE ManejoGeral SET 
                                    ID_Gado = @ID_Gado,
                                    Tipo_Manejo = @Tipo_Manejo,
                                    Data_Manejo = @Data_Manejo,
                                    Observacoes = @Observacoes,
                                    ID_Usuario = @ID_Usuario
                                 WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID_Gado", manejo.ID_Gado);
                cmd.Parameters.AddWithValue("@Tipo_Manejo", (object?)manejo.Tipo_Manejo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Data_Manejo", manejo.Data_Manejo);
                cmd.Parameters.AddWithValue("@Observacoes", (object?)manejo.Observacoes ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ID_Usuario", manejo.ID_Usuario);
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
                string query = "DELETE FROM ManejoGeral WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                conn.Open();
                int rows = cmd.ExecuteNonQuery();

                if (rows > 0) return Ok();
            }

            return NotFound();
        }

        [HttpGet("por-usuario")]
        public IEnumerable<ManejoGeral> GetByUsuario(int usuarioId)
        {
            List<ManejoGeral> lista = new List<ManejoGeral>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = "SELECT * FROM ManejoGeral WHERE ID_Usuario = @ID_Usuario";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID_Usuario", usuarioId);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    ManejoGeral manejo = new ManejoGeral
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ID_Gado = Convert.ToInt32(reader["ID_Gado"]),
                        Tipo_Manejo = reader["Tipo_Manejo"]?.ToString(),
                        Data_Manejo = Convert.ToDateTime(reader["Data_Manejo"]),
                        Observacoes = reader["Observacoes"]?.ToString(),
                        ID_Usuario = Convert.ToInt32(reader["ID_Usuario"])
                    };
                    lista.Add(manejo);
                }
                reader.Close();
            }

            return lista;
        }

    }

}
