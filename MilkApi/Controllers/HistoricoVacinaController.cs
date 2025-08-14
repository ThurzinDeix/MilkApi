using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace MilkApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HistoricoVacinaController : Controller
    {

        private const string ConnectionString = "Server=milkdatabase.cp64yi8w2sr2.us-east-2.rds.amazonaws.com;Database=BancoTccGado;User Id=Arthur;Password=Arthur-1234;TrustServerCertificate=True;";
        private readonly ILogger<HistoricoVacinaController> _logger;

        public HistoricoVacinaController(ILogger<HistoricoVacinaController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<HistoricoVacina> Get()
        {
            List<HistoricoVacina> lista = new List<HistoricoVacina>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = "SELECT * FROM HistoricoVacina";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    HistoricoVacina historico = new HistoricoVacina
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ID_Gado = Convert.ToInt32(reader["ID_Gado"]),
                        ID_Vacina = Convert.ToInt32(reader["ID_Vacina"]),
                        Lote = reader["Lote"]?.ToString(),
                        DataAplicacao = Convert.ToDateTime(reader["DataAplicacao"]),
                        ProximaDose = Convert.ToDateTime(reader["ProximaDose"]),
                        ResponsavelAplicacao = reader["ResponsavelAplicacao"]?.ToString(),
                        Observacoes = reader["Observacoes"]?.ToString()
                    };
                    lista.Add(historico);
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
                string query = "SELECT * FROM HistoricoVacina WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    HistoricoVacina historico = new HistoricoVacina
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ID_Gado = Convert.ToInt32(reader["ID_Gado"]),
                        ID_Vacina = Convert.ToInt32(reader["ID_Vacina"]),
                        Lote = reader["Lote"]?.ToString(),
                        DataAplicacao = Convert.ToDateTime(reader["DataAplicacao"]),
                        ProximaDose = Convert.ToDateTime(reader["ProximaDose"]),
                        ResponsavelAplicacao = reader["ResponsavelAplicacao"]?.ToString(),
                        Observacoes = reader["Observacoes"]?.ToString()
                    };

                    reader.Close();
                    return Ok(historico);
                }

                reader.Close();
                return NotFound();
            }
        }

        [HttpPost]
        public ActionResult Create(HistoricoVacina historico)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"INSERT INTO HistoricoVacina 
                                (ID_Gado, ID_Vacina, Lote, DataAplicacao, ProximaDose, ResponsavelAplicacao, Observacoes) 
                                 VALUES (@ID_Gado, @ID_Vacina, @Lote, @DataAplicacao, @ProximaDose, @ResponsavelAplicacao, @Observacoes)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID_Gado", historico.ID_Gado);
                cmd.Parameters.AddWithValue("@ID_Vacina", historico.ID_Vacina);
                cmd.Parameters.AddWithValue("@Lote", (object?)historico.Lote ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DataAplicacao", historico.DataAplicacao);
                cmd.Parameters.AddWithValue("@ProximaDose", historico.ProximaDose);
                cmd.Parameters.AddWithValue("@ResponsavelAplicacao", (object?)historico.ResponsavelAplicacao ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Observacoes", (object?)historico.Observacoes ?? DBNull.Value);

                conn.Open();
                int rows = cmd.ExecuteNonQuery();

                if (rows > 0) return Ok();
            }

            return BadRequest();
        }

        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] HistoricoVacina historico)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"UPDATE HistoricoVacina SET 
                                    ID_Gado = @ID_Gado,
                                    ID_Vacina = @ID_Vacina,
                                    Lote = @Lote,
                                    DataAplicacao = @DataAplicacao,
                                    ProximaDose = @ProximaDose,
                                    ResponsavelAplicacao = @ResponsavelAplicacao,
                                    Observacoes = @Observacoes
                                 WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID_Gado", historico.ID_Gado);
                cmd.Parameters.AddWithValue("@ID_Vacina", historico.ID_Vacina);
                cmd.Parameters.AddWithValue("@Lote", (object?)historico.Lote ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DataAplicacao", historico.DataAplicacao);
                cmd.Parameters.AddWithValue("@ProximaDose", historico.ProximaDose);
                cmd.Parameters.AddWithValue("@ResponsavelAplicacao", (object?)historico.ResponsavelAplicacao ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Observacoes", (object?)historico.Observacoes ?? DBNull.Value);
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
                string query = "DELETE FROM HistoricoVacina WHERE Id = @Id";
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
