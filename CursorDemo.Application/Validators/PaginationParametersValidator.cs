using CursorDemo.Application.DTOs;
using FluentValidation;

namespace CursorDemo.Application.Validators;

public class PaginationParametersValidator : AbstractValidator<PaginationParameters>
{
    public PaginationParametersValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithMessage("PageSize must be greater than or equal to 1.")
            .LessThanOrEqualTo(100)
            .WithMessage("PageSize must be less than or equal to 100.");
    }
}

