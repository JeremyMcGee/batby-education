namespace BatbyEducation.Application.DTOs;

/// <summary>
/// Represents the full calendar view with a date range and grouped days.
/// </summary>
public record CalendarDto(
    DateOnly StartDate,
    DateOnly EndDate,
    IReadOnlyList<CalendarDayDto> Days);
