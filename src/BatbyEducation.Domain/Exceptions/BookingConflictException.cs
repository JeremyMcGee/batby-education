namespace BatbyEducation.Domain.Exceptions;

public class BookingConflictException : DomainException
{
    public DateTime ConflictStart { get; }
    public DateTime ConflictEnd { get; }

    public BookingConflictException(DateTime start, DateTime end)
        : base($"Conflicting session exists from {start} to {end}", "BOOKING_CONFLICT")
    {
        ConflictStart = start;
        ConflictEnd = end;
    }
}
