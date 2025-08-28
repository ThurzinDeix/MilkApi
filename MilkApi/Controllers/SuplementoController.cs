using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MilkApi;

namespace MilkApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SuplementoController : Controller
    {
        private const string ConnectionString = "Server=milkdatabase.cp64yi8w2sr2.us-east-2.rds.amazonaws.com;Database=BancoTccGado;User Id=Arthur;Password=Arthur-1234;TrustServerCertificate=True;";
        private readonly ILogger<SuplementoController> _logger;

        public SuplementoController(ILogger<SuplementoController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<Suplemento> Get()
        {
            List<Suplemento> lista = new List<Suplemento>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = "SELECT * FROM Suplemento";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Suplemento sup = new Suplemento
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ID_Gado = Convert.ToInt32(reader["ID_Gado"]),
                        Tipo = reader["Tipo"]?.ToString(),
                        Nome = reader["Nome"]?.ToString(),
                        Date = Convert.ToDateTime(reader["Date"]),
                        intervalo = Convert.ToInt32(reader["intervalo"]),
                        ID_Usuario = Convert.ToInt32(reader["ID_Usuario"])
                    };
                    lista.Add(sup);
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
                string query = "SELECT * FROM Suplemento WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    Suplemento sup = new Suplemento
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ID_Gado = Convert.ToInt32(reader["ID_Gado"]),
                        Tipo = reader["Tipo"]?.ToString(),
                        Nome = reader["Nome"]?.ToString(),
                        Date = Convert.ToDateTime(reader["Date"]),
                        intervalo = Convert.ToInt32(reader["intervalo"]),
                        ID_Usuario = Convert.ToInt32(reader["ID_Usuario"])
                    };

                    reader.Close();
                    return Ok(sup);
                }

                reader.Close();
                return NotFound();
            }
        }

        [HttpPost]
        public ActionResult Create(Suplemento sup)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"INSERT INTO Suplemento (ID_Gado, Tipo, Nome, Date, intervalo, ID_Usuario)
                                 VALUES (@ID_Gado, @Tipo, @Nome, @Date, @intervalo, @ID_Usuario)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID_Gado", sup.ID_Gado);
                cmd.Parameters.AddWithValue("@Tipo", sup.Tipo ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Nome", sup.Nome ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Date", sup.Date);
                cmd.Parameters.AddWithValue("@intervalo", sup.intervalo);
                cmd.Parameters.AddWithValue("@ID_Usuario", sup.ID_Usuario);

                conn.Open();
                int rows = cmd.ExecuteNonQuery();

                if (rows > 0) return Ok();
            }

            return BadRequest();
        }

        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] Suplemento sup)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"UPDATE Suplemento SET 
                                    ID_Gado = @ID_Gado,
                                    Tipo = @Tipo,
                                    Nome = @Nome,
                                    Date = @Date,
                                    intervalo = @intervalo,
                                    ID_Usuario = @ID_Usuario
                                 WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID_Gado", sup.ID_Gado);
                cmd.Parameters.AddWithValue("@Tipo", sup.Tipo ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Nome", sup.Nome ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Date", sup.Date);
                cmd.Parameters.AddWithValue("@intervalo", sup.intervalo);
                cmd.Parameters.AddWithValue("@ID_Usuario", sup.ID_Usuario);
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
                string query = "DELETE FROM Suplemento WHERE Id = @Id";
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
