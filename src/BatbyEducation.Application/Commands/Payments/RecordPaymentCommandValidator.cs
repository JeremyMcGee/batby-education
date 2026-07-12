using FluentValidation;
using BatbyEducation.Domain.Enumerations;

namespace BatbyEducation.Application.Commands.Payments;

/// <summary>
/// FluentValidation validator for RecordPaymentCommand.
/// Ensures basic input constraints are met before the handler processes the command.
/// </summary>
public class RecordPaymentCommandValidator : AbstractValidator<RecordPaymentCommand>
{
    public RecordPaymentCommandValidator()
    {
        RuleFor(x => x.InvoiceId)
            .NotEmpty()
            .WithMessage("InvoiceId is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.PaymentDate)
            .NotEmpty()
            .WithMessage("PaymentDate is required.");

        RuleFor(x => x.Method)
            .IsInEnum()
            .WithMessage("Method must be a valid payment method (Cash or BankTransfer).");
    }
}
