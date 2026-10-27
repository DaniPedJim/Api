using ApiErreserba.Models;
using MySql.Data.MySqlClient;
using System.ComponentModel.DataAnnotations;

namespace ApiErreserba.DTO
{
    public class ErreserbaSortuDto
    {
        [Required]
        public int Pista_id { get; set; }
        [Required]
        public int Bazkidea_id { get; set; }
        [Required]
        public TimeOnly Ordua {  get; set; }
        [Required]
        public DateOnly Data {  get; set; }

        public static Erreserba SortuErreserba(ErreserbaSortuDto dto) => new()
        {
            Pista_id = dto.Pista_id,
            Bazkidea_id = dto.Bazkidea_id,
            Data = dto.Data,
            Ordua = dto.Ordua
        };
        public static void GehituParametroak(MySqlCommand cmd, ErreserbaSortuDto dto)
        {
            cmd.Parameters.AddWithValue("@pista_id", dto.Pista_id);
            cmd.Parameters.AddWithValue("@bazkidea_id", dto.Bazkidea_id);
            cmd.Parameters.AddWithValue("@data", dto.Data);
            cmd.Parameters.AddWithValue("@ordua", dto.Ordua);
        }
    }
}
