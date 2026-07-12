using MediatR;
using BatbyEducation.Application.DTOs;
using BatbyEducation.Domain.Enumerations;
using BatbyEducation.Domain.Interfaces;

namespace BatbyEducation.Application.Queries;

/// <summary>
/// Handles the GetTutorEarningsQuery by grouping completed sessions by tutor and calculating hours and revenue.
/// </summary>
public class GetTutorEarningsQueryHandler : IRequestHandler<GetTutorEarningsQuery, List<TutorEarningsDto>>
{
    private readonly ISessionRepository _sessionRepository;
    private readonly ITutorRepository _tutorRepository;

    public GetTutorEarningsQueryHandler(
        ISessionRepository sessionRepository,
        ITutorRepository tutorRepository)
    {
        _sessionRepository = sessionRepository;
        _tutorRepository = tutorRepository;
    }

    public async Task<List<TutorEarningsDto>> Handle(GetTutorEarningsQuery request, CancellationToken cancellationToken)
    {
        // Get all sessions in range
        var sessions = await _sessionRepository.GetByDateRangeAsync(request.StartDate, request.EndDate);

        // Filter to completed sessions only
        var completedSessions = sessions
            .Where(s => s.Status == SessionStatus.Completed)
            .ToList();

        // Group by tutor
        var groupedByTutor = completedSessions.GroupBy(s => s.TutorId);

        var results = new List<TutorEarningsDto>();

        foreach (var group in groupedByTutor)
        {
            var tutor = await _tutorRepository.GetByIdAsync(group.Key);
            var tutorName = tutor?.Name ?? "Unknown";
            var hourlyRate = tutor?.HourlyRate ?? 0m;

            // Sum actual duration in hours
            var totalHours = group.Sum(s => (s.ActualDurationMinutes ?? s.ScheduledDurationMinutes) / 60m);

            // Calculate invoiced amount (hours × rate)
            var totalInvoicedAmount = totalHours * hourlyRate;

            results.Add(new TutorEarningsDto(tutorName, totalHours, totalInvoicedAmount));
        }

        return results;
    }
}
