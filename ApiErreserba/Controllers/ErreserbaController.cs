using ApiErreserba.Responses;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using ApiErreserba.DTO;
using ApiErreserba.Models;


namespace ApiErreserba.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ErreserbaController : BaseApiController
    {
        private readonly string _connectionString;
        private readonly ILogger<ErreserbaController> _logger;

        public ErreserbaController(IConfiguration configuration, ILogger<ErreserbaController> logger)
        {
            _connectionString = configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Default konexio-katea falta da.");
            _logger = logger;
        }
        [HttpPost("insert/{Pista_id}/{Bazkidea_id}/{Data}/{Ordua}")]
        public ActionResult<ApiResponse<Erreserba>> Gehitu([FromRoute] ErreserbaSortuDto dto)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                connection.Open();
                using var transaction = connection.BeginTransaction();

                using var cmd = new MySqlCommand("INSERT INTO erreserba (pista_id, bazkidea_id, data, ordua) VALUES (@pista_id, @bazkidea_id, @data, @ordua);", connection, transaction);
                cmd.Parameters.AddWithValue("@pista_id", dto.Pista_id);
                cmd.Parameters.AddWithValue("@bazkidea_id", dto.Bazkidea_id);
                cmd.Parameters.AddWithValue("@data", dto.Data.ToDateTime(TimeOnly.MinValue));
                cmd.Parameters.AddWithValue("@ordua", dto.Ordua);
                cmd.ExecuteNonQuery();
                transaction.Commit();
                return StatusCode(201, ApiResponse<Erreserba>.Ok((Erreserba?)null, 201, "Erreserba gehitu da."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errorea erreserba gehitzean.");
                return StatusCode(500, ApiError.Create(500, "datu_basearen_errorea", "Ezin izan da erreserba gehitu."));
            }
        }
    }
}