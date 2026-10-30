using LibraryManagement.Api.DTOs.Authors;
using LibraryManagement.Api.Filters;
using LibraryManagement.Api.RequestParameters;
using LibraryManagement.Api.Services.Contracts;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Api.Controllers;

[ServiceFilter(typeof(LogActionFilter))]
[ApiController]
[Route("api/[controller]")]
public class AuthorsController(IAuthorsService service) : WebApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(List<ReadAuthorDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ReadAuthorDto>>> GetAll([FromQuery] PaginationParameters pagination)
    {
        var result = await service.GetAllAsync(pagination);
        if (result.IsError)
        {
            return Problem(result.FirstError);
        }

        Response.Headers.Append("X-Total-Count", result.Value.TotalCount.ToString());
        Response.Headers.Append("X-Total-Pages", result.Value.TotalPages.ToString());

        return Ok(result.Value.Items);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReadAuthorDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReadAuthorDetailDto>> GetById(int id)
    {
        var result = await service.GetByIdAsync(id);
        if (result.IsError)
        {
            return Problem(result.FirstError);
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ReadAuthorDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReadAuthorDetailDto>> Create(CreateAuthorDto createDto)
    {
        var result = await service.CreateAsync(createDto);
        if (result.IsError)
        {
            return Problem(result.FirstError);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }


    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ReadAuthorDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReadAuthorDetailDto>> Update(int id, UpdateAuthorDto updateDto)
    {
        var result = await service.UpdateAsync(id, updateDto);
        if (result.IsError)
        {
            return Problem(result.FirstError);
        }

        return Ok(result.Value);
    }

    [HttpPatch("{id:int}")]
    [Consumes("application/json-patch+json")]
    [ProducesResponseType(typeof(ReadAuthorDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReadAuthorDetailDto>> Patch(int id, JsonPatchDocument<PatchAuthorDto> patchDocument)
    {
        // because the patch request data is typed as JsonPatchDocument, the data annotations will
        // not be automatically applied. We have to manually activate the validation.
        var patchDto = new PatchAuthorDto();
        patchDocument.ApplyTo(
            patchDto, 
            jsonPatchError => ModelState.AddModelError(
                jsonPatchError.AffectedObject.GetType().Name, 
                jsonPatchError.ErrorMessage));
        if (!TryValidateModel(patchDto))
        {
            return ValidationProblem(ModelState);
            // ModelState is a dictionary that .Net stores validation errors in
        }
        
        var result = await service.PatchAsync(id, patchDocument);
        if (result.IsError)
        {
            return Problem(result.FirstError);
        }

        return Ok(result.Value);
    }
    
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await service.DeleteAsync(id);
        if (result.IsError)
        {
            return Problem(result.FirstError);
        }
        return NoContent();
    }

}
