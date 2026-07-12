using FluentValidation;

namespace BatbyEducation.Application.Commands.Sessions;

public class CompleteSessionCommandValidator : AbstractValidator<CompleteSessionCommand>
{
    public CompleteSessionCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Session ID is required");

        RuleFor(x => x.ActualDurationMinutes)
            .InclusiveBetween(15, 240)
            .WithMessage("Actual duration must be between 15 and 240 minutes")
            .When(x => x.ActualDurationMinutes.HasValue);
    }
}
