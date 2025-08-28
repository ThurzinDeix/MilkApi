namespace MilkApi
{
    public class HistoricoVacina
    {
        public int Id { get; set; }

        public int ID_Gado { get; set; }

        public int ID_Vacina { get; set; }

        public string? Lote { get; set; }

        public DateTime DataAplicacao {get; set; }
        public DateTime ProximaDose { get; set; }

        public string? ResponsavelAplicacao { get; set; }

        public string? Observacoes { get; set; }

        public int ID_Usuario { get; set; }

    }
}
