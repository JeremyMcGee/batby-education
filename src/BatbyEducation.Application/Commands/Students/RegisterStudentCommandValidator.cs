using FluentValidation;

namespace BatbyEducation.Application.Commands.Students;

public class RegisterStudentCommandValidator : AbstractValidator<RegisterStudentCommand>
{
    public RegisterStudentCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email format is invalid")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters");

        RuleFor(x => x.GuardianName)
            .NotEmpty().WithMessage("Guardian name is required")
            .MaximumLength(100).WithMessage("Guardian name must not exceed 100 characters");

        RuleFor(x => x.GuardianEmail)
            .NotEmpty().WithMessage("Guardian email is required")
            .EmailAddress().WithMessage("Guardian email format is invalid")
            .MaximumLength(255).WithMessage("Guardian email must not exceed 255 characters");

        RuleFor(x => x.HourlyRate)
            .InclusiveBetween(0.01m, 999.99m)
            .WithMessage("Hourly rate must be between £0.01 and £999.99")
            .When(x => x.HourlyRate.HasValue);
    }
}
