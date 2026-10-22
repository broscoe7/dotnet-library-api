using LibraryManagement.Api.DataAccess;
using LibraryManagement.Api.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

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
        // the WithTags sets the heading for this endpoint group in OpenAPI (Scalar, Swagger, etc.)

        group.MapGet("/", GetAll);
        group.MapGet("/{id:int}", GetById).WithName("GetCategoryById")
            .ProducesProblem(StatusCodes.Status404NotFound);
        group.MapPost("/", Create)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);
        group.MapPut("/{id:int}", Update)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
        group.MapDelete("/{id:int}", Delete)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }
    
    private static async Task<Ok<List<Category>>> GetAll(LibraryDbContext dbContext)
    {
        var categories = await dbContext.Categories.ToListAsync();
        return TypedResults.Ok(categories);
    }

    private static async Task<Results<Ok<Category>, ProblemHttpResult>> GetById(int id, LibraryDbContext dbContext)
    {
        var category = await dbContext.Categories.FindAsync(id);
        return category is null ? TypedResults
            .Problem(detail: $"Cannot find category with Id {id}", statusCode: StatusCodes.Status404NotFound) : 
            TypedResults.Ok(category);
    }

    private static async Task<Results<CreatedAtRoute<Category>, ProblemHttpResult>> Create(Category category, LibraryDbContext dbContext)
    {
        if (string.IsNullOrWhiteSpace(category.Name))
        {
            return TypedResults.Problem(
                detail: "The Name field cannot be empty",
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        
        if (await dbContext.Categories.AnyAsync(c => c.Name == category.Name))
        {
            return TypedResults.Problem(
                detail: $"A category with Name {category.Name} already exists",
                statusCode: StatusCodes.Status409Conflict
            );
        }
        
        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync();
        return TypedResults.CreatedAtRoute(
            category,
            "GetCategoryById",
            new {id = category.Id});
    }

    private static async Task<Results<Ok<Category>, ProblemHttpResult>> Update(int id, Category category, LibraryDbContext dbContext)
    {
        if (string.IsNullOrWhiteSpace(category.Name))
        {
            return TypedResults.Problem(
                detail: "The Name field cannot be empty",
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        var existingCategory = await dbContext.Categories.FindAsync(id);
        if (existingCategory is null)
        {
            return TypedResults.Problem(
                detail: $"Cannot find category with Id {id}",
                statusCode: StatusCodes.Status404NotFound);
        }

        if (await dbContext.Categories.AnyAsync(c => c.Id != id && c.Name == category.Name))
        {
            return TypedResults.Problem(
                detail: $"A category with Name {category.Name} already exists",
                statusCode: StatusCodes.Status409Conflict);
        }
        existingCategory.Name = category.Name;
        await dbContext.SaveChangesAsync();
        return TypedResults.Ok(existingCategory);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> Delete(int id, LibraryDbContext dbContext)
    {
        var existingCategory = await dbContext.Categories.FindAsync(id);
        if (existingCategory is null)
        {
            return TypedResults.Problem(
                detail: $"Cannot find category with Id {id}",
                statusCode: StatusCodes.Status404NotFound);
        }
        dbContext.Categories.Remove(existingCategory);
        await dbContext.SaveChangesAsync();
        return TypedResults.NoContent();
    }
}