using MediatR;
using BatbyEducation.Application.DTOs;
using BatbyEducation.Domain.Common;
using BatbyEducation.Domain.Entities;
using BatbyEducation.Domain.Enumerations;
using BatbyEducation.Domain.Interfaces;
using BatbyEducation.Domain.ValueObjects;

namespace BatbyEducation.Application.Queries;

/// <summary>
/// Handles the GetCalendarQuery by fetching sessions within a date range,
/// resolving tutor/student names, and grouping results by day.
/// </summary>
public class GetCalendarQueryHandler : IRequestHandler<GetCalendarQuery, Result<CalendarDto>>
{
    private readonly ISessionRepository _sessionRepository;
    private readonly ITutorRepository _tutorRepository;
    private readonly IStudentRepository _studentRepository;

    public GetCalendarQueryHandler(
        ISessionRepository sessionRepository,
        ITutorRepository tutorRepository,
        IStudentRepository studentRepository)
    {
        _sessionRepository = sessionRepository;
        _tutorRepository = tutorRepository;
        _studentRepository = studentRepository;
    }

    public async Task<Result<CalendarDto>> Handle(GetCalendarQuery request, CancellationToken cancellationToken)
    {
        // 1. Determine date range — default to current week if not provided
        var dateRange = (request.StartDate.HasValue && request.EndDate.HasValue)
            ? new DateRange(request.StartDate.Value, request.EndDate.Value)
            : DateRange.CurrentWeek();

        // 2. Query sessions based on filters
        IReadOnlyList<Session> sessions;

        if (request.TutorId.HasValue)
        {
            sessions = await _sessionRepository.GetByTutorAndDateRangeAsync(
                request.TutorId.Value, dateRange.Start, dateRange.End);
        }
        else if (request.StudentId.HasValue)
        {
            sessions = await _sessionRepository.GetByStudentAndDateRangeAsync(
                request.StudentId.Value, dateRange.Start, dateRange.End);
        }
        else
        {
            sessions = await _sessionRepository.GetByDateRangeAsync(dateRange.Start, dateRange.End);
        }

        // 3. Resolve tutor and student names (batch to avoid repeated lookups)
        var tutorIds = sessions.Select(s => s.TutorId).Distinct().ToList();
        var studentIds = sessions.Select(s => s.StudentId).Distinct().ToList();

        var tutorNames = new Dictionary<Guid, string>();
        foreach (var tutorId in tutorIds)
        {
            var tutor = await _tutorRepository.GetByIdAsync(tutorId);
            tutorNames[tutorId] = tutor?.Name ?? "Unknown Tutor";
        }

        var studentNames = new Dictionary<Guid, string>();
        foreach (var studentId in studentIds)
        {
            var student = await _studentRepository.GetByIdAsync(studentId);
            studentNames[studentId] = student?.Name ?? "Unknown Student";
        }

        // 4. Group sessions by date and map to DTOs
        var days = sessions
            .GroupBy(s => s.SessionDate)
            .OrderBy(g => g.Key)
            .Select(g => new CalendarDayDto(
                g.Key,
                g.OrderBy(s => s.StartTime)
                 .Select(s => new CalendarSessionDto(
                     s.Id,
                     tutorNames[s.TutorId],
                     studentNames[s.StudentId],
                     s.Subject,
                     s.StartTime,
                     s.ScheduledDurationMinutes,
                     s.Status.ToString(),
                     GetStatusColour(s.Status)))
                 .ToList()))
            .ToList();

        var calendarDto = new CalendarDto(dateRange.Start, dateRange.End, days);
        return Result<CalendarDto>.Success(calendarDto);
    }

    private static string GetStatusColour(SessionStatus status) => status switch
    {
        SessionStatus.Scheduled => "blue",
        SessionStatus.Completed => "green",
        SessionStatus.Cancelled => "grey",
        SessionStatus.PendingConfirmation => "amber",
        SessionStatus.RequiresRescheduling => "red",
        SessionStatus.Rescheduled => "grey",
        _ => "grey"
    };
}
