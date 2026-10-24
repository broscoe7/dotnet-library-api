using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;

using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Api.Extensions;

public static class ValidationExtensions
{
    public static ProblemHttpResult ToProblem(this ValidationResult validationResult)
    {
        return TypedResults.Problem(
            new ValidationProblemDetails(validationResult.ToDictionary())
            {
                Status = StatusCodes.Status400BadRequest
            });
    }
}

// the above works for minimal api endpoints only
// for controller methods use
// return ValidationProblem(
//    new ValidationProblemDetails(ValidationResult.ToDictionary()));
// and make sure the return type is ActionResult