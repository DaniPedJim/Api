using ApiErreserba.DTO;
using ApiErreserba.Models;
using ApiErreserba.Responses;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data;

namespace ApiErreserba.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PistaController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly ILogger<PistaController> _logger;

        public PistaController(IConfiguration configuration, ILogger<PistaController> logger)
        {
            _connectionString = configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Default konexio-katea falta da.");
            _logger = logger;
        }

        [HttpPatch("{id:int}/desgaitu")]
        public ActionResult<ApiResponse<Pista>> Desgaitu([FromRoute] PistaDesgaituDto dto)
        {
            if (dto.Id <= 0)
            {
                return BadRequest(ApiError.Create(400, "balidazio_errorea",
                    "Pistaren id-ak zero baino handiagoa izan behar du."));
            }

            try
            {
                using var connection = new MySqlConnection(_connectionString);
                connection.Open();
                using var transaction = connection.BeginTransaction();

                try
                {
                    Pista? pista = null;

                    // Pista blokeatzen da transakzioa amaitu arte.
                    using (var bilatu = new MySqlCommand(
                        "SELECT izena, kirola FROM pista WHERE id = @id FOR UPDATE;",
                        connection, transaction))
                    {
                        PistaDesgaituDto.GehituParametroak(bilatu, dto);
                        using var reader = bilatu.ExecuteReader();
                        if (reader.Read())
                        {
                            pista = PistaDesgaituDto.SortuPista(dto);
                            pista.Izena = reader.IsDBNull(0) ? null : reader.GetString(0);
                            pista.Kirola = reader.IsDBNull(1) ? null : reader.GetString(1);
                        }
                    }

                    if (pista is null)
                    {
                        transaction.Rollback();
                        return NotFound(ApiError.Create(404, "ez_da_aurkitu",
                            "Ez da adierazitako pista aurkitu."));
                    }

                    using var pistaDesgaitu = new MySqlCommand(
                        "UPDATE pista SET desgaituta = 1 WHERE id = @id;",
                        connection, transaction);
                    PistaDesgaituDto.GehituParametroak(pistaDesgaitu, dto);
                    pistaDesgaitu.ExecuteNonQuery();

                    using var erreserbakDesgaitu = new MySqlCommand(
                        "UPDATE erreserba SET desgaituta = 1 WHERE pista_id = @id;",
                        connection, transaction);
                    PistaDesgaituDto.GehituParametroak(erreserbakDesgaitu, dto);
                    erreserbakDesgaitu.ExecuteNonQuery();

                    // Bi eguneratzeak ondo amaitu direnean baieztatzen dira aldaketak.
                    transaction.Commit();
                    return Ok(ApiResponse<Pista>.Ok(pista, 200,
                        "Pista eta haren erreserbak desgaitu dira."));
                }
                catch
                {
                    // Eragiketaren batek huts egiten badu, aldaketa guztiak desegiten dira.
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Errorea {PistaId} pista desgaitzean.", dto.Id);
                return StatusCode(500, ApiError.Create(500, "datu_basearen_errorea",
                    "Ezin izan dira pista eta haren erreserbak desgaitu."));
            }
        }

        [HttpPost("select")]
        public async Task<ActionResult<ApiResponse<List<Pista>>>> Bistaratu([FromBody] PistaDesgaituDto dto)
        {

            try
            {
                using var con = new MySqlConnection(_connectionString);
                await con.OpenAsync();

                using var cmd = new MySqlCommand(
                    "SELECT izena, kirola FROM kiroldegia.pista WHERE desgaituta = 0",
                    con);

                var lista = new List<Pista>();
                using var rd = await cmd.ExecuteReaderAsync();

                while (await rd.ReadAsync())
                {
                    lista.Add(new Pista
                    {
                        Izena = rd.GetString("izena"),
                        Kirola = rd.GetString("kirola")
                    });
                }

                return Ok(ApiResponse<List<Pista>>.Ok(lista, 200, "Pistak bistaratuta."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiError.Create(500, "datu_basearen_errorea",
                    "Errorea: " + ex.Message));
            }
        }
    }
}
