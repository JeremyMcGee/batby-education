using BatbyEducation.Domain.Common;
using BatbyEducation.Domain.ValueObjects;

namespace BatbyEducation.Domain.Entities;

/// <summary>
/// Represents a tutor who conducts tutoring sessions.
/// Stores tutor details, subjects taught, and hourly rate.
/// </summary>
public class Tutor : Entity
{
    public string Name { get; private set; } = default!;
    public EmailAddress Email { get; private set; } = default!;
    public IReadOnlyList<string> Subjects { get; private set; } = default!;
    public decimal HourlyRate { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Tutor() { }

    /// <summary>
    /// Creates a new Tutor with validated input fields.
    /// </summary>
    /// <param name="name">Tutor name (required, max 100 characters).</param>
    /// <param name="email">Tutor email address (must be valid format).</param>
    /// <param name="subjects">List of subjects taught (1–20 items).</param>
    /// <param name="hourlyRate">Hourly rate in GBP (£0.01–£999.99).</param>
    /// <returns>A Result containing the created Tutor or validation errors.</returns>
    public static Result<Tutor> Create(string name, string email, List<string> subjects, decimal hourlyRate)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(name))
            errors.Add(new ValidationError("Name", "Name is required"));
        else if (name.Length > 100)
            errors.Add(new ValidationError("Name", "Name must not exceed 100 characters"));

        if (!EmailAddress.IsValid(email))
            errors.Add(new ValidationError("Email", "Email format is invalid"));

        if (subjects is null or { Count: 0 })
            errors.Add(new ValidationError("Subjects", "At least one subject is required"));
        else if (subjects.Count > 20)
            errors.Add(new ValidationError("Subjects", "No more than 20 subjects are allowed"));

        if (hourlyRate < 0.01m)
            errors.Add(new ValidationError("HourlyRate", "Hourly rate must be at least £0.01"));
        else if (hourlyRate > 999.99m)
            errors.Add(new ValidationError("HourlyRate", "Hourly rate must not exceed £999.99"));

        if (errors.Count > 0)
            return Result<Tutor>.Failure(errors);

        var tutor = new Tutor
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = new EmailAddress(email),
            Subjects = subjects!.AsReadOnly(),
            HourlyRate = hourlyRate,
            CreatedAt = DateTime.UtcNow
        };

        return Result<Tutor>.Success(tutor);
    }
}
