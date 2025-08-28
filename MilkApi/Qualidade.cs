namespace MilkApi
{
    public class Qualidade
    {
        public int Id { get; set; }
        public int ID_Lote { get; set; }
        public int CCS { get; set; }
        public decimal Gordura { get; set; }
        public decimal Proteina { get; set; }

        public int ID_Usuario { get; set; }
    }
}
