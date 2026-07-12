using FluentValidation;

namespace BatbyEducation.Application.Commands.Tutors;

public class RegisterTutorCommandValidator : AbstractValidator<RegisterTutorCommand>
{
    public RegisterTutorCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email format is invalid")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

        RuleFor(x => x.Subjects)
            .NotNull().WithMessage("Subjects are required")
            .Must(s => s != null && s.Count >= 1).WithMessage("At least one subject is required")
            .Must(s => s != null && s.Count <= 20).WithMessage("No more than 20 subjects are allowed");

        RuleForEach(x => x.Subjects)
            .NotEmpty().WithMessage("Subject name must not be empty");

        RuleFor(x => x.HourlyRate)
            .InclusiveBetween(0.01m, 999.99m)
            .WithMessage("Hourly rate must be between £0.01 and £999.99");
    }
}
