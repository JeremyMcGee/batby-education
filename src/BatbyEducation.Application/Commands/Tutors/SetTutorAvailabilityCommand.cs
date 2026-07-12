using BatbyEducation.Domain.Common;
using MediatR;

namespace BatbyEducation.Application.Commands.Tutors;

public record AvailabilitySlotDto(
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    DateOnly? SpecificDate,
    bool IsAvailable);

public record SetTutorAvailabilityCommand(
    Guid TutorId,
    List<AvailabilitySlotDto> Slots) : IRequest<Result<Guid>>;
