using BatbyEducation.Domain.Common;
using BatbyEducation.Domain.Exceptions;
using BatbyEducation.Domain.Interfaces;
using MediatR;

namespace BatbyEducation.Application.Commands.Sessions;

public class CancelSessionCommandHandler : IRequestHandler<CancelSessionCommand, Result<bool>>
{
    private readonly ISessionRepository _sessionRepository;

    public CancelSessionCommandHandler(ISessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task<Result<bool>> Handle(CancelSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _sessionRepository.GetByIdAsync(request.SessionId);
        if (session is null)
        {
            return Result<bool>.Failure("SessionId", "Session not found");
        }

        try
        {
            session.Cancel(DateTime.UtcNow);
        }
        catch (InvalidStateTransitionException)
        {
            return Result<bool>.Failure("Status", "Only sessions with status 'Scheduled' can be cancelled");
        }

        await _sessionRepository.UpdateAsync(session);

        return Result<bool>.Success(true);
    }
}
