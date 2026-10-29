using LibraryManagement.Api.CustomResults;
using LibraryManagement.Api.DTOs.Authors;
using LibraryManagement.Api.RequestParameters;
using ErrorOr;

namespace LibraryManagement.Api.Services.Contracts;

public interface IAuthorsService
{
    Task<ErrorOr<PagedResult<ReadAuthorDto>>> GetAllAsync(PaginationParameters pagination);

    Task<ErrorOr<ReadAuthorDetailDto>> GetByIdAsync(int id);

    Task<ErrorOr<ReadAuthorDetailDto>> CreateAsync(CreateAuthorDto createDto);

    Task<ErrorOr<ReadAuthorDetailDto>> UpdateAsync(int id, UpdateAuthorDto updateDto);

    Task<ErrorOr<Deleted>> DeleteAsync(int id);
}
