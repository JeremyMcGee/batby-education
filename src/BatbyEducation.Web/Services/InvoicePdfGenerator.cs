using BatbyEducation.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BatbyEducation.Web.Services;

public static class InvoicePdfGenerator
{
    public static IDocument Generate(Invoice invoice, Student student)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text("INVOICE").FontSize(24).Bold();
                    col.Item().Text($"Invoice Number: {invoice.InvoiceNumber}").FontSize(12);
                    col.Item().Text($"Date: {invoice.IssuedAt:dd/MM/yyyy}");
                    col.Item().PaddingTop(10).Text($"Billing Period: {invoice.BillingPeriodStart:dd/MM/yyyy} – {invoice.BillingPeriodEnd:dd/MM/yyyy}");
                });

                page.Content().PaddingVertical(20).Column(col =>
                {
                    // Student details
                    col.Item().Text("Bill To:").Bold();
                    col.Item().Text(student.Name);
                    col.Item().Text($"Guardian: {student.GuardianName}");
                    col.Item().Text($"Email: {student.GuardianEmail.Value}");
                    col.Item().PaddingBottom(15);

                    // Line items table
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2); // Date
                            columns.RelativeColumn(2); // Tutor
                            columns.RelativeColumn(2); // Subject
                            columns.RelativeColumn(1); // Duration
                            columns.RelativeColumn(1); // Rate
                            columns.RelativeColumn(1); // Amount
                        });

                        // Header
                        table.Header(header =>
                        {
                            header.Cell().Text("Date").Bold();
                            header.Cell().Text("Tutor").Bold();
                            header.Cell().Text("Subject").Bold();
                            header.Cell().Text("Duration").Bold();
                            header.Cell().Text("Rate").Bold();
                            header.Cell().AlignRight().Text("Amount").Bold();
                        });

                        // Rows
                        foreach (var item in invoice.LineItems)
                        {
                            table.Cell().Text(item.SessionDate.ToString("dd/MM/yyyy"));
                            table.Cell().Text(item.TutorName);
                            table.Cell().Text(item.Subject);
                            table.Cell().Text(item.IsLateCancellationFee ? "Late Cancel" : $"{item.DurationMinutes} min");
                            table.Cell().Text($"£{item.Rate:F2}");
                            table.Cell().AlignRight().Text($"£{item.Amount:F2}");
                        }
                    });

                    // Total
                    col.Item().PaddingTop(15).AlignRight().Text($"Total: £{invoice.TotalAmount:F2}").FontSize(14).Bold();

                    // Status
                    col.Item().PaddingTop(5).AlignRight().Text($"Status: {invoice.Status}").FontSize(10);
                });

                page.Footer().AlignCenter().Text("Batby Education");
            });
        });
    }
}
