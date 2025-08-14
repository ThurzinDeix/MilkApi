using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace MilkApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AlimentacaoController : Controller
    {

        private const string ConnectionString = "Server=milkdatabase.cp64yi8w2sr2.us-east-2.rds.amazonaws.com;Database=BancoTccGado;User Id=Arthur;Password=Arthur-1234;TrustServerCertificate=True;"; 
        private readonly ILogger<AlimentacaoController> _logger;

        public AlimentacaoController(ILogger<AlimentacaoController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<Alimentacao> Get()
        {
            List<Alimentacao> lista = new List<Alimentacao>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = "SELECT * FROM Alimentacao";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Alimentacao item = new Alimentacao
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ID_Gado = Convert.ToInt32(reader["ID_Gado"]),
                        Data = Convert.ToDateTime(reader["Data"]),
                        Tipo = reader["Tipo"]?.ToString(),
                        Quantidade = Convert.ToSingle(reader["Quantidade"]),
                        Observacao = reader["Observacao"]?.ToString()
                    };
                    lista.Add(item);
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
                string query = "SELECT * FROM Alimentacao WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    Alimentacao item = new Alimentacao
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ID_Gado = Convert.ToInt32(reader["ID_Gado"]),
                        Data = Convert.ToDateTime(reader["Data"]),
                        Tipo = reader["Tipo"]?.ToString(),
                        Quantidade = Convert.ToSingle(reader["Quantidade"]),
                        Observacao = reader["Observacao"]?.ToString()
                    };

                    reader.Close();
                    return Ok(item);
                }

                reader.Close();
                return NotFound();
            }
        }

        [HttpPost]
        public ActionResult Create(Alimentacao item)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"INSERT INTO Alimentacao (ID_Gado, Data, Tipo, Quantidade, Observacao) 
                                 VALUES (@ID_Gado, @Data, @Tipo, @Quantidade, @Observacao)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID_Gado", item.ID_Gado);
                cmd.Parameters.AddWithValue("@Data", item.Data);
                cmd.Parameters.AddWithValue("@Tipo", item.Tipo ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Quantidade", item.Quantidade);
                cmd.Parameters.AddWithValue("@Observacao", item.Observacao ?? (object)DBNull.Value);

                conn.Open();
                int rows = cmd.ExecuteNonQuery();

                if (rows > 0) return Ok();
            }

            return BadRequest();
        }

        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] Alimentacao item)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"UPDATE Alimentacao SET 
                                    ID_Gado = @ID_Gado,
                                    Data = @Data,
                                    Tipo = @Tipo,
                                    Quantidade = @Quantidade,
                                    Observacao = @Observacao
                                 WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID_Gado", item.ID_Gado);
                cmd.Parameters.AddWithValue("@Data", item.Data);
                cmd.Parameters.AddWithValue("@Tipo", item.Tipo ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Quantidade", item.Quantidade);
                cmd.Parameters.AddWithValue("@Observacao", item.Observacao ?? (object)DBNull.Value);
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
                string query = "DELETE FROM Alimentacao WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                conn.Open();
                int rows = cmd.ExecuteNonQuery();

                if (rows > 0) return Ok();
            }

            return NotFound();
        }
    }

    // Modelo usado pelo controller
    public class Alimentacao
    {
        public int Id { get; set; }
        public int ID_Gado { get; set; }
        public DateTime Data { get; set; }
        public string? Tipo { get; set; }
        public float Quantidade { get; set; }
        public string? Observacao { get; set; }
    }
}
