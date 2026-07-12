using FluentValidation;

namespace BatbyEducation.Application.Commands.Invoices;

/// <summary>
/// FluentValidation validator for GenerateInvoiceCommand.
/// Ensures basic input constraints are met before the handler processes the command.
/// </summary>
public class GenerateInvoiceCommandValidator : AbstractValidator<GenerateInvoiceCommand>
{
    public GenerateInvoiceCommandValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty()
            .WithMessage("StudentId is required.");

        RuleFor(x => x.BillingPeriodStart)
            .LessThan(x => x.BillingPeriodEnd)
            .WithMessage("BillingPeriodStart must be before BillingPeriodEnd.");

        RuleFor(x => x.LateCancellationFee)
            .GreaterThanOrEqualTo(0)
            .WithMessage("LateCancellationFee must be zero or greater.");
    }
}
