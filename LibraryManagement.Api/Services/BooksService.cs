using LibraryManagement.Api.DataAccess;
using LibraryManagement.Api.DTOs.Books;
using LibraryManagement.Api.Mapping;
using LibraryManagement.Api.Services.Contracts;
using Microsoft.EntityFrameworkCore;
using ErrorOr;
using LibraryManagement.Api.CustomResults;
using LibraryManagement.Api.Entities;
using LibraryManagement.Api.Errors;
using LibraryManagement.Api.RequestParameters;
using LibraryManagement.Api.RequestParameters.Enums;

namespace LibraryManagement.Api.Services;

public class BooksService(LibraryDbContext dbContext, BookMapper mapper) : IBooksService
{
    public async Task<ErrorOr<PagedResult<ReadBookDto>>> GetAllAsync(
        PaginationParameters pagination, 
        BookFilteringParameters filters,
        SortingParameters<BookSortBy> sortingParameters)
    {
        var query = dbContext.Books
            .Include(book => book.Category)
            .Include(book => book.Authors)
            .AsQueryable();
        
        // apply filters
        if (filters.Title is not null)
        {
            query = query.Where(book => book.Title.Contains(filters.Title));
        }
        if (filters.CategoryId is not null)
        {
            query = query.Where(book => book.CategoryId == filters.CategoryId);
        }
        if (filters.AuthorId is not null)
        {
            query = query.Where(book => book.Authors.Any(author => author.Id == filters.AuthorId));
        }
        
        // sort
        query = sortingParameters.SortBy switch
        {
            BookSortBy.Title => sortingParameters.SortDirection == SortDirections.Asc
                ? query.OrderBy(book => book.Title)
                : query.OrderByDescending(book => book.Title),
            BookSortBy.AvailableCopies => sortingParameters.SortDirection == SortDirections.Asc
                ? query.OrderBy(book => book.AvailableCopies)
                : query.OrderByDescending(book => book.AvailableCopies),
            _ => query.OrderBy(book => book.Id)
        };
        
        var totalCount = await query.CountAsync();
        
        var books = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();
        var readDtos = books.Select(mapper.ToReadDto).ToList(); 

        return new PagedResult<ReadBookDto>(
            readDtos,
            totalCount,
            pagination);
    }
    
    public async Task<ErrorOr<ReadBookDto>> GetByIdAsync(int id)
    {
        var book = await dbContext.Books
            .Include(b => b.Category)
            .Include(b => b.Authors)
            .FirstOrDefaultAsync(b => b.Id == id);
        if (book is null)
        {
            return BookErrors.BookNotFound(id);
        }

        return mapper.ToReadDto(book);
    }
    
    public async Task<ErrorOr<ReadBookDto>> CreateAsync(CreateBookDto createDto)
    {
        // validate category
        var category = await dbContext.Categories.FindAsync(createDto.CategoryId);
        if (category is null)
        {
            return BookErrors.CategoryNotFound(createDto.CategoryId);
        }
        
        // gracefully handle possibility of duplicate author ids
        var uniqueAuthorIds = createDto.AuthorIds.Distinct().ToList();

        // validate authors
        var authors = await dbContext.Authors.Where(a => uniqueAuthorIds.Contains(a.Id)).ToListAsync();
        if (authors.Count != uniqueAuthorIds.Count)
        {
            return BookErrors.InvalidAuthorId();
        }
        
        var book = mapper.ToEntity(createDto);
        book.AvailableCopies = book.TotalCopies; // since none have been borrowed yet
        book.Category = category;
        book.Authors = authors;
        
        dbContext.Books.Add(book);
        await dbContext.SaveChangesAsync();

        return mapper.ToReadDto(book);
    }
    
    public async Task<ErrorOr<ReadBookDto>> UpdateAsync(int id, UpdateBookDto updateDto)
    {
        var existingBook = await dbContext.Books
            .Include(b => b.Authors)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (existingBook is null)
        {
            return BookErrors.BookNotFound(id);
        }
        
        // validate category
        var category = await dbContext.Categories.FindAsync(updateDto.CategoryId);
        if (category is null)
        {
            return BookErrors.CategoryNotFound(updateDto.CategoryId);
        }
        
        // gracefully handle possibility of duplicate author ids
        var uniqueAuthorIds = updateDto.AuthorIds.Distinct().ToList();

        // validate authors
        var authors = await dbContext.Authors.Where(a => uniqueAuthorIds.Contains(a.Id)).ToListAsync();
        if (authors.Count != uniqueAuthorIds.Count)
        {
            return BookErrors.InvalidAuthorId();
        }

        var totalCopiesBeforeEntityUpdate = existingBook.TotalCopies;
        UpdateAvailableCopies(totalCopiesBeforeEntityUpdate, updateDto.TotalCopies, existingBook);

        mapper.UpdateEntity(updateDto, existingBook);
        existingBook.Category = category;
        existingBook.Authors = authors;
        await dbContext.SaveChangesAsync();

        return mapper.ToReadDto(existingBook);
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
    
    public async Task<ErrorOr<Deleted>> DeleteAsync(int id)
    {
        var existingBook = await dbContext.Books.FindAsync(id);
        if (existingBook is null)
        {
            return BookErrors.BookNotFound(id);
        }

        dbContext.Books.Remove(existingBook);
        await dbContext.SaveChangesAsync();
        return Result.Deleted;
    }
}