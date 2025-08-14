using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace MilkApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsuarioController : Controller
    {

        private const string ConnectionString = "Server=milkdatabase.cp64yi8w2sr2.us-east-2.rds.amazonaws.com;Database=BancoTccGado;User Id=Arthur;Password=Arthur-1234;TrustServerCertificate=True;"; 
        private readonly ILogger<UsuarioController> _logger;

        public UsuarioController(ILogger<UsuarioController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<Usuario> Get()
        {
            List<Usuario> lista = new List<Usuario>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = "SELECT * FROM Usuario";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Usuario usuario = new Usuario
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Nome = reader["Nome"]?.ToString(),
                        Email = reader["Email"]?.ToString(),
                        Senha = reader["Senha"]?.ToString(),
                        Data_Nasc = Convert.ToDateTime(reader["Data_Nasc"]),
                        CPF = reader["CPF"]?.ToString(),
                    };
                    lista.Add(usuario);
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
                string query = "SELECT * FROM Usuario WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    Usuario usuario = new Usuario
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Nome = reader["Nome"]?.ToString(),
                        Email = reader["Email"]?.ToString(),
                        Senha = reader["Senha"]?.ToString(),
                        Data_Nasc = Convert.ToDateTime(reader["Data_Nasc"]),
                        CPF = reader["CPF"]?.ToString(),
                    };

                    reader.Close();
                    return Ok(usuario);
                }

                reader.Close();
                return NotFound();
            }
        }

        [HttpPost]
        public ActionResult Create(Usuario usuario)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"INSERT INTO Usuario 
                                (Nome, Email, Senha, Data_Nasc, CPF) 
                                VALUES (@Nome, @Email, @Senha, @Data_Nasc, @CPF)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Nome", (object?)usuario.Nome ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", (object?)usuario.Email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Senha", (object?)usuario.Senha ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Data_Nasc", usuario.Data_Nasc);
                cmd.Parameters.AddWithValue("@CPF", (object?)usuario.CPF ?? DBNull.Value);

                conn.Open();
                int rows = cmd.ExecuteNonQuery();

                if (rows > 0) return Ok();
            }

            return BadRequest();
        }

        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] Usuario usuario)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"UPDATE Usuario SET 
                                    Nome = @Nome,
                                    Email = @Email,
                                    Senha = @Senha,
                                    Data_Nasc = @Data_Nasc,
                                    CPF = @CPF
                                 WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Nome", (object?)usuario.Nome ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", (object?)usuario.Email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Senha", (object?)usuario.Senha ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Data_Nasc", usuario.Data_Nasc);
                cmd.Parameters.AddWithValue("@CPF", (object?)usuario.CPF ?? DBNull.Value);
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
                string query = "DELETE FROM Usuario WHERE Id = @Id";
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
