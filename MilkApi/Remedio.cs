namespace MilkApi
{
    public class Remedio
    {
        public int Id { get; set; }
        public int ID_Gado { get; set; }
        public string? Nome { get; set; }
        public DateTime Date { get; set; }
        public int Doses { get; set; }
        public int intervalo { get; set; }
        public string? via { get; set; }

        public int ID_Usuario { get; set; }
    }
}
