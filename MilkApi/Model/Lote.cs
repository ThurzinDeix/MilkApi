namespace MilkApi
{
    public class Lote
    {
        public int Id { get; set; }
        public int ID_Leite { get; set; }
        public int Num { get; set; }
        public int ID_Usuario { get; set; }
        public List<Leite> leites { get; set; } = new List<Leite>();
        public Qualidade qualidade { get; set; } = null;

    }
}
