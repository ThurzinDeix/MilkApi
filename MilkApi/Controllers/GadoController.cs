using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MilkApi;

namespace MilkApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GadoController : Controller
    {
        private const string ConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=BancoTccGado;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";
        private readonly ILogger<GadoController> _logger;

        public GadoController(ILogger<GadoController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<Gado> Get()
        {
            List<Gado> lista = new List<Gado>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = "SELECT * FROM Gado";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Gado gado = new Gado
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ID_Fazenda = Convert.ToInt32(reader["ID_Fazenda"]),
                        ID_Saude = Convert.ToInt32(reader["ID_Saude"]),
                        ID_Alementacao = Convert.ToInt32(reader["ID_Alementacao"]),
                        ID_Leite = Convert.ToInt32(reader["ID_Leite"]),
                        ID_Alerta = Convert.ToInt32(reader["ID_Alerta"]),
                        ID_Reproducao = Convert.ToInt32(reader["ID_Reproducao"]),
                        Data_Nasc = Convert.ToDateTime(reader["Data_Nasc"]),
                        Raca = reader["Raca"]?.ToString(),
                        Peso = Convert.ToSingle(reader["Peso"]),
                        Sexo = reader["Sexo"]?.ToString(),
                        Brinco = Convert.ToInt32(reader["Brinco"]),
                        Observacao = reader["Observacao"]?.ToString()
                    };
                    lista.Add(gado);
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
                string query = "SELECT * FROM Gado WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    Gado gado = new Gado
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ID_Fazenda = Convert.ToInt32(reader["ID_Fazenda"]),
                        ID_Saude = Convert.ToInt32(reader["ID_Saude"]),
                        ID_Alementacao = Convert.ToInt32(reader["ID_Alementacao"]),
                        ID_Leite = Convert.ToInt32(reader["ID_Leite"]),
                        ID_Alerta = Convert.ToInt32(reader["ID_Alerta"]),
                        ID_Reproducao = Convert.ToInt32(reader["ID_Reproducao"]),
                        Data_Nasc = Convert.ToDateTime(reader["Data_Nasc"]),
                        Raca = reader["Raca"]?.ToString(),
                        Peso = Convert.ToSingle(reader["Peso"]),
                        Sexo = reader["Sexo"]?.ToString(),
                        Brinco = Convert.ToInt32(reader["Brinco"]),
                        Observacao = reader["Observacao"]?.ToString()
                    };

                    reader.Close();
                    return Ok(gado);
                }

                reader.Close();
                return NotFound();
            }
        }

        [HttpPost]
        public ActionResult Create(Gado gado)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"INSERT INTO Gado (ID_Fazenda, ID_Saude, ID_Alementacao, ID_Leite, ID_Alerta, ID_Reproducao, Data_Nasc, Raca, Peso, Sexo, Brinco, Observacao)
                                 VALUES (@ID_Fazenda, @ID_Saude, @ID_Alementacao, @ID_Leite, @ID_Alerta, @ID_Reproducao, @Data_Nasc, @Raca, @Peso, @Sexo, @Brinco, @Observacao)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID_Fazenda", gado.ID_Fazenda);
                cmd.Parameters.AddWithValue("@ID_Saude", gado.ID_Saude);
                cmd.Parameters.AddWithValue("@ID_Alementacao", gado.ID_Alementacao);
                cmd.Parameters.AddWithValue("@ID_Leite", gado.ID_Leite);
                cmd.Parameters.AddWithValue("@ID_Alerta", gado.ID_Alerta);
                cmd.Parameters.AddWithValue("@ID_Reproducao", gado.ID_Reproducao);
                cmd.Parameters.AddWithValue("@Data_Nasc", gado.Data_Nasc);
                cmd.Parameters.AddWithValue("@Raca", gado.Raca ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Peso", gado.Peso);
                cmd.Parameters.AddWithValue("@Sexo", gado.Sexo ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Brinco", gado.Brinco);
                cmd.Parameters.AddWithValue("@Observacao", gado.Observacao ?? (object)DBNull.Value);

                conn.Open();
                int rows = cmd.ExecuteNonQuery();

                if (rows > 0) return Ok();
            }

            return BadRequest();
        }

        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] Gado gado)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"UPDATE Gado SET 
                                    ID_Fazenda = @ID_Fazenda,
                                    ID_Saude = @ID_Saude,
                                    ID_Alementacao = @ID_Alementacao,
                                    ID_Leite = @ID_Leite,
                                    ID_Alerta = @ID_Alerta,
                                    ID_Reproducao = @ID_Reproducao,
                                    Data_Nasc = @Data_Nasc,
                                    Raca = @Raca,
                                    Peso = @Peso,
                                    Sexo = @Sexo,
                                    Brinco = @Brinco,
                                    Observacao = @Observacao
                                 WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID_Fazenda", gado.ID_Fazenda);
                cmd.Parameters.AddWithValue("@ID_Saude", gado.ID_Saude);
                cmd.Parameters.AddWithValue("@ID_Alementacao", gado.ID_Alementacao);
                cmd.Parameters.AddWithValue("@ID_Leite", gado.ID_Leite);
                cmd.Parameters.AddWithValue("@ID_Alerta", gado.ID_Alerta);
                cmd.Parameters.AddWithValue("@ID_Reproducao", gado.ID_Reproducao);
                cmd.Parameters.AddWithValue("@Data_Nasc", gado.Data_Nasc);
                cmd.Parameters.AddWithValue("@Raca", gado.Raca ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Peso", gado.Peso);
                cmd.Parameters.AddWithValue("@Sexo", gado.Sexo ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Brinco", gado.Brinco);
                cmd.Parameters.AddWithValue("@Observacao", gado.Observacao ?? (object)DBNull.Value);
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
                string query = "DELETE FROM Gado WHERE Id = @Id";
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
