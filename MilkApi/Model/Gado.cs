namespace MilkApi
{
    public class Gado
    {
        public int Id { get; set; }

        public int ID_Usuario { get; set; } 

        public DateTime Data_Nasc {get; set; }  

        public string? Raca { get; set; }

        public float Peso { get; set; }
        public string? Sexo { get; set; }  
        
        public int Brinco { get; set; }

        public string? Observacao { get; set; }

        public string? StatusProdutivo { get; set; }
       
        public DateTime? UltimoParto { get; set; }
        public List<Prenhez> Prenhezes { get; set; } = new();

        public bool StatusManual { get; set; } = false;

        public string CalcularStatus()
        {
            var hoje = DateTime.Now;

            var prenhezAtiva = Prenhezes
                .FirstOrDefault(p => p.Data_Termino == null);

            if (StatusManual && !string.IsNullOrEmpty(StatusProdutivo))
                return StatusProdutivo!;

            var partos = Prenhezes
                .Where(p => p.Data_Termino.HasValue && p.Status == "Parto")
                .Select(p => p.Data_Termino.Value);

            DateTime? ultimoParto = partos.Any() ? partos.Max() : null;

            if (prenhezAtiva != null)
            {
                if (ultimoParto == null)
                    return "Novilha Gestante"; 
                else
                {
                    if (prenhezAtiva.Data_Esperada.HasValue &&
                        (prenhezAtiva.Data_Esperada.Value - hoje).TotalDays <= 60)
                        return "Seca";
                    else
                        return "Lactante Gestante";
                }
            }
            else
            {
                if (ultimoParto == null)
                    return "Novilha";

                var diasDesdeParto = (hoje - ultimoParto.Value).TotalDays;
                if (diasDesdeParto <= 305)
                    return "Lactante Vazia";
                else
                    return "Vazia Não Lactante";
            }
        }

    }
}
