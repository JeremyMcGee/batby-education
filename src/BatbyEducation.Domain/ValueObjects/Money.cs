namespace BatbyEducation.Domain.ValueObjects;

public record Money(decimal Amount)
{
    public static Money Zero => new(0m);

    public Money Add(Money other) => new(Amount + other.Amount);

    public Money Subtract(Money other) => new(Amount - other.Amount);

    public bool IsPositive => Amount > 0;

    public bool IsZeroOrNegative => Amount <= 0;
}
