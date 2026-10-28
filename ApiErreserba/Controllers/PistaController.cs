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
    public class PistaController : BaseApiController
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

        [HttpPost("ezabatu")]
        public async Task<IActionResult> PostHardDelete([FromBody] PistaDesgaituDto dto)
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            await using MySqlTransaction transaction = await conn.BeginTransactionAsync(IsolationLevel.RepeatableRead);
            try
            {
                await using var cmdSelect = new MySqlCommand("select count(*) as pistaDesgaituta from pista where id = @pista", conn, transaction);
                cmdSelect.Parameters.AddWithValue("@pista", dto.Id);
                var emaitza = await cmdSelect.ExecuteScalarAsync();

                int erreserbak = Convert.ToInt32(emaitza);
                if (erreserbak == 0)
                {
                    await transaction.RollbackAsync();
                    return Error(409, "CONFLICT", "Norbait zuk baino lehenago ezabatu du");
                }

                await using var cmdInsert = new MySqlCommand("delete from pista where id = @pista", conn, transaction);
                cmdInsert.Parameters.AddWithValue("@pista", dto.Id);
                await cmdInsert.ExecuteNonQueryAsync();

                await transaction.CommitAsync();
                return Created(PistaDesgaituDto.SortuPista(dto), 201);
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                await transaction.RollbackAsync();
                return Error(409, "CONFLICT", "Akatsa", ex.Message);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

    }
}
