using LibraryManagement.Api.DataAccess;
using LibraryManagement.Api.DTOs.Authors;
using LibraryManagement.Api.Filters;
using LibraryManagement.Api.Mapping;
using LibraryManagement.Api.RequestParameters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Api.Controllers;

[ServiceFilter(typeof(LogActionFilter))]
[ApiController]
[Route("api/[controller]")]
public class AuthorsController(LibraryDbContext dbContext, AuthorMapper mapper): ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<ReadAuthorDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ReadAuthorDto>>> GetAll([FromQuery] PaginationParameters pagination)
    {
        var totalCount = await dbContext.Books.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / pagination.PageSize);
        
        var authors = await dbContext.Authors
            .OrderBy(author => author.Id)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        Response.Headers.Append("X-Total-Count", totalCount.ToString());
        Response.Headers.Append("X-Total-Pages", totalPages.ToString());
        
        return Ok(authors.Select(mapper.ToReadDto).ToList());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReadAuthorDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReadAuthorDetailDto>> GetById(int id)
    {
        var author = await dbContext.Authors
            .Include(a => a.Books)
            .FirstOrDefaultAsync(a => a.Id == id);
        
        if (author is null)
        {
            return Problem(
                detail: $"Cannot find author with Id {id}",
                statusCode: StatusCodes.Status404NotFound);
        }

        return Ok(mapper.ToReadDetailDto(author));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ReadAuthorDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReadAuthorDetailDto>> Create(CreateAuthorDto createDto)
    {
        var author = mapper.ToEntity(createDto);
        dbContext.Authors.Add(author);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new {id = author.Id}, mapper.ToReadDetailDto(author));
    }


    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ReadAuthorDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReadAuthorDetailDto>> Update(int id, UpdateAuthorDto updateDto)
    {
        var existingAuthor = await dbContext.Authors.Include(a => a.Books).FirstOrDefaultAsync(a => a.Id == id);

        if (existingAuthor is null)
        {
            return Problem(
                detail: $"Cannot find author with Id {id}",
                statusCode: StatusCodes.Status404NotFound);
        }

        mapper.UpdateEntity(updateDto, existingAuthor);
        await dbContext.SaveChangesAsync();

        return Ok(mapper.ToReadDetailDto(existingAuthor));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Delete(int id)
    {
        var existingAuthor = await dbContext.Authors.Include(a => a.Books).FirstOrDefaultAsync(a => a.Id == id);
        
        if (existingAuthor is null)
        {
            return Problem(
                detail: $"Cannot find author with Id {id}",
                statusCode: StatusCodes.Status404NotFound);
        }
        
        // it should be impossible to delete an author if they are still associated with a book (the book needs to be deleted first)
        if (existingAuthor.Books.Count > 0)
        {
            return Problem(
                detail:
                $"Cannot delete author with Id {id} because they have books associated with them. Please remove or reassign the books first.",
                statusCode: StatusCodes.Status409Conflict
            );
        }
        
        dbContext.Authors.Remove(existingAuthor);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

}
