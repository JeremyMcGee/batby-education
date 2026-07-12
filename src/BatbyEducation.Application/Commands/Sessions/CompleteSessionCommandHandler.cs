using BatbyEducation.Domain.Common;
using BatbyEducation.Domain.Exceptions;
using BatbyEducation.Domain.Interfaces;
using MediatR;

namespace BatbyEducation.Application.Commands.Sessions;

public class CompleteSessionCommandHandler : IRequestHandler<CompleteSessionCommand, Result<bool>>
{
    private readonly ISessionRepository _sessionRepository;

    public CompleteSessionCommandHandler(ISessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task<Result<bool>> Handle(CompleteSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _sessionRepository.GetByIdAsync(request.SessionId);
        if (session is null)
        {
            return Result<bool>.Failure("SessionId", "Session not found");
        }

        try
        {
            session.Complete(request.ActualDurationMinutes);
        }
        catch (InvalidStateTransitionException)
        {
            return Result<bool>.Failure("Status", "Session is not eligible for completion");
        }
        catch (DomainException ex)
        {
            return Result<bool>.Failure("ActualDurationMinutes", ex.Message);
        }

        await _sessionRepository.UpdateAsync(session);

        return Result<bool>.Success(true);
    }
}
