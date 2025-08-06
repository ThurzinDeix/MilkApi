using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace MilkApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LeiteController : Controller
    {
        private const string ConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=BancoTccGado;Integrated Security=True;";
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
                        CCS = Convert.ToInt32(reader["CCS"]),
                        Gordura = Convert.ToDecimal(reader["Gordura"]),
                        Proteina = Convert.ToDecimal(reader["Proteina"]),
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
                        CCS = Convert.ToInt32(reader["CCS"]),
                        Gordura = Convert.ToDecimal(reader["Gordura"]),
                        Proteina = Convert.ToDecimal(reader["Proteina"]),
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
                                (ID_Gado, Data, CCS, Gordura, Proteina, Litros) 
                                VALUES (@ID_Gado, @Data, @CCS, @Gordura, @Proteina, @Litros)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID_Gado", leite.ID_Gado);
                cmd.Parameters.AddWithValue("@Data", leite.Data);
                cmd.Parameters.AddWithValue("@CCS", leite.CCS);
                cmd.Parameters.AddWithValue("@Gordura", leite.Gordura);
                cmd.Parameters.AddWithValue("@Proteina", leite.Proteina);
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
                                    CCS = @CCS,
                                    Gordura = @Gordura,
                                    Proteina = @Proteina,
                                    Litros = @Litros
                                 WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID_Gado", leite.ID_Gado);
                cmd.Parameters.AddWithValue("@Data", leite.Data);
                cmd.Parameters.AddWithValue("@CCS", leite.CCS);
                cmd.Parameters.AddWithValue("@Gordura", leite.Gordura);
                cmd.Parameters.AddWithValue("@Proteina", leite.Proteina);
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
    }
}
