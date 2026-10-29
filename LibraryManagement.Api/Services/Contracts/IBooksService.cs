using LibraryManagement.Api.DTOs.Books;
using ErrorOr;
using LibraryManagement.Api.CustomResults;
using LibraryManagement.Api.RequestParameters;
using LibraryManagement.Api.RequestParameters.Enums;

namespace LibraryManagement.Api.Services.Contracts;

public interface IBooksService
{
    Task<ErrorOr<PagedResult<ReadBookDto>>> GetAllAsync(
        PaginationParameters pagination,
        BookFilteringParameters filters,
        SortingParameters<BookSortBy> sortingParameters);

    Task<ErrorOr<ReadBookDto>> GetByIdAsync(int id);
    
    Task<ErrorOr<ReadBookDto>> CreateAsync(CreateBookDto createDto);

    Task<ErrorOr<ReadBookDto>> UpdateAsync(int id, UpdateBookDto updateDto);

    Task<ErrorOr<Deleted>> DeleteAsync(int id);
}