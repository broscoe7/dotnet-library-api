using ErrorOr;
using LibraryManagement.Api.CustomResults;
using LibraryManagement.Api.DataAccess;
using LibraryManagement.Api.DTOs.Authors;
using LibraryManagement.Api.Errors;
using LibraryManagement.Api.Mapping;
using LibraryManagement.Api.RequestParameters;
using LibraryManagement.Api.Services.Contracts;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Api.Services;

public class AuthorsService(LibraryDbContext dbContext, AuthorMapper mapper) : IAuthorsService
{
    public async Task<ErrorOr<PagedResult<ReadAuthorDto>>> GetAllAsync(PaginationParameters pagination)
    {
        var totalCount = await dbContext.Authors.CountAsync();

        var authors = await dbContext.Authors
            .OrderBy(author => author.Id)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();
        var readDtos = authors.Select(mapper.ToReadDto).ToList();

        return new PagedResult<ReadAuthorDto>(
            readDtos,
            totalCount,
            pagination);
    }

    public async Task<ErrorOr<ReadAuthorDetailDto>> GetByIdAsync(int id)
    {
        var author = await dbContext.Authors
            .Include(a => a.Books)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (author is null)
        {
            return AuthorErrors.AuthorNotFound(id);
        }

        return mapper.ToReadDetailDto(author);
    }

    public async Task<ErrorOr<ReadAuthorDetailDto>> CreateAsync(CreateAuthorDto createDto)
    {
        var author = mapper.ToEntity(createDto);

        dbContext.Authors.Add(author);
        await dbContext.SaveChangesAsync();

        return mapper.ToReadDetailDto(author);
    }

    public async Task<ErrorOr<ReadAuthorDetailDto>> UpdateAsync(int id, UpdateAuthorDto updateDto)
    {
        var existingAuthor = await dbContext.Authors
            .Include(a => a.Books)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (existingAuthor is null)
        {
            return AuthorErrors.AuthorNotFound(id);
        }

        mapper.UpdateEntity(updateDto, existingAuthor);
        await dbContext.SaveChangesAsync();

        return mapper.ToReadDetailDto(existingAuthor);
    }

    public async Task<ErrorOr<ReadAuthorDetailDto>> PatchAsync(int id, JsonPatchDocument<PatchAuthorDto> patchJson)
    {
        var existingAuthor = await dbContext.Authors
            .Include(a => a.Books)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (existingAuthor is null)
        {
            return AuthorErrors.AuthorNotFound(id);
        }

        var patchDto = mapper.ToPatchDto(existingAuthor);
        patchJson.ApplyTo(patchDto);
        mapper.UpdateEntity(patchDto, existingAuthor);
        await dbContext.SaveChangesAsync();

        return mapper.ToReadDetailDto(existingAuthor);
    }

    public async Task<ErrorOr<Deleted>> DeleteAsync(int id)
    {
        var existingAuthor = await dbContext.Authors
            .Include(a => a.Books)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (existingAuthor is null)
        {
            return AuthorErrors.AuthorNotFound(id);
        }

        // it should be impossible to delete an author if they are still associated with a book
        // (the book needs to be deleted first)
        if (existingAuthor.Books.Count > 0)
        {
            return AuthorErrors.AuthorHasBooks(id);
        }

        dbContext.Authors.Remove(existingAuthor);
        await dbContext.SaveChangesAsync();
        return Result.Deleted;
    }
}
