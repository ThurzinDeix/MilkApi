namespace MilkApi
{
    public class Alertas
    {
        public int Id { get; set; }

        public int ID_Gado { get; set; }

        public DateTime Data_Prevista {get; set; }  

        public string? Status { get; set; }
        public int ID_Usuario { get; set; }
    }
}
