namespace MilkApi
{
    public class Alerta
    {
        public string Tipo { get; set; }     // danger, warning, info
        public string Mensagem { get; set; }
        public string Origem { get; set; }
        public int? ID_Gado { get; set; }
    }
}
