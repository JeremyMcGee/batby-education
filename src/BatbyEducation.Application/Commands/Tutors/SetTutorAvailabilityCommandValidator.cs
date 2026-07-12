using FluentValidation;

namespace BatbyEducation.Application.Commands.Tutors;

public class SetTutorAvailabilityCommandValidator : AbstractValidator<SetTutorAvailabilityCommand>
{
    public SetTutorAvailabilityCommandValidator()
    {
        RuleFor(x => x.TutorId)
            .NotEmpty().WithMessage("Tutor ID is required");

        RuleFor(x => x.Slots)
            .NotNull().WithMessage("Slots are required");

        RuleForEach(x => x.Slots).ChildRules(slot =>
        {
            slot.RuleFor(s => s.EndTime)
                .Must((s, endTime) => endTime > s.StartTime)
                .WithMessage("End time must be after start time");

            slot.RuleFor(s => s)
                .Must(s => (s.EndTime - s.StartTime).TotalMinutes >= 30)
                .WithMessage("Availability slot must be at least 30 minutes");
        });
    }
}
