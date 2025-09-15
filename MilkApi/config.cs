namespace MilkApi
{
    public class config
    {   
        public static string ConnectionString { get; } =
        "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=BancoTccGado;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";

    }
}
