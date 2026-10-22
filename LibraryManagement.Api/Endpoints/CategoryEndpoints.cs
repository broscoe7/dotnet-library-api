using LibraryManagement.Api.DataAccess;
using LibraryManagement.Api.DTOs.Categories;
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
    
    private static async Task<Ok<List<ReadCategoryDTO>>> GetAll(LibraryDbContext dbContext)
    {
        var categories = await dbContext.Categories.ToListAsync();
        var readDtos = categories.Select(category => 
            new ReadCategoryDTO
            {
                Id = category.Id, 
                Name = category.Name,
            }).ToList();
        return TypedResults.Ok(readDtos);
    }

    private static async Task<Results<Ok<ReadCategoryDTO>, ProblemHttpResult>> GetById(int id, LibraryDbContext dbContext)
    {
        var category = await dbContext.Categories.FindAsync(id);
        if (category is null)
        {
            return TypedResults
                .Problem(detail: $"Cannot find category with Id {id}", statusCode: StatusCodes.Status404NotFound);
        }

        var readDto = new ReadCategoryDTO { Id = category.Id, Name = category.Name };
        return TypedResults.Ok(readDto);
    }

    private static async Task<Results<CreatedAtRoute<ReadCategoryDTO>, ProblemHttpResult>> Create(CreateCategoryDTO createDto, LibraryDbContext dbContext)
    {
        if (string.IsNullOrWhiteSpace(createDto.Name))
        {
            return TypedResults.Problem(
                detail: "The Name field cannot be empty",
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        
        if (await dbContext.Categories.AnyAsync(c => c.Name == createDto.Name))
        {
            return TypedResults.Problem(
                detail: $"A category with Name {createDto.Name} already exists",
                statusCode: StatusCodes.Status409Conflict
            );
        }

        var category = new Category { Name = createDto.Name };
        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync();
        
        var readDto = new ReadCategoryDTO { Id = category.Id, Name = category.Name };
        return TypedResults.CreatedAtRoute(
            readDto,
            "GetCategoryById",
            new {id = category.Id});
    }

    private static async Task<Results<Ok<ReadCategoryDTO>, ProblemHttpResult>> Update(int id, UpdateCategoryDTO updateDto, LibraryDbContext dbContext)
    {
        if (string.IsNullOrWhiteSpace(updateDto.Name))
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

        if (await dbContext.Categories.AnyAsync(c => c.Id != id && c.Name == updateDto.Name))
        {
            return TypedResults.Problem(
                detail: $"A category with Name {updateDto.Name} already exists",
                statusCode: StatusCodes.Status409Conflict);
        }
        existingCategory.Name = updateDto.Name;
        await dbContext.SaveChangesAsync();
        var readDto = new ReadCategoryDTO { Id = existingCategory.Id, Name = existingCategory.Name };
        return TypedResults.Ok(readDto);
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