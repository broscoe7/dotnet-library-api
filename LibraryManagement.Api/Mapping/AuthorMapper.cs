using LibraryManagement.Api.DTOs.Authors;
using LibraryManagement.Api.Entities;
using Riok.Mapperly.Abstractions;

namespace LibraryManagement.Api.Mapping;

[Mapper]
public partial class AuthorMapper
{
  public partial ReadAuthorDTO ToReadDto(Author source);
  
  [MapperIgnoreTarget(nameof(Author.Id))]
  public partial Author ToEntity(CreateAuthorDTO source);
  
  [MapperIgnoreTarget(nameof(Author.Id))]
  public partial void UpdateEntity(UpdateAuthorDTO source, Author target);
}
