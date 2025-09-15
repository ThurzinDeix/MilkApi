using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace MilkApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoteController : Controller
    {
        private readonly string ConnectionString = config.ConnectionString;
        private readonly ILogger<LoteController> _logger;

        public LoteController(ILogger<LoteController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<IEnumerable<Lote>> Get()
        {
            var lotes = new List<Lote>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                await conn.OpenAsync();

                // 1) Pega todos os lotes
                string queryLotes = "SELECT * FROM Lote";
                SqlCommand cmdLotes = new SqlCommand(queryLotes, conn);
                SqlDataReader readerLotes = await cmdLotes.ExecuteReaderAsync();

                while (await readerLotes.ReadAsync())
                {
                    lotes.Add(new Lote
                    {
                        Id = Convert.ToInt32(readerLotes["Id"]),
                        Num = Convert.ToInt32(readerLotes["Num"]),
                        ID_Usuario = Convert.ToInt32(readerLotes["ID_Usuario"]),
                        leites = new List<Leite>(),
                        qualidade = null // inicializa como null
                    });
                }
                readerLotes.Close();

                // 2) Pega todos os leites associados a cada lote
                string queryLeites = @"
            SELECT ll.ID_Lote, l.Id AS LeiteId, l.ID_Gado, l.Data, l.Litros, l.ID_Usuario
            FROM LoteLeite ll
            INNER JOIN Leite l ON ll.ID_Leite = l.Id";
                SqlCommand cmdLeites = new SqlCommand(queryLeites, conn);
                SqlDataReader readerLeites = await cmdLeites.ExecuteReaderAsync();

                while (await readerLeites.ReadAsync())
                {
                    int loteId = Convert.ToInt32(readerLeites["ID_Lote"]);
                    var leite = new Leite
                    {
                        Id = Convert.ToInt32(readerLeites["LeiteId"]),
                        ID_Gado = Convert.ToInt32(readerLeites["ID_Gado"]),
                        Data = Convert.ToDateTime(readerLeites["Data"]),
                        Litros = Convert.ToDecimal(readerLeites["Litros"]),
                        ID_Usuario = Convert.ToInt32(readerLeites["ID_Usuario"])
                    };

                    var lote = lotes.FirstOrDefault(l => l.Id == loteId);
                    if (lote != null)
                    {
                        lote.leites.Add(leite);
                    }
                }
                readerLeites.Close();

                // 3) Pega a qualidade de cada lote
                string queryQualidade = "SELECT * FROM Qualidade";
                SqlCommand cmdQualidade = new SqlCommand(queryQualidade, conn);
                SqlDataReader readerQualidade = await cmdQualidade.ExecuteReaderAsync();

                while (await readerQualidade.ReadAsync())
                {
                    int loteId = Convert.ToInt32(readerQualidade["ID_Lote"]);
                    var qualidade = new Qualidade
                    {
                        Id = Convert.ToInt32(readerQualidade["Id"]),
                        ID_Lote = loteId,
                        CCS = Convert.ToInt32(readerQualidade["CCS"]),
                        Gordura = Convert.ToDecimal(readerQualidade["Gordura"]),
                        Proteina = Convert.ToDecimal(readerQualidade["Proteina"]),
                        ID_Usuario = Convert.ToInt32(readerQualidade["ID_Usuario"])
                    };

                    var lote = lotes.FirstOrDefault(l => l.Id == loteId);
                    if (lote != null)
                    {
                        lote.qualidade = qualidade;
                    }
                }
                readerQualidade.Close();
            }

            return lotes;
        }



        [HttpGet("{id}")]
        public ActionResult GetById(int id)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = "SELECT * FROM Lote WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    var lote = new Lote
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Num = Convert.ToInt32(reader["Num"]),
                        ID_Usuario = Convert.ToInt32(reader["ID_Usuario"])
                    };
                    reader.Close();
                    return Ok(lote);
                }

                reader.Close();
                return NotFound();
            }
        }

        [HttpPost]
        public ActionResult Create(Lote lote)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"INSERT INTO Lote (Num, ID_Usuario)
                                 VALUES (@Num, @ID_Usuario)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Num", lote.Num);
                cmd.Parameters.AddWithValue("@ID_Usuario", lote.ID_Usuario);

                conn.Open();
                int rows = cmd.ExecuteNonQuery();
                if (rows > 0) return Ok();
            }

            return BadRequest();
        }

        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] Lote lote)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"UPDATE Lote SET 
                                    Num = @Num,
                                    ID_Usuario = @ID_Usuario
                                 WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Num", lote.Num);
                cmd.Parameters.AddWithValue("@ID_Usuario", lote.ID_Usuario);
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
                string query = "DELETE FROM Lote WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                conn.Open();
                int rows = cmd.ExecuteNonQuery();
                if (rows > 0) return Ok();
            }

            return NotFound();
        }

        [HttpGet("por-numero/{num}")]
        public ActionResult<IEnumerable<Lote>> GetByNumero(int num)
        {
            List<Lote> lista = new List<Lote>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = "SELECT * FROM Lote WHERE Num = @Num";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Num", num);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lista.Add(new Lote
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ID_Leite = Convert.ToInt32(reader["ID_Leite"]),
                        Num = Convert.ToInt32(reader["Num"]),
                        ID_Usuario = Convert.ToInt32(reader["ID_Usuario"])
                    });
                }

                reader.Close();
            }

            if (lista.Count == 0)
                return NotFound();

            return Ok(lista);
        }

        [HttpPost("criar-com-leites")]
        public async Task<IActionResult> CriarLoteComLeites([FromBody] LoteComLeitesDTO dto)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                await conn.OpenAsync();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        // 1) Criar lote
                        string insertLote = "INSERT INTO Lote (Num, ID_Usuario) OUTPUT INSERTED.Id VALUES (@Num, @ID_Usuario)";
                        SqlCommand cmdLote = new SqlCommand(insertLote, conn, tran);
                        cmdLote.Parameters.AddWithValue("@Num", dto.Num);
                        cmdLote.Parameters.AddWithValue("@ID_Usuario", dto.ID_Usuario);

                        int loteId = (int)await cmdLote.ExecuteScalarAsync();

                        // 2) Criar relações na tabela LoteLeite
                        foreach (var idLeite in dto.IDsLeite)
                        {
                            string insertLoteLeite = "INSERT INTO LoteLeite (ID_Lote, ID_Leite) VALUES (@ID_Lote, @ID_Leite)";
                            SqlCommand cmdLoteLeite = new SqlCommand(insertLoteLeite, conn, tran);
                            cmdLoteLeite.Parameters.AddWithValue("@ID_Lote", loteId);
                            cmdLoteLeite.Parameters.AddWithValue("@ID_Leite", idLeite);
                            await cmdLoteLeite.ExecuteNonQueryAsync();
                        }

                        tran.Commit();
                        return Ok(new { loteId = loteId });
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        return StatusCode(500, $"Erro ao criar lote: {ex.Message}");
                    }
                }
            }
        }

        [HttpGet("{id}/leites")]
        public ActionResult<IEnumerable<Leite>> GetLeitesDoLote(int id)
        {
            List<Leite> lista = new List<Leite>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"
            SELECT l.* 
            FROM LoteLeite ll
            INNER JOIN Leite l ON ll.ID_Leite = l.Id
            WHERE ll.ID_Lote = @Id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lista.Add(new Leite
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ID_Gado = Convert.ToInt32(reader["ID_Gado"]),
                        Data = Convert.ToDateTime(reader["Data"]),
                        Litros = Convert.ToDecimal(reader["Litros"]),
                        ID_Usuario = Convert.ToInt32(reader["ID_Usuario"])
                    });
                }

                reader.Close();
            }

            return Ok(lista);
        }



    }
}
