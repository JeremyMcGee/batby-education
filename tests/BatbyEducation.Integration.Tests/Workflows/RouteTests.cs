using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BatbyEducation.Integration.Tests.Workflows;

/// <summary>
/// End-to-end route smoke tests that boot the real web host and verify
/// all key routes respond without errors (catches ambiguous routes, 
/// missing pages, DI failures, etc.)
/// </summary>
public class RouteTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public RouteTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/students")]
    [InlineData("/students/register")]
    [InlineData("/tutors")]
    [InlineData("/tutors/register")]
    [InlineData("/calendar")]
    [InlineData("/sessions")]
    [InlineData("/sessions/book")]
    [InlineData("/invoices")]
    [InlineData("/invoices/generate")]
    [InlineData("/payments")]
    [InlineData("/payments/list")]
    [InlineData("/reports")]
    [InlineData("/reports/revenue")]
    [InlineData("/reports/tutor-earnings")]
    [InlineData("/reports/ledger")]
    [InlineData("/reports/balances")]
    [InlineData("/reports/payment-not-received")]
    [InlineData("/reports/tax-year")]
    public async Task Route_ReturnsSuccessStatusCode(string url)
    {
        var response = await _client.GetAsync(url);

        // Blazor Server pages return 200 OK
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ApiRoute_InvoicePdf_ReturnsNotFound_ForMissingInvoice()
    {
        var response = await _client.GetAsync($"/api/invoices/{Guid.NewGuid()}/pdf");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
