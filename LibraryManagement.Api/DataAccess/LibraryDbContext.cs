using System;
using LibraryManagement.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Api.DataAccess;

public class LibraryDbContext : DbContext
{
  public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
  {
  }

  // define db tables
  // the sets are cached by EF for better performance
  public DbSet<Category> Categories => Set<Category>();
  public DbSet<Author> Authors => Set<Author>();
  public DbSet<Book> Books => Set<Book>();
}
