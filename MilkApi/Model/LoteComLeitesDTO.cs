namespace MilkApi
{
    public class LoteComLeitesDTO
    {
        public int Num { get; set; }
        public int ID_Usuario { get; set; }
        public required List<int> IDsLeite { get; set; }
    }
}
