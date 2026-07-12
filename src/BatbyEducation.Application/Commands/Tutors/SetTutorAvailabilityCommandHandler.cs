using BatbyEducation.Domain.Common;
using BatbyEducation.Domain.Entities;
using BatbyEducation.Domain.Interfaces;
using MediatR;

namespace BatbyEducation.Application.Commands.Tutors;

public class SetTutorAvailabilityCommandHandler : IRequestHandler<SetTutorAvailabilityCommand, Result<Guid>>
{
    private readonly ITutorRepository _tutorRepository;
    private readonly ITutorAvailabilityRepository _availabilityRepository;
    private readonly ISessionRepository _sessionRepository;

    public SetTutorAvailabilityCommandHandler(
        ITutorRepository tutorRepository,
        ITutorAvailabilityRepository availabilityRepository,
        ISessionRepository sessionRepository)
    {
        _tutorRepository = tutorRepository;
        _availabilityRepository = availabilityRepository;
        _sessionRepository = sessionRepository;
    }

    public async Task<Result<Guid>> Handle(SetTutorAvailabilityCommand request, CancellationToken cancellationToken)
    {
        var tutor = await _tutorRepository.GetByIdAsync(request.TutorId);
        if (tutor is null)
        {
            return Result<Guid>.Failure("TutorId", "Tutor not found");
        }

        // Get existing availability to check for bookings in removed slots
        var existingSlots = await _availabilityRepository.GetByTutorIdAsync(request.TutorId);

        // Check for existing bookings that conflict with removed availability
        // Look 30 days ahead for affected sessions
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var endDate = startDate.AddDays(30);
        var existingSessions = await _sessionRepository.GetByTutorAndDateRangeAsync(
            request.TutorId, startDate, endDate);

        // Flag sessions that fall within removed availability as RequiresRescheduling
        foreach (var session in existingSessions)
        {
            var sessionEnd = session.StartTime.AddMinutes(session.ScheduledDurationMinutes);
            var isStillCovered = request.Slots.Any(slot =>
                slot.DayOfWeek == session.SessionDate.DayOfWeek &&
                slot.StartTime <= session.StartTime &&
                slot.EndTime >= sessionEnd &&
                slot.IsAvailable);

            if (!isStillCovered && session.Status == Domain.Enumerations.SessionStatus.Scheduled)
            {
                session.FlagAsRequiresRescheduling();
                await _sessionRepository.UpdateAsync(session);
            }
        }

        // Remove old availability and save new slots
        await _availabilityRepository.RemoveByTutorIdAsync(request.TutorId);

        var newAvailabilities = new List<TutorAvailability>();
        foreach (var slot in request.Slots)
        {
            var availabilityResult = TutorAvailability.Create(
                request.TutorId,
                slot.DayOfWeek,
                slot.StartTime,
                slot.EndTime,
                slot.SpecificDate,
                slot.IsAvailable);

            if (!availabilityResult.IsSuccess)
            {
                return Result<Guid>.Failure(availabilityResult.Errors);
            }

            newAvailabilities.Add(availabilityResult.Value!);
        }

        await _availabilityRepository.AddRangeAsync(newAvailabilities);

        return Result<Guid>.Success(request.TutorId);
    }
}
