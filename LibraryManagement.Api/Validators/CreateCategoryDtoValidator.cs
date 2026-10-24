using FluentValidation;
using LibraryManagement.Api.DTOs.Categories;

namespace LibraryManagement.Api.Validators;

public class CreateCategoryDtoValidator : AbstractValidator<CreateCategoryDto>
{
    public CreateCategoryDtoValidator()
    {
        RuleFor(dto => dto.Name)
            .NotEmpty()
            .MaximumLength(50);
    }
}