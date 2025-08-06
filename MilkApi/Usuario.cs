namespace MilkApi
{
    public class Usuario
    {
        public int Id { get; set; }
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public string? Senha { get; set; }
        
        
        public DateTime Data_Nasc { get; set; }

        public string? CPF { get; set; }

        
    }
}
