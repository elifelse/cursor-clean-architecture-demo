using CursorDemo.Application.DTOs;
using FluentValidation;

namespace CursorDemo.Application.Validators;

public class CreateBookDtoValidator : AbstractValidator<CreateBookDto>
{
    public CreateBookDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required.")
            .MinimumLength(3)
            .WithMessage("Title must be at least 3 characters long.");

        RuleFor(x => x.Author)
            .NotEmpty()
            .WithMessage("Author is required.")
            .MinimumLength(3)
            .WithMessage("Author must be at least 3 characters long.");

        RuleFor(x => x.ISBN)
            .NotEmpty()
            .WithMessage("ISBN is required.");

        RuleFor(x => x.PublishedDate)
            .NotEmpty()
            .WithMessage("Published date is required.")
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Published date cannot be in the future.");
    }
}

