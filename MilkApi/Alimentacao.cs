namespace MilkApi
{
    public class Alimentacao
    {
        public int Id { get; set; }

        public int ID_Gado { get; set; }

        public DateTime Data {get; set; }  

        public string? Tipo { get; set; }

        public float Quantidade { get; set; }

        public string? Observacao { get; set; }
    }
}
