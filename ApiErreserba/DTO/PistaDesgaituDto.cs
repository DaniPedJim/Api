using ApiErreserba.Models;
using MySql.Data.MySqlClient;

namespace ApiErreserba.DTO
{
    public class PistaDesgaituDto
    {
        public int Id { get; set; }

        public static Pista SortuPista(PistaDesgaituDto dto) => new()
        {
            Id = dto.Id,
            Desgaituta = true
        };

        public static void GehituParametroak(MySqlCommand cmd, PistaDesgaituDto dto)
        {
            cmd.Parameters.AddWithValue("@id", dto.Id);
        }
    }
}
