using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MilkApi;

namespace MilkApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RemedioController : Controller
    {
        private const string ConnectionString = "Server=milkdatabase.cp64yi8w2sr2.us-east-2.rds.amazonaws.com;Database=BancoTccGado;User Id=Arthur;Password=Arthur-1234;TrustServerCertificate=True;";
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
                    lista.Add(r);
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
    }

   }
