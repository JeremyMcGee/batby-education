using MediatR;
using BatbyEducation.Application.DTOs;
using BatbyEducation.Domain.Common;

namespace BatbyEducation.Application.Queries;

/// <summary>
/// Query to retrieve calendar sessions grouped by day, with optional tutor/student filtering.
/// Defaults to the current week (Monday–Sunday) if no date range is specified.
/// </summary>
public record GetCalendarQuery(
    DateOnly? StartDate = null,
    DateOnly? EndDate = null,
    Guid? TutorId = null,
    Guid? StudentId = null) : IRequest<Result<CalendarDto>>;
