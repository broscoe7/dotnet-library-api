using LibraryManagement.Api.DataAccess;
using LibraryManagement.Api.DTOs.Authors;
using LibraryManagement.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthorsController(LibraryDbContext dbContext): ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<ReadAuthorDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ReadAuthorDTO>>> GetAll()
    {
        var authors = await dbContext.Authors.ToListAsync();
        var readDtos = authors.Select(author =>
            new ReadAuthorDTO
            {
                Id = author.Id,
                FirstName = author.FirstName,
                LastName = author.LastName,
                YearOfBirth = author.YearOfBirth,
            }).ToList();
        return Ok(readDtos);
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

        var readDto = new ReadAuthorDTO
        {
            Id = author.Id,
            FirstName = author.FirstName,
            LastName = author.LastName,
            YearOfBirth = author.YearOfBirth,
        };
        return Ok(readDto);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ReadAuthorDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReadAuthorDTO>> Create(CreateAuthorDTO createDto)
    {
        if (string.IsNullOrWhiteSpace(createDto.FirstName) ||
        string.IsNullOrWhiteSpace(createDto.LastName))
        {
            return Problem(
                detail: "Author's first and last names cannot be empty",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var author = new Author
        {
            FirstName = createDto.FirstName,
            LastName = createDto.LastName,
            YearOfBirth = createDto.YearOfBirth,
        };
        dbContext.Authors.Add(author);
        await dbContext.SaveChangesAsync();

        var readDto = new ReadAuthorDTO
        {
            Id = author.Id,
            FirstName = author.FirstName,
            LastName = author.LastName,
            YearOfBirth = author.YearOfBirth,
        };
        return CreatedAtAction(nameof(GetById), new {id = author.Id}, readDto);
    }


    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ReadAuthorDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReadAuthorDTO>> Update(int id, UpdateAuthorDTO updateDto)
    {
        var existingAuthor = await dbContext.Authors.FindAsync(id);

        if (existingAuthor is null)
        {
            return Problem(
                detail: $"Cannot find author with Id {id}",
                statusCode: StatusCodes.Status404NotFound);
        }

        if (string.IsNullOrWhiteSpace(updateDto.FirstName) ||
            string.IsNullOrWhiteSpace(updateDto.LastName))
        {
            return Problem(
                detail: "Author's first and last names cannot be empty",
                statusCode: StatusCodes.Status400BadRequest);
        }

        existingAuthor.FirstName = updateDto.FirstName;
        existingAuthor.LastName = updateDto.LastName;
        existingAuthor.YearOfBirth = updateDto.YearOfBirth;
        await dbContext.SaveChangesAsync();

        var readDto = new ReadAuthorDTO
        {
            Id = existingAuthor.Id,
            FirstName = existingAuthor.FirstName,
            LastName = existingAuthor.LastName,
            YearOfBirth = existingAuthor.YearOfBirth,
        };
        return Ok(readDto);
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
