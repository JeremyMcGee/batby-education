using MediatR;
using BatbyEducation.Domain.Common;

namespace BatbyEducation.Infrastructure.Events;

/// <summary>
/// Dispatches domain events using MediatR's notification pipeline.
/// Uses dynamic dispatch so domain events don't need to reference MediatR directly.
/// </summary>
public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IMediator _mediator;

    public DomainEventDispatcher(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task DispatchEventsAsync(IEnumerable<IDomainEvent> events)
    {
        foreach (var domainEvent in events)
        {
            await _mediator.Publish(domainEvent);
        }
    }
}
