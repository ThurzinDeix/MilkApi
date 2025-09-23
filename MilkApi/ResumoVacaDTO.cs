namespace MilkApi
{
    public class ResumoVacaDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; } = "";
        public string Brinco { get; set; } = "";
        public string Raca { get; set; } = "";

        public List<Leite> Leites { get; set; } = new List<Leite>();
        public List<ManejoGeral> Manejos { get; set; } = new List<ManejoGeral>();
        public List<Prenhez> Prenhezes { get; set; } = new List<Prenhez>();
        public List<Remedio> Remedios { get; set; } = new List<Remedio>();
        public List<Reproducao> Reproducoes { get; set; } = new List<Reproducao>();
        public List<Suplemento> Suplementos { get; set; } = new List<Suplemento>();
        public List<Alertas> Alertas { get; set; } = new List<Alertas>();
        public List<Lote> Lotes { get; set; } = new List<Lote>();
    }
}
