using FluentValidation;

namespace BatbyEducation.Application.Commands.Sessions;

/// <summary>
/// FluentValidation validator for BookSessionCommand.
/// Ensures basic input constraints are met before the handler processes the command.
/// </summary>
public class BookSessionCommandValidator : AbstractValidator<BookSessionCommand>
{
    public BookSessionCommandValidator()
    {
        RuleFor(x => x.TutorId)
            .NotEmpty()
            .WithMessage("TutorId is required.");

        RuleFor(x => x.StudentId)
            .NotEmpty()
            .WithMessage("StudentId is required.");

        RuleFor(x => x.Subject)
            .NotEmpty()
            .WithMessage("Subject is required.");

        RuleFor(x => x.DurationMinutes)
            .InclusiveBetween(15, 240)
            .WithMessage("Duration must be between 15 and 240 minutes.");

        RuleFor(x => x.SessionDate)
            .Must(date => date >= DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Session date cannot be in the past.");

        RuleFor(x => x.RateOverride)
            .InclusiveBetween(0.01m, 999.99m)
            .WithMessage("Rate override must be between £0.01 and £999.99")
            .When(x => x.RateOverride.HasValue);
    }
}
