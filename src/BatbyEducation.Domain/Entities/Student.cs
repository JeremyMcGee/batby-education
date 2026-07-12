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
    public Guid? DefaultTutorId { get; private set; }
    public string? DefaultSubject { get; private set; }
    public DayOfWeek? DefaultDay { get; private set; }
    public TimeOnly? DefaultStartTime { get; private set; }
    public bool IsActive { get; private set; } = true;
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
        decimal? hourlyRate = null,
        Guid? defaultTutorId = null,
        string? defaultSubject = null,
        DayOfWeek? defaultDay = null,
        TimeOnly? defaultStartTime = null)
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
            DefaultTutorId = defaultTutorId,
            DefaultSubject = defaultSubject,
            DefaultDay = defaultDay,
            DefaultStartTime = defaultStartTime,
            IsActive = true,
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
        decimal? hourlyRate = null,
        Guid? defaultTutorId = null,
        string? defaultSubject = null,
        DayOfWeek? defaultDay = null,
        TimeOnly? defaultStartTime = null)
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

        if (DefaultTutorId != defaultTutorId)
        {
            var oldValue = DefaultTutorId.HasValue ? DefaultTutorId.Value.ToString() : "Not set";
            var newValue = defaultTutorId.HasValue ? defaultTutorId.Value.ToString() : "Not set";
            _auditHistory.Add(AuditEntry.Create(Id, nameof(Student), nameof(DefaultTutorId), oldValue, newValue));
            DefaultTutorId = defaultTutorId;
        }

        if (DefaultSubject != defaultSubject)
        {
            var oldValue = DefaultSubject ?? "Not set";
            var newValue = defaultSubject ?? "Not set";
            _auditHistory.Add(AuditEntry.Create(Id, nameof(Student), nameof(DefaultSubject), oldValue, newValue));
            DefaultSubject = defaultSubject;
        }

        if (DefaultDay != defaultDay)
        {
            var oldValue = DefaultDay.HasValue ? DefaultDay.Value.ToString() : "Not set";
            var newValue = defaultDay.HasValue ? defaultDay.Value.ToString() : "Not set";
            _auditHistory.Add(AuditEntry.Create(Id, nameof(Student), nameof(DefaultDay), oldValue, newValue));
            DefaultDay = defaultDay;
        }

        if (DefaultStartTime != defaultStartTime)
        {
            var oldValue = DefaultStartTime.HasValue ? DefaultStartTime.Value.ToString("HH:mm") : "Not set";
            var newValue = defaultStartTime.HasValue ? defaultStartTime.Value.ToString("HH:mm") : "Not set";
            _auditHistory.Add(AuditEntry.Create(Id, nameof(Student), nameof(DefaultStartTime), oldValue, newValue));
            DefaultStartTime = defaultStartTime;
        }

        UpdatedAt = now;

        return Result<Student>.Success(this);
    }

    /// <summary>
    /// Deactivates the student so they no longer appear in pickers.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reactivates the student so they appear in pickers again.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
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
