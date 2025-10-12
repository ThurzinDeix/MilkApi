using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace MilkApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReproducaoController : Controller
    {
        private readonly string ConnectionString = config.ConnectionString;
        private readonly ILogger<ReproducaoController> _logger;

        public ReproducaoController(ILogger<ReproducaoController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<Reproducao> Get()
        {
            List<Reproducao> lista = new List<Reproducao>();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = "SELECT * FROM Reproducao";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new Reproducao
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ID_Gado = Convert.ToInt32(reader["ID_Gado"]),
                        Tipo = reader["Tipo"]?.ToString(),
                        Data = Convert.ToDateTime(reader["Data"]),
                        Observacao = reader["Observacao"]?.ToString(),
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
                string query = "SELECT * FROM Reproducao WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    var repro = new Reproducao
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ID_Gado = Convert.ToInt32(reader["ID_Gado"]),
                        Tipo = reader["Tipo"]?.ToString(),
                        Data = Convert.ToDateTime(reader["Data"]),
                        Observacao = reader["Observacao"]?.ToString(),
                        ID_Usuario = Convert.ToInt32(reader["ID_Usuario"])
                    };

                    reader.Close();
                    return Ok(repro);
                }

                reader.Close();
                return NotFound();
            }
        }

        // NOVO ENDPOINT: GET por usuário
        [HttpGet("por-usuario")]
        public IEnumerable<Reproducao> GetByUsuario(int usuarioId)
        {
            List<Reproducao> lista = new List<Reproducao>();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = "SELECT * FROM Reproducao WHERE ID_Usuario = @ID_Usuario";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID_Usuario", usuarioId);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lista.Add(new Reproducao
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ID_Gado = Convert.ToInt32(reader["ID_Gado"]),
                        Tipo = reader["Tipo"]?.ToString(),
                        Data = Convert.ToDateTime(reader["Data"]),
                        Observacao = reader["Observacao"]?.ToString(),
                        ID_Usuario = Convert.ToInt32(reader["ID_Usuario"])
                    });
                }

                reader.Close();
            }

            return lista;
        }

        [HttpPost]
        public ActionResult Create(Reproducao repro)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"INSERT INTO Reproducao (ID_Gado, Tipo, Data, Observacao, ID_Usuario) 
                                 VALUES (@ID_Gado, @Tipo, @Data, @Observacao, @ID_Usuario)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID_Gado", repro.ID_Gado);
                cmd.Parameters.AddWithValue("@Tipo", (object?)repro.Tipo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Data", repro.Data);
                cmd.Parameters.AddWithValue("@Observacao", (object?)repro.Observacao ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ID_Usuario", repro.ID_Usuario);

                conn.Open();
                int rows = cmd.ExecuteNonQuery();

                if (rows > 0) return Ok();
            }

            return BadRequest();
        }

        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] Reproducao repro)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"UPDATE Reproducao SET 
                                    ID_Gado = @ID_Gado,
                                    Tipo = @Tipo,
                                    Data = @Data,
                                    Observacao = @Observacao,
                                    ID_Usuario = @ID_Usuario
                                 WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID_Gado", repro.ID_Gado);
                cmd.Parameters.AddWithValue("@Tipo", (object?)repro.Tipo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Data", repro.Data);
                cmd.Parameters.AddWithValue("@Observacao", (object?)repro.Observacao ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ID_Usuario", repro.ID_Usuario);
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
                string query = "DELETE FROM Reproducao WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                conn.Open();
                int rows = cmd.ExecuteNonQuery();

                if (rows > 0) return Ok();
            }

            return NotFound();
        }

        [HttpGet("verificar-prenha")]
        public ActionResult VerificarPrenha(string brinco, int usuarioId)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                // Buscar o ID_Gado pelo brinco
                string queryGado = "SELECT Id FROM Gado WHERE Brinco = @Brinco AND ID_Usuario = @ID_Usuario";
                SqlCommand cmdGado = new SqlCommand(queryGado, conn);
                cmdGado.Parameters.AddWithValue("@Brinco", brinco);
                cmdGado.Parameters.AddWithValue("@ID_Usuario", usuarioId);

                var gadoIdObj = cmdGado.ExecuteScalar();
                if (gadoIdObj == null)
                {
                    return NotFound(new { mensagem = "Gado não encontrado." });
                }

                int gadoId = Convert.ToInt32(gadoIdObj);

                // Verifica se há prenhez ativa para este gado
                string queryPrenha = "SELECT COUNT(*) FROM Prenhez WHERE ID_Gado = @ID_Gado AND Status = 'Prenha'";
                SqlCommand cmdPrenha = new SqlCommand(queryPrenha, conn);
                cmdPrenha.Parameters.AddWithValue("@ID_Gado", gadoId);

                int count = Convert.ToInt32(cmdPrenha.ExecuteScalar());

                if (count > 0)
                {
                    return Ok(new { prenha = true, mensagem = "Esta vaca já está prenha." });
                }
                else
                {
                    return Ok(new { prenha = false });
                }
            }
        }
    }
}