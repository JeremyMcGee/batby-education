using FluentValidation;

namespace BatbyEducation.Application.Commands.Sessions;

public class RescheduleSessionCommandValidator : AbstractValidator<RescheduleSessionCommand>
{
    public RescheduleSessionCommandValidator()
    {
        RuleFor(x => x.OriginalSessionId)
            .NotEmpty().WithMessage("Original session ID is required");

        RuleFor(x => x.NewDurationMinutes)
            .InclusiveBetween(15, 240)
            .WithMessage("Duration must be between 15 and 240 minutes");
    }
}
