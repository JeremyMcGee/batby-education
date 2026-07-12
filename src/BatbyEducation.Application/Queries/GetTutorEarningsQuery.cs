using MediatR;
using BatbyEducation.Application.DTOs;

namespace BatbyEducation.Application.Queries;

/// <summary>
/// Query to retrieve per-tutor earnings for a specified date range.
/// Returns total hours and invoiced amount for each tutor with completed sessions in the range.
/// </summary>
public record GetTutorEarningsQuery(
    DateOnly StartDate,
    DateOnly EndDate) : IRequest<List<TutorEarningsDto>>;
