using LibraryManagement.Api.DTOs.Books;
using LibraryManagement.Api.RequestParameters;
using LibraryManagement.Api.RequestParameters.Enums;
using LibraryManagement.Api.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController(IBooksService service): WebApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(List<ReadBookDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ReadBookDto>>> GetAll(
        [FromQuery] PaginationParameters pagination, 
        [FromQuery] BookFilteringParameters filters,
        [FromQuery] SortingParameters<BookSortBy> sortingParameters)
    {
        var result = await service.GetAllAsync(pagination, filters, sortingParameters);
        if (result.IsError)
        {
            return Problem(result.FirstError);
        }
            
        Response.Headers.Append("X-Total-Count", result.Value.TotalCount.ToString());
        Response.Headers.Append("X-Total-Pages", result.Value.TotalPages.ToString());
        
        return Ok(result.Value.Items);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReadBookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReadBookDto>> GetById(int id)
    {
        var result = await service.GetByIdAsync(id);
        if (result.IsError)
        {
            return Problem(result.FirstError);
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ReadBookDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReadBookDto>> Create(CreateBookDto createDto)
    {
        var result = await service.CreateAsync(createDto);
        if (result.IsError)
        {
            return Problem(result.FirstError);
        }

        return CreatedAtAction(nameof(GetById), new {id = result.Value.Id}, result.Value);
    }


    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ReadBookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReadBookDto>> Update(int id, UpdateBookDto updateDto)
    {
        var result = await service.UpdateAsync(id, updateDto);
        if (result.IsError)
        {
            return Problem(result.FirstError);
        }

        return Ok(result.Value);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
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
