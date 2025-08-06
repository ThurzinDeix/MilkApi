using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace MilkApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VacinaController : Controller
    {
        private const string ConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=BancoTccGado;Integrated Security=True;";
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
                    Vacina v = new Vacina
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ID_TipoVacina = Convert.ToInt32(reader["ID_TipoVacina"]),
                        Lote = reader["Lote"]?.ToString(),
                        DataValidade = Convert.ToDateTime(reader["DataValidade"]),
                        Fabricante = reader["Fabricante"]?.ToString(),
                        Observacoes = reader["Observacoes"]?.ToString()
                    };
                    lista.Add(v);
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
                    Vacina v = new Vacina
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ID_TipoVacina = Convert.ToInt32(reader["ID_TipoVacina"]),
                        Lote = reader["Lote"]?.ToString(),
                        DataValidade = Convert.ToDateTime(reader["DataValidade"]),
                        Fabricante = reader["Fabricante"]?.ToString(),
                        Observacoes = reader["Observacoes"]?.ToString()
                    };
                    reader.Close();
                    return Ok(v);
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
                string query = @"INSERT INTO Vacina (ID_TipoVacina, Lote, DataValidade, Fabricante, Observacoes)
                                 VALUES (@ID_TipoVacina, @Lote, @DataValidade, @Fabricante, @Observacoes)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID_TipoVacina", v.ID_TipoVacina);
                cmd.Parameters.AddWithValue("@Lote", v.Lote ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@DataValidade", v.DataValidade);
                cmd.Parameters.AddWithValue("@Fabricante", v.Fabricante ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Observacoes", v.Observacoes ?? (object)DBNull.Value);

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
                                    Observacoes = @Observacoes
                                 WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID_TipoVacina", v.ID_TipoVacina);
                cmd.Parameters.AddWithValue("@Lote", v.Lote ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@DataValidade", v.DataValidade);
                cmd.Parameters.AddWithValue("@Fabricante", v.Fabricante ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Observacoes", v.Observacoes ?? (object)DBNull.Value);
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
    }
}