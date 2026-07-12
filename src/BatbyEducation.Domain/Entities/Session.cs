using BatbyEducation.Domain.Common;
using BatbyEducation.Domain.Enumerations;
using BatbyEducation.Domain.Events;
using BatbyEducation.Domain.Exceptions;

namespace BatbyEducation.Domain.Entities;

/// <summary>
/// Represents a tutoring session with state machine behavior governing lifecycle transitions.
/// </summary>
public class Session : Entity
{
    public Guid TutorId { get; private set; }
    public Guid StudentId { get; private set; }
    public string Subject { get; private set; } = string.Empty;
    public DateOnly SessionDate { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public int ScheduledDurationMinutes { get; private set; }
    public int? ActualDurationMinutes { get; private set; }
    public SessionStatus Status { get; private set; }
    public Guid? RescheduledFromId { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public bool IsLateCancellation { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public decimal? RateOverride { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Session() { }

    /// <summary>
    /// Creates a new Session after validating inputs.
    /// </summary>
    public static Result<Session> Create(
        Guid tutorId,
        Guid studentId,
        string subject,
        DateOnly sessionDate,
        TimeOnly startTime,
        int scheduledDurationMinutes,
        Guid? rescheduledFromId = null,
        decimal? rateOverride = null)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(subject))
        {
            errors.Add(new ValidationError("Subject", "Subject must not be empty"));
        }

        if (scheduledDurationMinutes < 15 || scheduledDurationMinutes > 240)
        {
            errors.Add(new ValidationError("ScheduledDurationMinutes", "Duration must be between 15 and 240 minutes"));
        }

        if (rateOverride.HasValue && (rateOverride.Value < 0.01m || rateOverride.Value > 999.99m))
        {
            errors.Add(new ValidationError("RateOverride", "Rate override must be between £0.01 and £999.99"));
        }

        if (errors.Count > 0)
        {
            return Result<Session>.Failure(errors);
        }

        var session = new Session
        {
            Id = Guid.NewGuid(),
            TutorId = tutorId,
            StudentId = studentId,
            Subject = subject,
            SessionDate = sessionDate,
            StartTime = startTime,
            ScheduledDurationMinutes = scheduledDurationMinutes,
            RescheduledFromId = rescheduledFromId,
            RateOverride = rateOverride,
            Status = SessionStatus.Scheduled,
            IsLateCancellation = false,
            CreatedAt = DateTime.UtcNow
        };

        return Result<Session>.Success(session);
    }

    /// <summary>
    /// Sets or clears the rate override. Only valid for sessions in Scheduled status.
    /// </summary>
    public Result<Session> SetRateOverride(decimal? rate)
    {
        if (Status != SessionStatus.Scheduled)
            return Result<Session>.Failure("Status", "Rate override can only be set for sessions with status 'Scheduled'");

        if (rate.HasValue && (rate.Value < 0.01m || rate.Value > 999.99m))
            return Result<Session>.Failure("RateOverride", "Rate override must be between £0.01 and £999.99");

        RateOverride = rate;
        return Result<Session>.Success(this);
    }

    /// <summary>
    /// Cancels the session. Only valid from Scheduled status.
    /// Sets IsLateCancellation if cancellation is within 24 hours of the session start.
    /// </summary>
    public void Cancel(DateTime cancellationTime)
    {
        if (Status != SessionStatus.Scheduled)
        {
            throw new InvalidStateTransitionException(Status.ToString(), "cancel");
        }

        Status = SessionStatus.Cancelled;
        CancelledAt = cancellationTime;

        var sessionStart = SessionDate.ToDateTime(StartTime);
        if ((sessionStart - cancellationTime).TotalHours < 24)
        {
            IsLateCancellation = true;
        }
    }

    /// <summary>
    /// Completes the session. Only valid from Scheduled or PendingConfirmation status.
    /// Raises a SessionCompletedEvent domain event.
    /// </summary>
    public void Complete(int? actualDurationMinutes)
    {
        if (Status == SessionStatus.Cancelled || Status == SessionStatus.Rescheduled)
        {
            throw new InvalidStateTransitionException(Status.ToString(), "complete");
        }

        if (Status != SessionStatus.Scheduled && Status != SessionStatus.PendingConfirmation)
        {
            throw new InvalidStateTransitionException(Status.ToString(), "complete");
        }

        if (actualDurationMinutes.HasValue)
        {
            if (actualDurationMinutes.Value < 15 || actualDurationMinutes.Value > 240)
            {
                throw new DomainException(
                    "Actual duration must be between 15 and 240 minutes",
                    "INVALID_DURATION");
            }
        }

        ActualDurationMinutes = actualDurationMinutes ?? ScheduledDurationMinutes;
        Status = SessionStatus.Completed;
        CompletedAt = DateTime.UtcNow;

        AddDomainEvent(new SessionCompletedEvent(
            Id,
            StudentId,
            TutorId,
            SessionDate,
            ActualDurationMinutes.Value));
    }

    /// <summary>
    /// Marks the session as rescheduled.
    /// </summary>
    public void MarkAsRescheduled()
    {
        Status = SessionStatus.Rescheduled;
    }

    /// <summary>
    /// Flags the session as requiring rescheduling.
    /// </summary>
    public void FlagAsRequiresRescheduling()
    {
        Status = SessionStatus.RequiresRescheduling;
    }

    /// <summary>
    /// Flags the session as pending confirmation.
    /// </summary>
    public void FlagAsPendingConfirmation()
    {
        Status = SessionStatus.PendingConfirmation;
    }
}
