namespace ApiErreserba.Models
{
    public class Erreserba
    {
        public int Id { get; set; }
        public int Pista_id { get; set; }
        public int Bazkidea_id { get; set; }
        public DateOnly Data { get; set; }
        public TimeOnly Ordua { get; set; }
    }
}
