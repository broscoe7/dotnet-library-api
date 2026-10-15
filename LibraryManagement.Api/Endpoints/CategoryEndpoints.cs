using LibraryManagement.Api.Entities;

namespace LibraryManagement.Api.Endpoints;

public static class CategoryEndpoints
{
    private static List<Category> _categories =
    [
        new Category { Id = 1, Name = "Biography" },
        new Category { Id = 2, Name = "Novel" },
    ];

    // an extension method to register categories endpoints as a group
    public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categories").WithTags("Categories");

        group.MapGet("/", GetAll);
    }
    
    private static async Task<IResult> GetAll()
    {
        var categories = _categories;
        return Results.Ok(categories);
    }
}