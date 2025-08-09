using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MilkApi;

namespace MilkApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GadoController : Controller
    {
        private const string ConnectionString = "Server=milkdatabase.cp64yi8w2sr2.us-east-2.rds.amazonaws.com;Database=BancoTccGado;User Id=Arthur;Password=Arthur-1234;TrustServerCertificate=True;";
        private readonly ILogger<GadoController> _logger;

        public GadoController(ILogger<GadoController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<Gado> Get([FromQuery] int? usuarioId)
        {
            List<Gado> lista = new List<Gado>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = "SELECT * FROM Gado";

                if (usuarioId.HasValue)
                {
                    query += " WHERE ID_Usuario = @ID_Usuario";
                }

                SqlCommand cmd = new SqlCommand(query, conn);

                if (usuarioId.HasValue)
                {
                    cmd.Parameters.AddWithValue("@ID_Usuario", usuarioId.Value);
                }

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Gado gado = new Gado
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ID_Usuario = Convert.ToInt32(reader["ID_Usuario"]),
                        Data_Nasc = Convert.ToDateTime(reader["Data_Nasc"]),
                        Raca = reader["Raca"]?.ToString(),
                        Peso = Convert.ToSingle(reader["Peso"]),
                        Sexo = reader["Sexo"]?.ToString(),
                        Brinco = Convert.ToInt32(reader["Brinco"]),
                        Observacao = reader["Observacao"]?.ToString()
                    };
                    lista.Add(gado);
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
                string query = "SELECT * FROM Gado WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    Gado gado = new Gado
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ID_Usuario = Convert.ToInt32(reader["ID_Usuario"]),
                        Data_Nasc = Convert.ToDateTime(reader["Data_Nasc"]),
                        Raca = reader["Raca"]?.ToString(),
                        Peso = Convert.ToSingle(reader["Peso"]),
                        Sexo = reader["Sexo"]?.ToString(),
                        Brinco = Convert.ToInt32(reader["Brinco"]),
                        Observacao = reader["Observacao"]?.ToString()
                    };

                    reader.Close();
                    return Ok(gado);
                }

                reader.Close();
                return NotFound();
            }
        }

        [HttpPost]
        public ActionResult Create(Gado gado)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"INSERT INTO Gado (ID_Usuario, Data_Nasc, Raca, Peso, Sexo, Brinco, Observacao)
                                 VALUES (@ID_Usuario, @Data_Nasc, @Raca, @Peso, @Sexo, @Brinco, @Observacao)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID_Usuario", gado.ID_Usuario);
                cmd.Parameters.AddWithValue("@Data_Nasc", gado.Data_Nasc);
                cmd.Parameters.AddWithValue("@Raca", gado.Raca ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Peso", gado.Peso);
                cmd.Parameters.AddWithValue("@Sexo", gado.Sexo ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Brinco", gado.Brinco);
                cmd.Parameters.AddWithValue("@Observacao", gado.Observacao ?? (object)DBNull.Value);

                conn.Open();
                int rows = cmd.ExecuteNonQuery();

                if (rows > 0) return Ok();
            }

            return BadRequest();
        }

        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] Gado gado)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"UPDATE Gado SET 
                                    ID_Usuario = @ID_Usuario,
                                    Data_Nasc = @Data_Nasc,
                                    Raca = @Raca,
                                    Peso = @Peso,
                                    Sexo = @Sexo,
                                    Brinco = @Brinco,
                                    Observacao = @Observacao
                                 WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID_Usuario", gado.ID_Usuario);
                cmd.Parameters.AddWithValue("@Data_Nasc", gado.Data_Nasc);
                cmd.Parameters.AddWithValue("@Raca", gado.Raca ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Peso", gado.Peso);
                cmd.Parameters.AddWithValue("@Sexo", gado.Sexo ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Brinco", gado.Brinco);
                cmd.Parameters.AddWithValue("@Observacao", gado.Observacao ?? (object)DBNull.Value);
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
                string query = "DELETE FROM Gado WHERE Id = @Id";
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
