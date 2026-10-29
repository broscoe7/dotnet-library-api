using ErrorOr;

namespace LibraryManagement.Api.Errors;

public static class AuthorErrors
{
    public static Error AuthorNotFound(int id) => Error.NotFound(
        description: $"Author with Id {id} does not exist"
    );

    public static Error AuthorHasBooks(int id) => Error.Conflict(
        description: $"Cannot delete author with Id {id} because they have books associated with them. Please remove or reassign the books first."
    );
}
