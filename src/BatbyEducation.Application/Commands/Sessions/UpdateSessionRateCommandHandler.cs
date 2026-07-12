using BatbyEducation.Domain.Common;
using BatbyEducation.Domain.Interfaces;
using MediatR;

namespace BatbyEducation.Application.Commands.Sessions;

public class UpdateSessionRateCommandHandler : IRequestHandler<UpdateSessionRateCommand, Result<bool>>
{
    private readonly ISessionRepository _sessionRepository;

    public UpdateSessionRateCommandHandler(ISessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task<Result<bool>> Handle(UpdateSessionRateCommand request, CancellationToken cancellationToken)
    {
        var session = await _sessionRepository.GetByIdAsync(request.SessionId);
        if (session is null)
            return Result<bool>.Failure("SessionId", "Session not found");

        var result = session.SetRateOverride(request.RateOverride);
        if (!result.IsSuccess)
            return Result<bool>.Failure(result.Errors);

        await _sessionRepository.UpdateAsync(session);
        return Result<bool>.Success(true);
    }
}
