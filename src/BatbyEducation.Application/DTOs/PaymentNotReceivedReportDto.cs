namespace BatbyEducation.Application.DTOs;

/// <summary>
/// Report containing sessions where bank transfer payment has not been received on time.
/// </summary>
public record PaymentNotReceivedReportDto(
    List<PaymentNotReceivedEntryDto> UpcomingSessions,
    List<PaymentNotReceivedEntryDto> PastDueSessions);

/// <summary>
/// Represents a single session entry in the payment-not-received report.
/// </summary>
public record PaymentNotReceivedEntryDto(
    string StudentName,
    string GuardianName,
    DateOnly SessionDate,
    decimal ExpectedAmount,
    int DaysSinceDeadline);
