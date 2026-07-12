namespace BatbyEducation.Domain.Common;

/// <summary>
/// Represents the outcome of a domain operation, carrying either a success value or validation errors.
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public IReadOnlyList<ValidationError> Errors { get; }

    private Result(T? value, bool isSuccess, IReadOnlyList<ValidationError>? errors = null)
    {
        IsSuccess = isSuccess;
        Value = value;
        Errors = errors ?? Array.Empty<ValidationError>();
    }

    public static Result<T> Success(T value) => new(value, true);

    public static Result<T> Failure(IReadOnlyList<ValidationError> errors) => new(default, false, errors);

    public static Result<T> Failure(string field, string message) =>
        new(default, false, new[] { new ValidationError(field, message) });
}

/// <summary>
/// Represents a validation error with the field name and error message.
/// </summary>
public record ValidationError(string Field, string Message);
