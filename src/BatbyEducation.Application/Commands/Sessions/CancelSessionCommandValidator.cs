using FluentValidation;

namespace BatbyEducation.Application.Commands.Sessions;

public class CancelSessionCommandValidator : AbstractValidator<CancelSessionCommand>
{
    public CancelSessionCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Session ID is required");
    }
}
