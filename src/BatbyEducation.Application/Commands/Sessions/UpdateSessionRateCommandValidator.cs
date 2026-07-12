using FluentValidation;

namespace BatbyEducation.Application.Commands.Sessions;

public class UpdateSessionRateCommandValidator : AbstractValidator<UpdateSessionRateCommand>
{
    public UpdateSessionRateCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Session ID is required");

        RuleFor(x => x.RateOverride)
            .InclusiveBetween(0.01m, 999.99m)
            .WithMessage("Rate override must be between £0.01 and £999.99")
            .When(x => x.RateOverride.HasValue);
    }
}
