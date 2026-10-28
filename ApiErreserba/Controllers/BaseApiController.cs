using ApiErreserba.Responses;
using Microsoft.AspNetCore.Mvc;

namespace ApiErreserba.Controllers
{
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        protected IActionResult Succes<T>(T data, int code, string message = "OK")
            => Ok(ApiResponse<T>.Ok(data, code, message));
        protected IActionResult SuccessList<T>(List<T> dataList, int code, string message = "OK")
            => Ok(ApiResponse<T>.Ok(dataList, code, message));
        protected IActionResult Created<T>(T data, int code, string message = "Zuzen sortu da")
            => StatusCode(201, ApiResponse<T>.Ok(data, code, message));
        protected IActionResult Error(int code, string type, string message, string detail = null)
            => StatusCode(code, ApiError.Create(code, type, message, detail));
        protected IActionResult NotFoudError(string detail)
            => Error(404, "NOT_FOUND", "Eskatutakoa ez da aurkitu", detail);
    }
}
