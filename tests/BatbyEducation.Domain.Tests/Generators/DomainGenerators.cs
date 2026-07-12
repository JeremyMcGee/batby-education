using BatbyEducation.Domain.Enumerations;
using FsCheck;
using FsCheck.Fluent;
using Gen = FsCheck.Fluent.Gen;

namespace BatbyEducation.Domain.Tests.Generators;

/// <summary>
/// Custom FsCheck Arbitrary generators for domain types used in property-based testing.
/// Use these generators via Prop.ForAll or by referencing in property test setup.
/// </summary>
public static class DomainGenerators
{
    private static readonly string[] Tlds = ["com", "co.uk", "org", "net", "edu"];
    private static readonly char[] AlphanumericChars = "abcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
    private static readonly char[] AlphaChars = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
    private static readonly char[] NameChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ".ToCharArray();
    private static readonly char[] DigitChars = "0123456789".ToCharArray();

    private static readonly string[] SubjectPool =
    [
        "Maths", "English", "Science", "History", "Geography",
        "Physics", "Chemistry", "Biology", "French", "Spanish",
        "Music", "Art", "Computing", "Drama", "Economics",
        "Business", "Psychology", "Sociology", "Philosophy", "Law"
    ];

    // ──────────────────────────────────────────────────────────────────────────
    // 1. ValidEmail
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Generates valid email strings like {word}@{word}.{tld}
    /// where word is 1-20 alphanumeric chars and tld is from a known set.
    /// </summary>
    public static Arbitrary<string> ValidEmail() =>
        Arb.ToArbitrary(ValidEmailGen());

    // ──────────────────────────────────────────────────────────────────────────
    // 2. InvalidEmail
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Generates strings that are NOT valid emails (missing @, missing domain, spaces, empty, etc.)
    /// </summary>
    public static Arbitrary<string> InvalidEmail() =>
        Arb.ToArbitrary(InvalidEmailGen());

    // ──────────────────────────────────────────────────────────────────────────
    // 3. ValidStudentName / ValidTutorName
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Generates valid names: 1-100 character strings (alphanumeric + spaces, no leading/trailing space).
    /// </summary>
    public static Arbitrary<string> ValidStudentName() =>
        Arb.ToArbitrary(ValidNameGen());

    /// <summary>
    /// Generates valid tutor names: 1-100 character strings (alphanumeric + spaces, no leading/trailing space).
    /// </summary>
    public static Arbitrary<string> ValidTutorName() =>
        Arb.ToArbitrary(ValidNameGen());

    // ──────────────────────────────────────────────────────────────────────────
    // 4. ValidPhoneNumber
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Generates valid phone numbers like "+44" followed by 10 digits.
    /// </summary>
    public static Arbitrary<string> ValidPhoneNumber() =>
        Arb.ToArbitrary(ValidPhoneNumberGen());

    // ──────────────────────────────────────────────────────────────────────────
    // 5. ValidSubjectList
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Generates lists of 1-20 non-empty strings from the subject pool.
    /// </summary>
    public static Arbitrary<List<string>> ValidSubjectList() =>
        Arb.ToArbitrary(ValidSubjectListGen());

    // ──────────────────────────────────────────────────────────────────────────
    // 6. ValidHourlyRate
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Generates a decimal between 0.01 and 999.99.
    /// </summary>
    public static Arbitrary<decimal> ValidHourlyRate() =>
        Arb.ToArbitrary(ValidHourlyRateGen());

    // ──────────────────────────────────────────────────────────────────────────
    // 7. ValidDuration
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Generates an integer between 15 and 240 (session duration in minutes).
    /// </summary>
    public static Arbitrary<int> ValidDuration() =>
        Arb.ToArbitrary(Gen.Choose(15, 240));

    // ──────────────────────────────────────────────────────────────────────────
    // 8. InvalidDuration
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Generates an integer outside [15, 240] range.
    /// </summary>
    public static Arbitrary<int> InvalidDuration() =>
        Arb.ToArbitrary(
            Gen.OneOf(
                Gen.Choose(-100, 14),
                Gen.Choose(241, 1000)
            ));

    // ──────────────────────────────────────────────────────────────────────────
    // 9. ValidDateRange
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Generates DateOnly pairs where start &lt;= end, within reasonable bounds (2020-2030).
    /// </summary>
    public static Arbitrary<(DateOnly Start, DateOnly End)> ValidDateRange() =>
        Arb.ToArbitrary(ValidDateRangeGen());

    // ──────────────────────────────────────────────────────────────────────────
    // 10. TaxYearStartYear
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Generates an integer between 2020 and 2030.
    /// </summary>
    public static Arbitrary<int> TaxYearStartYear() =>
        Arb.ToArbitrary(Gen.Choose(2020, 2030));

    // ──────────────────────────────────────────────────────────────────────────
    // 11. ValidSessionStatus
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Generates a random SessionStatus enum value.
    /// </summary>
    public static Arbitrary<SessionStatus> ValidSessionStatus() =>
        Arb.ToArbitrary(Gen.Elements(Enum.GetValues<SessionStatus>()));

    // ──────────────────────────────────────────────────────────────────────────
    // 12. ValidPaymentMethod
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Generates a random PaymentMethod enum value.
    /// </summary>
    public static Arbitrary<PaymentMethod> ValidPaymentMethod() =>
        Arb.ToArbitrary(Gen.Elements(Enum.GetValues<PaymentMethod>()));

    // ──────────────────────────────────────────────────────────────────────────
    // 13. ValidPaymentAmount
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Generates a decimal between 0.01 and 10000.00.
    /// </summary>
    public static Arbitrary<decimal> ValidPaymentAmount() =>
        Arb.ToArbitrary(
            Gen.Choose(1, 1000000).Select(pence => pence / 100m));

    // ──────────────────────────────────────────────────────────────────────────
    // 14. TimeSlot
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Generates a pair of TimeOnly values where end > start and duration >= 30 minutes.
    /// </summary>
    public static Arbitrary<(TimeOnly Start, TimeOnly End)> TimeSlot() =>
        Arb.ToArbitrary(TimeSlotGen());

    // ──────────────────────────────────────────────────────────────────────────
    // 15. ValidDayOfWeek
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Generates a random DayOfWeek enum value.
    /// </summary>
    public static Arbitrary<DayOfWeek> ValidDayOfWeek() =>
        Arb.ToArbitrary(Gen.Elements(Enum.GetValues<DayOfWeek>()));

    // ──────────────────────────────────────────────────────────────────────────
    // Gen-only accessors for composing in other generators
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the Gen for valid emails (useful for composing in other generators).
    /// </summary>
    public static Gen<string> ValidEmailGen() =>
        Gen.Choose(1, 20).SelectMany(nameLen =>
            Gen.ArrayOf(Gen.Elements(AlphanumericChars), nameLen).SelectMany(nameChars =>
                Gen.Choose(1, 10).SelectMany(domainLen =>
                    Gen.ArrayOf(Gen.Elements(AlphaChars), domainLen).SelectMany(domainChars =>
                        Gen.Elements(Tlds).Select(tld =>
                            $"{new string(nameChars)}@{new string(domainChars)}.{tld}")))));

    /// <summary>
    /// Returns the Gen for invalid emails (useful for composing in other generators).
    /// </summary>
    public static Gen<string> InvalidEmailGen() =>
        Gen.OneOf(
            // Empty string
            Gen.Constant(""),
            // Whitespace only
            Gen.Constant("   "),
            // Missing @ symbol - just alphanumeric
            Gen.Choose(1, 10).SelectMany(len =>
                Gen.ArrayOf(Gen.Elements(AlphanumericChars), len)
                    .Select(chars => new string(chars))),
            // Missing domain (ends with @)
            Gen.Choose(1, 10).SelectMany(len =>
                Gen.ArrayOf(Gen.Elements(AlphanumericChars), len)
                    .Select(chars => $"{new string(chars)}@")),
            // Contains spaces in local part
            Gen.Choose(1, 5).SelectMany(len =>
                Gen.ArrayOf(Gen.Elements(AlphanumericChars), len)
                    .Select(chars => $"{new string(chars)} name@domain.com")),
            // Missing TLD (no dot after @)
            Gen.Choose(1, 5).SelectMany(len =>
                Gen.ArrayOf(Gen.Elements(AlphanumericChars), len)
                    .Select(chars => $"{new string(chars)}@nodot")),
            // Double @
            Gen.Choose(1, 5).SelectMany(len =>
                Gen.ArrayOf(Gen.Elements(AlphanumericChars), len)
                    .Select(chars => $"{new string(chars)}@@domain.com"))
        );

    /// <summary>
    /// Returns the Gen for valid names (useful for composing in other generators).
    /// </summary>
    public static Gen<string> ValidNameGen() =>
        Gen.Choose(1, 100).SelectMany(len =>
            Gen.Elements("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray()).SelectMany(firstChar =>
                Gen.ArrayOf(Gen.Elements(NameChars), Math.Max(0, len - 2)).SelectMany(middleChars =>
                    Gen.Elements("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray())
                        .Select(lastChar =>
                            len == 1
                                ? firstChar.ToString()
                                : len == 2
                                    ? $"{firstChar}{lastChar}"
                                    : $"{firstChar}{new string(middleChars)}{lastChar}"))));

    /// <summary>
    /// Returns the Gen for valid phone numbers (useful for composing in other generators).
    /// </summary>
    public static Gen<string> ValidPhoneNumberGen() =>
        Gen.ArrayOf(Gen.Elements(DigitChars), 10)
            .Select(digits => $"+44{new string(digits)}");

    /// <summary>
    /// Returns the Gen for valid subject lists (useful for composing in other generators).
    /// </summary>
    public static Gen<List<string>> ValidSubjectListGen() =>
        Gen.Choose(1, 20).SelectMany(count =>
            Gen.ArrayOf(Gen.Elements(SubjectPool), count)
                .Select(subjects => subjects.Distinct().ToList()))
        .Where(list => list.Count >= 1);

    /// <summary>
    /// Returns the Gen for valid hourly rates (useful for composing in other generators).
    /// </summary>
    public static Gen<decimal> ValidHourlyRateGen() =>
        Gen.Choose(1, 99999).Select(pence => pence / 100m);

    /// <summary>
    /// Returns the Gen for valid date ranges (useful for composing in other generators).
    /// </summary>
    public static Gen<(DateOnly Start, DateOnly End)> ValidDateRangeGen() =>
        Gen.Choose(0, 3650).SelectMany(startDayOffset =>
            Gen.Choose(0, 365).Select(duration =>
            {
                var baseDate = new DateOnly(2020, 1, 1);
                var start = baseDate.AddDays(startDayOffset);
                var end = start.AddDays(duration);
                return (start, end);
            }))
        .Where(range => range.end.Year <= 2030);

    /// <summary>
    /// Returns the Gen for valid time slots (useful for composing in other generators).
    /// </summary>
    public static Gen<(TimeOnly Start, TimeOnly End)> TimeSlotGen() =>
        Gen.Choose(0, 23 * 60 - 30).SelectMany(startMinutes =>
            Gen.Choose(30, Math.Min(240, 24 * 60 - startMinutes))
                .Select(durationMinutes =>
                {
                    var start = TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(startMinutes));
                    var end = TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(startMinutes + durationMinutes));
                    return (start, end);
                }));
}
