using FluentValidation;
using RecuritmentTask.src.RecruitmentTask.Domain.Entities;

namespace RecuritmentTask.src.Application.Validators
{
    public class TodoValidator : AbstractValidator<Todo>
    {
        public TodoValidator()
        {
            RuleFor(todo => todo.Title)
           .NotNull()
               .WithMessage("Title cannot be null")
           .NotEmpty()
               .WithMessage("Title cannot be empty")
           .MaximumLength(60)
               .WithMessage(todo => $"Title cannot be longer than 60 characters. Provided title length: {todo.Title.Length}");

            RuleFor(todo => todo.Description)
                .NotNull()
                    .WithMessage("Description cannot be null")
                .MaximumLength(400)
                    .WithMessage(todo => $"Description cannot be longer than 400 characters. Provided description length: {todo.Description.Length}");

            RuleFor(todo => todo.CompletedPercentage)
                .NotNull()
                    .WithMessage("CompletedPercentage cannot be null")
                .InclusiveBetween(0.0, 1.0)
                    .WithMessage(todo => $"Percentage must be a double number between 0.0 and 1.0. Provided percentage: {todo.CompletedPercentage}");

            RuleFor(todo => todo.ExpiryDate)
                .NotNull()
                    .WithMessage("ExpiryDate cannot be null")
                .GreaterThan(DateTime.UtcNow)
                    .WithMessage(todo => $"Expiry date must be in the future. Provided date: {todo.ExpiryDate:yyyy-MM-dd HH:mm:ss}");

        }

    }
}
