using BatbyEducation.Domain.Common;

namespace BatbyEducation.Domain.Entities;

/// <summary>
/// Represents a tutor's availability window. When SpecificDate is null, it represents
/// a recurring weekly slot. When SpecificDate is set, it represents a one-off exception.
/// </summary>
public class TutorAvailability : Entity
{
    public Guid TutorId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public DateOnly? SpecificDate { get; private set; }
    public bool IsAvailable { get; private set; }

    private TutorAvailability() { }

    /// <summary>
    /// Creates a new TutorAvailability instance with validation.
    /// </summary>
    /// <param name="tutorId">The tutor this availability belongs to.</param>
    /// <param name="day">The day of week for this slot.</param>
    /// <param name="start">Start time of the availability window.</param>
    /// <param name="end">End time of the availability window.</param>
    /// <param name="specificDate">If set, this is a one-off exception rather than recurring.</param>
    /// <param name="isAvailable">Whether the tutor is available during this window.</param>
    /// <returns>A Result containing the entity or validation errors.</returns>
    public static Result<TutorAvailability> Create(
        Guid tutorId,
        DayOfWeek day,
        TimeOnly start,
        TimeOnly end,
        DateOnly? specificDate = null,
        bool isAvailable = true)
    {
        if (end <= start)
        {
            return Result<TutorAvailability>.Failure("EndTime", "End time must be after start time.");
        }

        var durationMinutes = (end - start).TotalMinutes;
        if (durationMinutes < 30)
        {
            return Result<TutorAvailability>.Failure("Duration", "Availability slot must be at least 30 minutes.");
        }

        var availability = new TutorAvailability
        {
            Id = Guid.NewGuid(),
            TutorId = tutorId,
            DayOfWeek = day,
            StartTime = start,
            EndTime = end,
            SpecificDate = specificDate,
            IsAvailable = isAvailable
        };

        return Result<TutorAvailability>.Success(availability);
    }
}
