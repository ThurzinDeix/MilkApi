namespace MilkApi
{
    public class Gado
    {
        public int Id { get; set; }

        public int ID_Fazenda { get; set; }

        public int ID_Saude { get; set; }

        public int ID_Alementacao { get; set; }

        public int ID_Leite { get; set; }

        public int ID_Alerta { get; set; }

        public int ID_Reproducao { get; set; }

        public DateTime Data_Nasc {get; set; }  

        public string? Raca { get; set; }

        public float Peso { get; set; }
        public string? Sexo { get; set; }  
        
        public int Brinco { get; set; }

        public string? Observacao { get; set; }

    }
}
