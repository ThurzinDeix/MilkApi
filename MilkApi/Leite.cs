namespace MilkApi
{
    public class Leite
    {
        public int Id { get; set; }

        public int ID_Gado { get; set; }

        public DateTime Data {get; set; }
        public int CCS { get; set; }

        public decimal Gordura { get; set; }
        public decimal Proteina { get; set; }
        public decimal Litros { get; set; }


    }
}
