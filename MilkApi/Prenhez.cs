namespace MilkApi
{
    public class Prenhez
    {
        public int Id { get; set; }
        public int ID_Gado { get; set; }
        public DateTime Data_Prenhez { get; set; }
        public DateTime? Data_Termino { get; set; }
        public DateTime? Data_Esperada { get; set; }
        public int ID_Usuario { get; set; }
        public string? Status{ get; set; }

    }
}
