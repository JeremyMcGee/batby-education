namespace BatbyEducation.Domain.Exceptions;

public class InvalidStateTransitionException : DomainException
{
    public string CurrentStatus { get; }
    public string AttemptedAction { get; }

    public InvalidStateTransitionException(string status, string action)
        : base($"Cannot {action} a session with status '{status}'", "INVALID_STATE")
    {
        CurrentStatus = status;
        AttemptedAction = action;
    }
}
