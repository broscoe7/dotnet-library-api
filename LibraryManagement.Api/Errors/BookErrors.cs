using ErrorOr;

namespace LibraryManagement.Api.Errors;

public static class BookErrors
{
    public static Error CategoryNotFound(int id) => Error.Validation(
            description: $"Category with Id {id} does not exist"
        );
    
    public static Error BookNotFound(int id) => Error.NotFound(
        description: $"Book with Id {id} does not exist"
    );
    
    public static Error InvalidAuthorId() => Error.Validation(
        description: "One or more of the provided author Ids is not valid"
    );
}