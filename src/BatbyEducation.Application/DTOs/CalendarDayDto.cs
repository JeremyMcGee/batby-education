namespace BatbyEducation.Application.DTOs;

/// <summary>
/// Represents a single day on the calendar with its sessions.
/// </summary>
public record CalendarDayDto(
    DateOnly Date,
    IReadOnlyList<CalendarSessionDto> Sessions);
