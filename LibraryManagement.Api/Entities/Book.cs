namespace LibraryManagement.Api.Entities;

public class Book
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public int AvailableCopies { get; set; }
    public required int TotalCopies { get; set; }
    public required int CategoryId { get; set; } // foreign key property
    public Category? Category { get; set; } // reference navigation property, marked nullable because it will not always be loaded
    public ICollection<Author> Authors { get; set; } = []; // initialized as empty array so that we don't get null exceptions when we try to loop over empty db query results
}