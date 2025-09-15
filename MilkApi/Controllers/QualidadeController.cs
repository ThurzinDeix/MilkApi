using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace MilkApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class QualidadeController : Controller
    {
        private readonly string ConnectionString = config.ConnectionString;
        private readonly ILogger<QualidadeController> _logger;

        public QualidadeController(ILogger<QualidadeController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<Qualidade> Get()
        {
            List<Qualidade> lista = new List<Qualidade>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = "SELECT * FROM Qualidade";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lista.Add(new Qualidade
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ID_Lote = Convert.ToInt32(reader["ID_Lote"]),
                        CCS = Convert.ToInt32(reader["CCS"]),
                        Gordura = Convert.ToDecimal(reader["Gordura"]),
                        Proteina = Convert.ToDecimal(reader["Proteina"]),
                        ID_Usuario = Convert.ToInt32(reader["ID_Usuario"])
                    });
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
                string query = "SELECT * FROM Qualidade WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    var qualidade = new Qualidade
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ID_Lote = Convert.ToInt32(reader["ID_Lote"]),
                        CCS = Convert.ToInt32(reader["CCS"]),
                        Gordura = Convert.ToDecimal(reader["Gordura"]),
                        Proteina = Convert.ToDecimal(reader["Proteina"]),
                        ID_Usuario = Convert.ToInt32(reader["ID_Usuario"])
                    };
                    reader.Close();
                    return Ok(qualidade);
                }

                reader.Close();
                return NotFound();
            }
        }

        [HttpPost]
        public ActionResult Create(Qualidade qualidade)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"INSERT INTO Qualidade (ID_Lote, CCS, Gordura, Proteina, ID_Usuario)
                                 VALUES (@ID_Lote, @CCS, @Gordura, @Proteina, @ID_Usuario)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID_Lote", qualidade.ID_Lote);
                cmd.Parameters.AddWithValue("@CCS", qualidade.CCS);
                cmd.Parameters.AddWithValue("@Gordura", qualidade.Gordura);
                cmd.Parameters.AddWithValue("@Proteina", qualidade.Proteina);
                cmd.Parameters.AddWithValue("@ID_Usuario", qualidade.ID_Usuario);

                conn.Open();
                int rows = cmd.ExecuteNonQuery();
                if (rows > 0) return Ok();
            }

            return BadRequest();
        }

        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] Qualidade qualidade)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"UPDATE Qualidade SET 
                                    ID_Lote = @ID_Lote,
                                    CCS = @CCS,
                                    Gordura = @Gordura,
                                    Proteina = @Proteina,
                                    ID_Usuario = @ID_Usuario
                                 WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID_Lote", qualidade.ID_Lote);
                cmd.Parameters.AddWithValue("@CCS", qualidade.CCS);
                cmd.Parameters.AddWithValue("@Gordura", qualidade.Gordura);
                cmd.Parameters.AddWithValue("@Proteina", qualidade.Proteina);
                cmd.Parameters.AddWithValue("@ID_Usuario", qualidade.ID_Usuario);
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
                string query = "DELETE FROM Qualidade WHERE Id = @Id";
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
