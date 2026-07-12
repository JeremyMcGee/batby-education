using System.Text.RegularExpressions;
using BatbyEducation.Domain.Exceptions;

namespace BatbyEducation.Domain.ValueObjects;

public record EmailAddress
{
    public string Value { get; }

    public EmailAddress(string value)
    {
        if (!IsValid(value))
            throw new DomainException("Invalid email format");
        Value = value;
    }

    public static bool IsValid(string email) =>
        !string.IsNullOrWhiteSpace(email) &&
        Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
}
