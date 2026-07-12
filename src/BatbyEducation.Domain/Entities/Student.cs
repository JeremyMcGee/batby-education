using BatbyEducation.Domain.Common;
using BatbyEducation.Domain.ValueObjects;

namespace BatbyEducation.Domain.Entities;

/// <summary>
/// Represents a student registered in the tutoring system.
/// </summary>
public class Student : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public EmailAddress Email { get; private set; } = null!;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string GuardianName { get; private set; } = string.Empty;
    public EmailAddress GuardianEmail { get; private set; } = null!;
    public decimal? HourlyRate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private readonly List<AuditEntry> _auditHistory = new();
    public IReadOnlyList<AuditEntry> AuditHistory => _auditHistory.AsReadOnly();

    private Student() { }

    /// <summary>
    /// Creates a new Student with full validation of required fields and email format.
    /// </summary>
    public static Result<Student> Create(
        string name,
        string email,
        string phoneNumber,
        string guardianName,
        string guardianEmail,
        decimal? hourlyRate = null)
    {
        var errors = new List<ValidationError>();

        // Validate required fields
        if (string.IsNullOrWhiteSpace(name))
            errors.Add(new ValidationError("Name", "Name is required"));
        else if (name.Length > 100)
            errors.Add(new ValidationError("Name", "Name must not exceed 100 characters"));

        if (string.IsNullOrWhiteSpace(email))
            errors.Add(new ValidationError("Email", "Email is required"));
        else if (!EmailAddress.IsValid(email))
            errors.Add(new ValidationError("Email", "Email format is invalid"));

        if (string.IsNullOrWhiteSpace(phoneNumber))
            errors.Add(new ValidationError("PhoneNumber", "Phone number is required"));

        if (string.IsNullOrWhiteSpace(guardianName))
            errors.Add(new ValidationError("GuardianName", "Guardian name is required"));
        else if (guardianName.Length > 100)
            errors.Add(new ValidationError("GuardianName", "Guardian name must not exceed 100 characters"));

        if (string.IsNullOrWhiteSpace(guardianEmail))
            errors.Add(new ValidationError("GuardianEmail", "Guardian email is required"));
        else if (!EmailAddress.IsValid(guardianEmail))
            errors.Add(new ValidationError("GuardianEmail", "Guardian email format is invalid"));

        if (hourlyRate.HasValue && (hourlyRate.Value < 0.01m || hourlyRate.Value > 999.99m))
            errors.Add(new ValidationError("HourlyRate", "Hourly rate must be between £0.01 and £999.99"));

        if (errors.Count > 0)
            return Result<Student>.Failure(errors);

        var now = DateTime.UtcNow;
        var student = new Student
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = new EmailAddress(email),
            PhoneNumber = phoneNumber,
            GuardianName = guardianName,
            GuardianEmail = new EmailAddress(guardianEmail),
            HourlyRate = hourlyRate,
            CreatedAt = now,
            UpdatedAt = now
        };

        return Result<Student>.Success(student);
    }

    /// <summary>
    /// Updates the student record, recording audit entries for any changed fields.
    /// </summary>
    public Result<Student> Update(
        string name,
        string email,
        string phoneNumber,
        string guardianName,
        string guardianEmail,
        decimal? hourlyRate = null)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(name))
            errors.Add(new ValidationError("Name", "Name is required"));
        else if (name.Length > 100)
            errors.Add(new ValidationError("Name", "Name must not exceed 100 characters"));

        if (string.IsNullOrWhiteSpace(email))
            errors.Add(new ValidationError("Email", "Email is required"));
        else if (!EmailAddress.IsValid(email))
            errors.Add(new ValidationError("Email", "Email format is invalid"));

        if (string.IsNullOrWhiteSpace(phoneNumber))
            errors.Add(new ValidationError("PhoneNumber", "Phone number is required"));

        if (string.IsNullOrWhiteSpace(guardianName))
            errors.Add(new ValidationError("GuardianName", "Guardian name is required"));
        else if (guardianName.Length > 100)
            errors.Add(new ValidationError("GuardianName", "Guardian name must not exceed 100 characters"));

        if (string.IsNullOrWhiteSpace(guardianEmail))
            errors.Add(new ValidationError("GuardianEmail", "Guardian email is required"));
        else if (!EmailAddress.IsValid(guardianEmail))
            errors.Add(new ValidationError("GuardianEmail", "Guardian email format is invalid"));

        if (hourlyRate.HasValue && (hourlyRate.Value < 0.01m || hourlyRate.Value > 999.99m))
            errors.Add(new ValidationError("HourlyRate", "Hourly rate must be between £0.01 and £999.99"));

        if (errors.Count > 0)
            return Result<Student>.Failure(errors);

        var now = DateTime.UtcNow;

        if (Name != name)
        {
            _auditHistory.Add(AuditEntry.Create(Id, nameof(Student), nameof(Name), Name, name));
            Name = name;
        }

        var newEmail = new EmailAddress(email);
        if (Email.Value != newEmail.Value)
        {
            _auditHistory.Add(AuditEntry.Create(Id, nameof(Student), nameof(Email), Email.Value, newEmail.Value));
            Email = newEmail;
        }

        if (PhoneNumber != phoneNumber)
        {
            _auditHistory.Add(AuditEntry.Create(Id, nameof(Student), nameof(PhoneNumber), PhoneNumber, phoneNumber));
            PhoneNumber = phoneNumber;
        }

        if (GuardianName != guardianName)
        {
            _auditHistory.Add(AuditEntry.Create(Id, nameof(Student), nameof(GuardianName), GuardianName, guardianName));
            GuardianName = guardianName;
        }

        var newGuardianEmail = new EmailAddress(guardianEmail);
        if (GuardianEmail.Value != newGuardianEmail.Value)
        {
            _auditHistory.Add(AuditEntry.Create(Id, nameof(Student), nameof(GuardianEmail), GuardianEmail.Value, newGuardianEmail.Value));
            GuardianEmail = newGuardianEmail;
        }

        if (HourlyRate != hourlyRate)
        {
            var oldValue = HourlyRate.HasValue ? HourlyRate.Value.ToString() : "Not set";
            var newValue = hourlyRate.HasValue ? hourlyRate.Value.ToString() : "Not set";
            _auditHistory.Add(AuditEntry.Create(Id, nameof(Student), nameof(HourlyRate), oldValue, newValue));
            HourlyRate = hourlyRate;
        }

        UpdatedAt = now;

        return Result<Student>.Success(this);
    }

    /// <summary>
    /// Sets or clears the student's hourly rate, recording an audit entry for the change.
    /// </summary>
    public Result<Student> SetHourlyRate(decimal? rate)
    {
        if (rate.HasValue && (rate.Value < 0.01m || rate.Value > 999.99m))
            return Result<Student>.Failure("HourlyRate", "Hourly rate must be between £0.01 and £999.99");

        var oldValue = HourlyRate.HasValue ? HourlyRate.Value.ToString() : "Not set";
        var newValue = rate.HasValue ? rate.Value.ToString() : "Not set";
        _auditHistory.Add(AuditEntry.Create(Id, nameof(Student), nameof(HourlyRate), oldValue, newValue));
        HourlyRate = rate;

        return Result<Student>.Success(this);
    }
}
