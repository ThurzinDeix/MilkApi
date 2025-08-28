namespace MilkApi
{
    public class ManejoGeral
    {
        public int Id { get; set; }

        public int ID_Gado { get; set; }

        public string? Tipo_Manejo { get; set; }

        public DateTime Data_Manejo {get; set; }

        public string? Observacoes { get; set; }

        public int ID_Usuario { get; set; }

    }
}
