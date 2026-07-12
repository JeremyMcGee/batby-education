using BatbyEducation.Domain.Common;
using MediatR;

namespace BatbyEducation.Application.Commands.Sessions;

public record UpdateSessionRateCommand(Guid SessionId, decimal? RateOverride) : IRequest<Result<bool>>;
