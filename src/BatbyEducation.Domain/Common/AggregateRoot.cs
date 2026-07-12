namespace BatbyEducation.Domain.Common;

/// <summary>
/// Base class for aggregate roots. Aggregate roots are the entry point for changes
/// in a bounded context and ensure consistency boundaries.
/// </summary>
public abstract class AggregateRoot : Entity
{
}
