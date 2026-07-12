namespace BatbyEducation.Domain.ValueObjects;

public record DateRange(DateOnly Start, DateOnly End)
{
    public bool Contains(DateOnly date) => date >= Start && date <= End;

    public static DateRange TaxYear(int startYear) =>
        new(new DateOnly(startYear, 4, 6), new DateOnly(startYear + 1, 4, 5));

    public static DateRange CurrentWeek()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var monday = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
        return new(monday, monday.AddDays(6));
    }
}
