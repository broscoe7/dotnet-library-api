namespace LibraryManagement.Api.DTOs.Categories;

public record CreateCategoryDTO
{
    public required string Name { get; init; }
}