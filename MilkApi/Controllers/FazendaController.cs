using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace MilkApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FazendaController : Controller
    {
        private const string ConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=BancoTccGado;Integrated Security=True;";
        private readonly ILogger<FazendaController> _logger;

        public FazendaController(ILogger<FazendaController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<Fazenda> Get()
        {
            List<Fazenda> lista = new List<Fazenda>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = "SELECT * FROM Fazenda";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Fazenda f = new Fazenda
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Nome = reader["Nome"]?.ToString(),
                        ID_Usuario = Convert.ToInt32(reader["ID_Usuario"]),
                        ID_Endereco = Convert.ToInt32(reader["ID_Endereco"]),
                        ID_Gado = Convert.ToInt32(reader["ID_Gado"])
                    };
                    lista.Add(f);
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
                string query = "SELECT * FROM Fazenda WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    Fazenda f = new Fazenda
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Nome = reader["Nome"]?.ToString(),
                        ID_Usuario = Convert.ToInt32(reader["ID_Usuario"]),
                        ID_Endereco = Convert.ToInt32(reader["ID_Endereco"]),
                        ID_Gado = Convert.ToInt32(reader["ID_Gado"])
                    };

                    reader.Close();
                    return Ok(f);
                }

                reader.Close();
                return NotFound();
            }
        }

        [HttpPost]
        public ActionResult Create(Fazenda f)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"INSERT INTO Fazenda (Nome, ID_Usuario, ID_Endereco, ID_Gado) 
                                 VALUES (@Nome, @ID_Usuario, @ID_Endereco, @ID_Gado)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Nome", f.Nome ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ID_Usuario", f.ID_Usuario);
                cmd.Parameters.AddWithValue("@ID_Endereco", f.ID_Endereco);
                cmd.Parameters.AddWithValue("@ID_Gado", f.ID_Gado);

                conn.Open();
                int rows = cmd.ExecuteNonQuery();

                if (rows > 0) return Ok();
            }

            return BadRequest();
        }

        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] Fazenda f)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"UPDATE Fazenda SET 
                                    Nome = @Nome,
                                    ID_Usuario = @ID_Usuario,
                                    ID_Endereco = @ID_Endereco,
                                    ID_Gado = @ID_Gado
                                 WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Nome", f.Nome ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ID_Usuario", f.ID_Usuario);
                cmd.Parameters.AddWithValue("@ID_Endereco", f.ID_Endereco);
                cmd.Parameters.AddWithValue("@ID_Gado", f.ID_Gado);
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
                string query = "DELETE FROM Fazenda WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                conn.Open();
                int rows = cmd.ExecuteNonQuery();

                if (rows > 0) return Ok();
            }

            return NotFound();
        }
    }

    public class Fazenda
    {
        public int Id { get; set; }
        public string? Nome { get; set; }
        public int ID_Usuario { get; set; }
        public int ID_Endereco { get; set; }
        public int ID_Gado { get; set; }
    }
}
