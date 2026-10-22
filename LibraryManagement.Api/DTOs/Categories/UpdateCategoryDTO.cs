namespace LibraryManagement.Api.DTOs.Categories;

public record UpdateCategoryDTO
{
    public required string Name { get; init; }
}