namespace MilkApi
{
    public class TipoVacina
    {
        public int Id { get; set; }

        public string? Nome { get; set; }

        public string? Descricao { get; set; }

        public string? Obrigatoriedade { get; set; }

        public int IdadeMinimaMeses { get; set; }
        public int PeriodicidadeMeses { get; set; }

        public bool RequerReforco { get; set; }

        public int IntervaloReforcoMeses { get; set; }

        public string? Funcao { get; set; }
        public int ID_Usuario { get; set; }

    }
}
