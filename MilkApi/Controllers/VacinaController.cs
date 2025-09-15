using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace MilkApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VacinaController : Controller
    {
        private readonly string ConnectionString = config.ConnectionString;
        private readonly ILogger<VacinaController> _logger;

        public VacinaController(ILogger<VacinaController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<Vacina> Get()
        {
            List<Vacina> lista = new List<Vacina>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = "SELECT * FROM Vacina";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new Vacina
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ID_TipoVacina = Convert.ToInt32(reader["ID_TipoVacina"]),
                        Lote = reader["Lote"]?.ToString(),
                        DataValidade = Convert.ToDateTime(reader["DataValidade"]),
                        Fabricante = reader["Fabricante"]?.ToString(),
                        Observacoes = reader["Observacoes"]?.ToString(),
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
                string query = "SELECT * FROM Vacina WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    var vacina = new Vacina
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ID_TipoVacina = Convert.ToInt32(reader["ID_TipoVacina"]),
                        Lote = reader["Lote"]?.ToString(),
                        DataValidade = Convert.ToDateTime(reader["DataValidade"]),
                        Fabricante = reader["Fabricante"]?.ToString(),
                        Observacoes = reader["Observacoes"]?.ToString(),
                        ID_Usuario = Convert.ToInt32(reader["ID_Usuario"])
                    };
                    reader.Close();
                    return Ok(vacina);
                }

                reader.Close();
                return NotFound();
            }
        }

        [HttpPost]
        public ActionResult Create(Vacina v)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"INSERT INTO Vacina (ID_TipoVacina, Lote, DataValidade, Fabricante, Observacoes, ID_Usuario)
                                 VALUES (@ID_TipoVacina, @Lote, @DataValidade, @Fabricante, @Observacoes, @ID_Usuario)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID_TipoVacina", v.ID_TipoVacina);
                cmd.Parameters.AddWithValue("@Lote", v.Lote ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@DataValidade", v.DataValidade);
                cmd.Parameters.AddWithValue("@Fabricante", v.Fabricante ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Observacoes", v.Observacoes ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ID_Usuario", v.ID_Usuario);

                conn.Open();
                int rows = cmd.ExecuteNonQuery();

                if (rows > 0) return Ok();
            }

            return BadRequest();
        }

        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] Vacina v)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"UPDATE Vacina SET 
                                    ID_TipoVacina = @ID_TipoVacina,
                                    Lote = @Lote,
                                    DataValidade = @DataValidade,
                                    Fabricante = @Fabricante,
                                    Observacoes = @Observacoes,
                                    ID_Usuario = @ID_Usuario
                                 WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID_TipoVacina", v.ID_TipoVacina);
                cmd.Parameters.AddWithValue("@Lote", v.Lote ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@DataValidade", v.DataValidade);
                cmd.Parameters.AddWithValue("@Fabricante", v.Fabricante ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Observacoes", v.Observacoes ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ID_Usuario", v.ID_Usuario);
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
                string query = "DELETE FROM Vacina WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                conn.Open();
                int rows = cmd.ExecuteNonQuery();

                if (rows > 0) return Ok();
            }

            return NotFound();
        }
    }

    public class Vacina
    {
        public int Id { get; set; }
        public int ID_TipoVacina { get; set; }
        public string? Lote { get; set; }
        public DateTime DataValidade { get; set; }
        public string? Fabricante { get; set; }
        public string? Observacoes { get; set; }
        public int ID_Usuario { get; set; }
    }
}
