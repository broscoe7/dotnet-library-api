using LibraryManagement.Api.DataAccess;
using LibraryManagement.Api.DTOs.Books;
using LibraryManagement.Api.Entities;
using LibraryManagement.Api.Mapping;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController(LibraryDbContext dbContext, BookMapper mapper): ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<ReadBookDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ReadBookDto>>> GetAll()
    {
        var books = await dbContext.Books
            .Include(book => book.Category)
            .Include(book => book.Authors)
            .ToListAsync();
        return Ok(books.Select(mapper.ToReadDto).ToList());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReadBookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReadBookDto>> GetById(int id)
    {
        var book = await dbContext.Books
            .Include(b => b.Category)
            .Include(b => b.Authors)
            .FirstOrDefaultAsync(b => b.Id == id);
        if (book is null)
        {
            return Problem(
                detail: $"Cannot find book with Id {id}",
                statusCode: StatusCodes.Status404NotFound);
        }

        return Ok(mapper.ToReadDto(book));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ReadBookDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReadBookDto>> Create(CreateBookDto createDto)
    {
        // validate category
        var category = await dbContext.Categories.FindAsync(createDto.CategoryId);
        if (category is null)
        {
            return Problem(
                detail: $"Category with Id {createDto.CategoryId} does not exist",
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        
        // gracefully handle possibility of duplicate author ids
        var uniqueAuthorIds = createDto.AuthorIds.Distinct().ToList();

        // validate authors
        var authors = await dbContext.Authors.Where(a => uniqueAuthorIds.Contains(a.Id)).ToListAsync();
        if (authors.Count != uniqueAuthorIds.Count)
        {
            return Problem(
                detail: "One or more of the provided author Ids is not valid",
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        
        var book = mapper.ToEntity(createDto);
        book.AvailableCopies = book.TotalCopies; // since none have been borrowed yet
        book.Category = category;
        book.Authors = authors;
        
        dbContext.Books.Add(book);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new {id = book.Id}, mapper.ToReadDto(book));
    }


    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ReadBookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReadBookDto>> Update(int id, UpdateBookDto updateDto)
    {
        var existingBook = await dbContext.Books
            .Include(b => b.Authors)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (existingBook is null)
        {
            return Problem(
                detail: $"Cannot find book with Id {id}",
                statusCode: StatusCodes.Status404NotFound);
        }
        
        // validate category
        var category = await dbContext.Categories.FindAsync(updateDto.CategoryId);
        if (category is null)
        {
            return Problem(
                detail: $"Category with Id {updateDto.CategoryId} does not exist",
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        
        // gracefully handle possibility of duplicate author ids
        var uniqueAuthorIds = updateDto.AuthorIds.Distinct().ToList();

        // validate authors
        var authors = await dbContext.Authors.Where(a => uniqueAuthorIds.Contains(a.Id)).ToListAsync();
        if (authors.Count != uniqueAuthorIds.Count)
        {
            return Problem(
                detail: "One or more of the provided author Ids is not valid",
                statusCode: StatusCodes.Status400BadRequest
            );
        }

        var totalCopiesBeforeEntityUpdate = existingBook.TotalCopies;
        UpdateAvailableCopies(totalCopiesBeforeEntityUpdate, updateDto.TotalCopies, existingBook);

        mapper.UpdateEntity(updateDto, existingBook);
        existingBook.Category = category;
        existingBook.Authors = authors;
        await dbContext.SaveChangesAsync();

        return Ok(mapper.ToReadDto(existingBook));
    }

    private static void UpdateAvailableCopies(int oldTotalCopies, int newTotalCopies, Book existingBook)
    {
        var totalCopiesDelta = newTotalCopies - oldTotalCopies;
        if (totalCopiesDelta > 0)
        {
            existingBook.AvailableCopies += totalCopiesDelta;
        }
        else if (totalCopiesDelta < 0)
        {
            // TODO: Once loans are implemented, add check here to ensure that
            // total copies cannot be reduced below the number of active loans.
            existingBook.AvailableCopies += totalCopiesDelta;
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(int id)
    {
        var existingBook = await dbContext.Books.FindAsync(id);
        if (existingBook is null)
        {
            return Problem(
                detail: $"Cannot find book with Id {id}",
                statusCode: StatusCodes.Status404NotFound);
        }
        dbContext.Books.Remove(existingBook);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

}
