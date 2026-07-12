using BatbyEducation.Domain.Common;
using BatbyEducation.Domain.Entities;
using BatbyEducation.Domain.Enumerations;
using BatbyEducation.Domain.Interfaces;
using MediatR;

namespace BatbyEducation.Application.Commands.Sessions;

public class RescheduleSessionCommandHandler : IRequestHandler<RescheduleSessionCommand, Result<Guid>>
{
    private readonly ISessionRepository _sessionRepository;
    private readonly ITutorAvailabilityRepository _availabilityRepository;

    public RescheduleSessionCommandHandler(
        ISessionRepository sessionRepository,
        ITutorAvailabilityRepository availabilityRepository)
    {
        _sessionRepository = sessionRepository;
        _availabilityRepository = availabilityRepository;
    }

    public async Task<Result<Guid>> Handle(RescheduleSessionCommand request, CancellationToken cancellationToken)
    {
        var originalSession = await _sessionRepository.GetByIdAsync(request.OriginalSessionId);
        if (originalSession is null)
        {
            return Result<Guid>.Failure("OriginalSessionId", "Original session not found");
        }

        if (originalSession.Status != SessionStatus.Scheduled)
        {
            return Result<Guid>.Failure("Status", "Only sessions with status 'Scheduled' can be rescheduled");
        }

        // Calculate new session time window
        var newStartDateTime = request.NewDate.ToDateTime(request.NewStartTime);
        var newEndDateTime = newStartDateTime.AddMinutes(request.NewDurationMinutes);

        // Check tutor availability
        var availabilities = await _availabilityRepository.GetByTutorAndDayAsync(
            originalSession.TutorId, request.NewDate.DayOfWeek);

        var specificAvailability = await _availabilityRepository.GetByTutorAndDateAsync(
            originalSession.TutorId, request.NewDate);

        bool isWithinAvailability = false;

        // Check specific date availability first (overrides recurring)
        if (specificAvailability is not null)
        {
            if (specificAvailability.IsAvailable &&
                request.NewStartTime >= specificAvailability.StartTime &&
                request.NewStartTime.AddMinutes(request.NewDurationMinutes) <= specificAvailability.EndTime)
            {
                isWithinAvailability = true;
            }
        }
        else
        {
            // Check recurring availability
            foreach (var availability in availabilities)
            {
                if (availability.IsAvailable &&
                    request.NewStartTime >= availability.StartTime &&
                    request.NewStartTime.AddMinutes(request.NewDurationMinutes) <= availability.EndTime)
                {
                    isWithinAvailability = true;
                    break;
                }
            }
        }

        if (!isWithinAvailability)
        {
            return Result<Guid>.Failure("NewStartTime", "The requested time slot is outside the tutor's availability");
        }

        // Check tutor conflicts
        var tutorConflicts = await _sessionRepository.GetConflictingSessionsAsync(
            originalSession.TutorId, newStartDateTime, newEndDateTime);

        // Exclude the original session from conflict check
        var actualTutorConflicts = tutorConflicts
            .Where(s => s.Id != originalSession.Id)
            .ToList();

        if (actualTutorConflicts.Count > 0)
        {
            var conflict = actualTutorConflicts.First();
            return Result<Guid>.Failure("NewStartTime",
                $"Tutor has a conflicting session on {conflict.SessionDate} at {conflict.StartTime}");
        }

        // Check student conflicts
        var studentConflicts = await _sessionRepository.GetStudentConflictsAsync(
            originalSession.StudentId, newStartDateTime, newEndDateTime);

        // Exclude the original session from conflict check
        var actualStudentConflicts = studentConflicts
            .Where(s => s.Id != originalSession.Id)
            .ToList();

        if (actualStudentConflicts.Count > 0)
        {
            var conflict = actualStudentConflicts.First();
            return Result<Guid>.Failure("NewStartTime",
                $"Student has a conflicting session on {conflict.SessionDate} at {conflict.StartTime}");
        }

        // Create new session linked to original
        var newSessionResult = Session.Create(
            originalSession.TutorId,
            originalSession.StudentId,
            originalSession.Subject,
            request.NewDate,
            request.NewStartTime,
            request.NewDurationMinutes,
            rescheduledFromId: originalSession.Id);

        if (!newSessionResult.IsSuccess)
        {
            return Result<Guid>.Failure(newSessionResult.Errors);
        }

        // Mark original as rescheduled
        originalSession.MarkAsRescheduled();

        // Save both sessions
        await _sessionRepository.AddAsync(newSessionResult.Value!);
        await _sessionRepository.UpdateAsync(originalSession);

        return Result<Guid>.Success(newSessionResult.Value!.Id);
    }
}
