namespace LibraryManagement.Api.DTOs.Categories;

public record ReadCategoryDTO
{
    public required int Id { get; init; }
    public required string Name { get; init; }
}