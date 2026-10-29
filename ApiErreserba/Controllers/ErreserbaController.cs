using ApiErreserba.Responses;
using Microsoft.AspNetCore.Mvc;

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
        [HttpPost("insert")]
        public ActionResult<ApiResponse<Pista>> Gehitu([FromRoute] PistaDesgaituDto dto)
        {
            
        }
    }
