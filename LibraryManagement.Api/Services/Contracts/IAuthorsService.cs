using LibraryManagement.Api.CustomResults;
using LibraryManagement.Api.DTOs.Authors;
using LibraryManagement.Api.RequestParameters;
using ErrorOr;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;

namespace LibraryManagement.Api.Services.Contracts;

public interface IAuthorsService
{
    Task<ErrorOr<PagedResult<ReadAuthorDto>>> GetAllAsync(PaginationParameters pagination);

    Task<ErrorOr<ReadAuthorDetailDto>> GetByIdAsync(int id);

    Task<ErrorOr<ReadAuthorDetailDto>> CreateAsync(CreateAuthorDto createDto);

    Task<ErrorOr<ReadAuthorDetailDto>> UpdateAsync(int id, UpdateAuthorDto updateDto);

    Task<ErrorOr<ReadAuthorDetailDto>> PatchAsync(int id, JsonPatchDocument<PatchAuthorDto> patchDto);

    Task<ErrorOr<Deleted>> DeleteAsync(int id);
}
