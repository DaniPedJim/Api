using ApiErreserba.DTO;
using ApiErreserba.Models;
using ApiErreserba.Responses;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

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
            return EzarriDesgaituta(dto, true);
        }

        [HttpPatch("{id:int}/berreskuratu")]
        public ActionResult<ApiResponse<Pista>> Berreskuratu([FromRoute] PistaDesgaituDto dto)
        {
            return EzarriDesgaituta(dto, false);
        }

        private ActionResult<ApiResponse<Pista>> EzarriDesgaituta(PistaDesgaituDto dto, bool desgaituta)
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
                            pista.Desgaituta = desgaituta;
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

                    using var pistaEguneratu = new MySqlCommand(
                        "UPDATE pista SET desgaituta = @desgaituta WHERE id = @id;",
                        connection, transaction);
                    PistaDesgaituDto.GehituParametroak(pistaEguneratu, dto);
                    pistaEguneratu.Parameters.AddWithValue("@desgaituta", desgaituta ? 1 : 0);
                    pistaEguneratu.ExecuteNonQuery();

                    using var erreserbakEguneratu = new MySqlCommand(
                        "UPDATE erreserba SET desgaituta = @desgaituta WHERE pista_id = @id;",
                        connection, transaction);
                    PistaDesgaituDto.GehituParametroak(erreserbakEguneratu, dto);
                    erreserbakEguneratu.Parameters.AddWithValue("@desgaituta", desgaituta ? 1 : 0);
                    erreserbakEguneratu.ExecuteNonQuery();

                    // Bi eguneratzeak ondo amaitu direnean baieztatzen dira aldaketak.
                    transaction.Commit();
                    return Ok(ApiResponse<Pista>.Ok(pista, 200,
                        desgaituta
                            ? "Pista eta haren erreserbak desgaitu dira."
                            : "Pista eta haren erreserbak berreskuratu dira."));
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
                _logger.LogError(exception, "Errorea {PistaId} pistaren egoera {Desgaituta} baliora aldatzean.",
                    dto.Id, desgaituta);
                return StatusCode(500, ApiError.Create(500, "datu_basearen_errorea",
                    desgaituta
                        ? "Ezin izan dira pista eta haren erreserbak desgaitu."
                        : "Ezin izan dira pista eta haren erreserbak berreskuratu."));
            }
        }
    }
}
