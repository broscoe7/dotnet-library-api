using LibraryManagement.Api.DataAccess;
using LibraryManagement.Api.DTOs.Authors;
using LibraryManagement.Api.Filters;
using LibraryManagement.Api.Mapping;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Api.Controllers;

[ServiceFilter(typeof(LogActionFilter))]
[ApiController]
[Route("api/[controller]")]
public class AuthorsController(LibraryDbContext dbContext, AuthorMapper mapper): ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<ReadAuthorDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ReadAuthorDTO>>> GetAll()
    {
        var authors = await dbContext.Authors.ToListAsync();
        return Ok(authors.Select(mapper.ToReadDto).ToList());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReadAuthorDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReadAuthorDTO>> GetById(int id)
    {
        var author = await dbContext.Authors.FindAsync(id);
        if (author is null)
        {
            return Problem(
                detail: $"Cannot find author with Id {id}",
                statusCode: StatusCodes.Status404NotFound);
        }

        return Ok(mapper.ToReadDto(author));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ReadAuthorDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReadAuthorDTO>> Create(CreateAuthorDto createDto)
    {
        var author = mapper.ToEntity(createDto);
        dbContext.Authors.Add(author);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new {id = author.Id}, mapper.ToReadDto(author));
    }


    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ReadAuthorDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReadAuthorDTO>> Update(int id, UpdateAuthorDto updateDto)
    {
        var existingAuthor = await dbContext.Authors.FindAsync(id);

        if (existingAuthor is null)
        {
            return Problem(
                detail: $"Cannot find author with Id {id}",
                statusCode: StatusCodes.Status404NotFound);
        }

        mapper.UpdateEntity(updateDto, existingAuthor);
        await dbContext.SaveChangesAsync();

        return Ok(mapper.ToReadDto(existingAuthor));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(int id)
    {
        var existingAuthor = await dbContext.Authors.FindAsync(id);
        if (existingAuthor is null)
        {
            return Problem(
                detail: $"Cannot find author with Id {id}",
                statusCode: StatusCodes.Status404NotFound);
        }
        dbContext.Authors.Remove(existingAuthor);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

}
