using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace MilkApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TelefoneController : Controller
    {
        private const string ConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=BancoTccGado;Integrated Security=True;";
        private readonly ILogger<TelefoneController> _logger;

        public TelefoneController(ILogger<TelefoneController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<Telefone> Get()
        {
            List<Telefone> lista = new List<Telefone>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = "SELECT * FROM Telefone";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Telefone telefone = new Telefone
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ID_Usuario = Convert.ToInt32(reader["ID_Usuario"]),
                        Numero = reader["Numero"]?.ToString(),
                        DDD = reader["DDD"]?.ToString(),
                        Tipo = reader["Tipo"]?.ToString()
                    };
                    lista.Add(telefone);
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
                string query = "SELECT * FROM Telefone WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    Telefone telefone = new Telefone
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ID_Usuario = Convert.ToInt32(reader["ID_Usuario"]),
                        Numero = reader["Numero"]?.ToString(),
                        DDD = reader["DDD"]?.ToString(),
                        Tipo = reader["Tipo"]?.ToString()
                    };

                    reader.Close();
                    return Ok(telefone);
                }

                reader.Close();
                return NotFound();
            }
        }

        [HttpPost]
        public ActionResult Create(Telefone telefone)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"INSERT INTO Telefone 
                                (ID_Usuario, Numero, DDD, Tipo) 
                                VALUES (@ID_Usuario, @Numero, @DDD, @Tipo)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID_Usuario", telefone.ID_Usuario);
                cmd.Parameters.AddWithValue("@Numero", (object?)telefone.Numero ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DDD", (object?)telefone.DDD ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Tipo", (object?)telefone.Tipo ?? DBNull.Value);

                conn.Open();
                int rows = cmd.ExecuteNonQuery();

                if (rows > 0) return Ok();
            }

            return BadRequest();
        }

        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] Telefone telefone)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"UPDATE Telefone SET 
                                    ID_Usuario = @ID_Usuario,
                                    Numero = @Numero,
                                    DDD = @DDD,
                                    Tipo = @Tipo
                                 WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID_Usuario", telefone.ID_Usuario);
                cmd.Parameters.AddWithValue("@Numero", (object?)telefone.Numero ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DDD", (object?)telefone.DDD ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Tipo", (object?)telefone.Tipo ?? DBNull.Value);
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
                string query = "DELETE FROM Telefone WHERE Id = @Id";
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
