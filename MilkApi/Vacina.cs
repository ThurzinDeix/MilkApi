namespace MilkApi
{
    public class Vacina
    {
        public int Id { get; set; }

        public int ID_TipoVacina { get; set; }
        public string? Lote { get; set; }
        public DateTime DataValidade { get; set; }
        public string? Fabricante { get; set; }

        public string? Observacoes { get; set; }
        public int ID_Usuario { get; set; }

    }
}
