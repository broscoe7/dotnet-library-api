using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Api.RequestParameters;

public record BookFilteringParameters(
    [property: FromQuery(Name = "title")] string? Title, 
    [property: FromQuery(Name = "categoryId")] int? CategoryId, 
    [property: FromQuery(Name = "authorId")] int? AuthorId);
    // since this is a record, not a class, the [FromQuery] will get applied to the constructor (not the property)
    // unless we specify that this should be applied to the property.