using ApiErreserba.DTO;
using ApiErreserba.Models;
using ApiErreserba.Responses;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace ApiErreserba.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BazkideaController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly ILogger<BazkideaController> _logger;

        public BazkideaController(IConfiguration configuration, ILogger<BazkideaController> logger)
        {
            _connectionString = configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Default konexio-katea falta da.");
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<Bazkidea>>> Login([FromBody] BazkideLoginDto dto)
        {
            if (dto.Id <= 0)
                return BadRequest(ApiError.Create(400, "balidazio_errorea",
                    "Bazkidearen ID-ak zero baino handiagoa izan behar du."));

            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                using var command = new MySqlCommand(
                    "SELECT id, izena, abizena FROM bazkidea WHERE id = @id;", connection);
                command.Parameters.Add("@id", MySqlDbType.Int32).Value = dto.Id;
                using var reader = await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                    return NotFound(ApiError.Create(404, "ez_da_aurkitu",
                        "Ez dago ID hori duen bazkiderik."));

                var bazkidea = new Bazkidea
                {
                    Id = reader.GetInt32(0),
                    Izena = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    Abizena = reader.IsDBNull(2) ? string.Empty : reader.GetString(2)
                };
                return Ok(ApiResponse<Bazkidea>.Ok(bazkidea, 200, "Saioa hasi da."));
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Errorea {BazkideaId} bazkidea egiaztatzean.", dto.Id);
                return StatusCode(500, ApiError.Create(500, "datu_basearen_errorea",
                    "Ezin izan da bazkidea egiaztatu. Egiaztatu APIaren datu-baseko konexioa."));
            }
        }
    }
}
