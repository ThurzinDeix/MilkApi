using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace MilkApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HistoricoVacinaController : Controller
    {

        private readonly string ConnectionString = config.ConnectionString;
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
                        Observacoes = reader["Observacoes"]?.ToString(),
                        ID_Usuario = Convert.ToInt32(reader["ID_Usuario"])
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
                        Observacoes = reader["Observacoes"]?.ToString(),
                        ID_Usuario = Convert.ToInt32(reader["ID_Usuario"])
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
                                (ID_Gado, ID_Vacina, Lote, DataAplicacao, ProximaDose, ResponsavelAplicacao, Observacoes, ID_Usuario) 
                                 VALUES (@ID_Gado, @ID_Vacina, @Lote, @DataAplicacao, @ProximaDose, @ResponsavelAplicacao, @Observacoes, @ID_Usuario)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID_Gado", historico.ID_Gado);
                cmd.Parameters.AddWithValue("@ID_Vacina", historico.ID_Vacina);
                cmd.Parameters.AddWithValue("@Lote", (object?)historico.Lote ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DataAplicacao", historico.DataAplicacao);
                cmd.Parameters.AddWithValue("@ProximaDose", historico.ProximaDose);
                cmd.Parameters.AddWithValue("@ResponsavelAplicacao", (object?)historico.ResponsavelAplicacao ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Observacoes", (object?)historico.Observacoes ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ID_Usuario", historico.ID_Usuario);

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
                                    Observacoes = @Observacoes,
                                    ID_Usuario = @ID_Usuario
                                 WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID_Gado", historico.ID_Gado);
                cmd.Parameters.AddWithValue("@ID_Vacina", historico.ID_Vacina);
                cmd.Parameters.AddWithValue("@Lote", (object?)historico.Lote ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DataAplicacao", historico.DataAplicacao);
                cmd.Parameters.AddWithValue("@ProximaDose", historico.ProximaDose);
                cmd.Parameters.AddWithValue("@ResponsavelAplicacao", (object?)historico.ResponsavelAplicacao ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Observacoes", (object?)historico.Observacoes ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ID_Usuario", historico.ID_Usuario);
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

    public class HistoricoVacina
    {
        public int Id { get; set; }
        public int ID_Gado { get; set; }
        public int ID_Vacina { get; set; }
        public string? Lote { get; set; }
        public DateTime DataAplicacao { get; set; }
        public DateTime ProximaDose { get; set; }
        public string? ResponsavelAplicacao { get; set; }
        public string? Observacoes { get; set; }
        public int ID_Usuario { get; set; }
    }
}
