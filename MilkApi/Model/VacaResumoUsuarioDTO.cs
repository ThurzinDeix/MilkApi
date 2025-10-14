namespace MilkApi
{
    public class VacaResumoUsuarioDTO
    {
        public int Id { get; set; }
        public string Brinco { get; set; } = string.Empty;
        public string Raca { get; set; } = string.Empty;
        public string Sexo { get; set; } = string.Empty;
        public int TotalRegistros { get; set; }

        public bool TemLeite { get; set; }
        public bool TemManejo { get; set; }
        public bool TemPrenhez { get; set; }
        public bool TemRemedio { get; set; }
        public bool TemReproducao { get; set; }
        public bool TemSuplemento { get; set; }
        public bool TemAlerta { get; set; }
        public bool TemLote { get; set; }
        public bool TemQualidade { get; internal set; }
    }
}
