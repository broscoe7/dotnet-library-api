using ErrorOr;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Api.Controllers;

public class WebApiController : ControllerBase
{
    protected ActionResult Problem(Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };
        return Problem(detail: error.Description, statusCode: statusCode);
    }
}