using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace MilkApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoteController : Controller
    {
        private const string ConnectionString = "Server=milkdatabase.cp64yi8w2sr2.us-east-2.rds.amazonaws.com;Database=BancoTccGado;User Id=Arthur;Password=Arthur-1234;TrustServerCertificate=True;";
        private readonly ILogger<LoteController> _logger;

        public LoteController(ILogger<LoteController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<Lote> Get()
        {
            List<Lote> lista = new List<Lote>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = "SELECT * FROM Lote";
                SqlCommand cmd = new SqlCommand(query, conn);
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

            return lista;
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
                        ID_Leite = Convert.ToInt32(reader["ID_Leite"]),
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
                string query = @"INSERT INTO Lote (ID_Leite, Num, ID_Usuario)
                                 VALUES (@ID_Leite, @Num, @ID_Usuario)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID_Leite", lote.ID_Leite);
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
                                    ID_Leite = @ID_Leite,
                                    Num = @Num,
                                    ID_Usuario = @ID_Usuario
                                 WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID_Leite", lote.ID_Leite);
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

    }
}
