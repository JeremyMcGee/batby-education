namespace BatbyEducation.Application.DTOs;

/// <summary>
/// Represents a single session entry on the calendar.
/// </summary>
public record CalendarSessionDto(
    Guid SessionId,
    string TutorName,
    string StudentName,
    string Subject,
    TimeOnly StartTime,
    int DurationMinutes,
    string Status,
    string StatusColour);
