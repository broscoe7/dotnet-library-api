using LibraryManagement.Api.DataAccess;
using LibraryManagement.Api.Endpoints;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();
// tells .Net how to generate URLs for things like Post response headers
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddDbContext<LibraryDbContext>(options => options.UseNpgsql(
    builder.Configuration.GetConnectionString("LibraryDb")
));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    await DataSeeder.SeedAsync(app.Services);
}

app.UseHttpsRedirection();

app.MapControllers();
app.MapCategoryEndpoints();

app.Run();

