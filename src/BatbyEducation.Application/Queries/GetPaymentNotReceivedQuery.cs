using MediatR;
using BatbyEducation.Application.DTOs;

namespace BatbyEducation.Application.Queries;

/// <summary>
/// Query to retrieve sessions flagged as "Payment Not Received"
/// (bank transfer sessions where payment was not received 24h before session start).
/// Returns upcoming and past-due sessions sorted by session date.
/// </summary>
public record GetPaymentNotReceivedQuery : IRequest<PaymentNotReceivedReportDto>;
