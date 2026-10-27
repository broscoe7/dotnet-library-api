namespace LibraryManagement.Api.DTOs.Books;

public class ReadBookDto
{
    public required int Id { get; init; }
    public required string Title { get; init; }
    public required int AvailableCopies { get; init; }
    public required int TotalCopies { get; init; }
    public required int CategoryId { get; init; }
    // Because of the naming convention, Mapperly recognizes this is the Name property of the Category entity
    public required string CategoryName { get; init; }
    public ICollection<BookAuthorDto> Authors { get; init; } = [];

}