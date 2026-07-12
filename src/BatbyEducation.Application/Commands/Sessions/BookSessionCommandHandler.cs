using MediatR;
using BatbyEducation.Domain.Common;
using BatbyEducation.Domain.Entities;
using BatbyEducation.Domain.Interfaces;

namespace BatbyEducation.Application.Commands.Sessions;

/// <summary>
/// Handles the BookSessionCommand by validating tutor availability, checking for conflicts,
/// and creating the session if all checks pass.
/// </summary>
public class BookSessionCommandHandler : IRequestHandler<BookSessionCommand, Result<Guid>>
{
    private readonly ITutorRepository _tutorRepository;
    private readonly ITutorAvailabilityRepository _tutorAvailabilityRepository;
    private readonly ISessionRepository _sessionRepository;

    public BookSessionCommandHandler(
        ITutorRepository tutorRepository,
        ITutorAvailabilityRepository tutorAvailabilityRepository,
        ISessionRepository sessionRepository)
    {
        _tutorRepository = tutorRepository;
        _tutorAvailabilityRepository = tutorAvailabilityRepository;
        _sessionRepository = sessionRepository;
    }

    public async Task<Result<Guid>> Handle(BookSessionCommand request, CancellationToken cancellationToken)
    {
        // 1. Load tutor
        var tutor = await _tutorRepository.GetByIdAsync(request.TutorId);
        if (tutor is null)
        {
            return Result<Guid>.Failure("TutorId", "Tutor not found.");
        }

        // 2. Validate subject is in tutor's subjects list
        if (!tutor.Subjects.Contains(request.Subject, StringComparer.OrdinalIgnoreCase))
        {
            var availableSubjects = string.Join(", ", tutor.Subjects);
            return Result<Guid>.Failure("Subject",
                $"Tutor does not teach '{request.Subject}'. Available subjects: {availableSubjects}");
        }

        // 3. Calculate session window
        var startDateTime = request.SessionDate.ToDateTime(request.StartTime);
        var endDateTime = startDateTime.AddMinutes(request.DurationMinutes);
        var endTime = request.StartTime.AddMinutes(request.DurationMinutes);

        // 4. Check tutor availability — entire session window must fall within at least one slot
        var dayOfWeek = request.SessionDate.DayOfWeek;
        var availabilitySlots = await _tutorAvailabilityRepository.GetByTutorAndDayAsync(request.TutorId, dayOfWeek);

        var isWithinAvailability = availabilitySlots
            .Where(slot => slot.IsAvailable)
            .Any(slot => request.StartTime >= slot.StartTime && endTime <= slot.EndTime);

        if (!isWithinAvailability)
        {
            return Result<Guid>.Failure("SessionDate",
                "Tutor is not available during the requested time slot.");
        }

        // 5. Check tutor conflicts
        var tutorConflicts = await _sessionRepository.GetConflictingSessionsAsync(
            request.TutorId, startDateTime, endDateTime);

        if (tutorConflicts.Count > 0)
        {
            var conflict = tutorConflicts[0];
            var conflictStart = conflict.SessionDate.ToDateTime(conflict.StartTime);
            var conflictEnd = conflictStart.AddMinutes(conflict.ScheduledDurationMinutes);
            return Result<Guid>.Failure("StartTime",
                $"Tutor has a conflicting session on {conflict.SessionDate:yyyy-MM-dd} from {conflict.StartTime:HH:mm} to {conflictEnd:HH:mm}.");
        }

        // 6. Check student conflicts
        var studentConflicts = await _sessionRepository.GetStudentConflictsAsync(
            request.StudentId, startDateTime, endDateTime);

        if (studentConflicts.Count > 0)
        {
            var conflict = studentConflicts[0];
            var conflictStart = conflict.SessionDate.ToDateTime(conflict.StartTime);
            var conflictEnd = conflictStart.AddMinutes(conflict.ScheduledDurationMinutes);
            return Result<Guid>.Failure("StudentId",
                $"Student has a conflicting session on {conflict.SessionDate:yyyy-MM-dd} from {conflict.StartTime:HH:mm} to {conflictEnd:HH:mm}.");
        }

        // 7. Create session
        var sessionResult = Session.Create(
            request.TutorId,
            request.StudentId,
            request.Subject,
            request.SessionDate,
            request.StartTime,
            request.DurationMinutes,
            rateOverride: request.RateOverride);

        if (!sessionResult.IsSuccess)
        {
            return Result<Guid>.Failure(sessionResult.Errors);
        }

        // 8. Save and return success
        await _sessionRepository.AddAsync(sessionResult.Value!);

        return Result<Guid>.Success(sessionResult.Value!.Id);
    }
}
