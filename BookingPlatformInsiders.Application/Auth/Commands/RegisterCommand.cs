using BookingPlatformInsiders.Application.Auth.Dtos;
using BookingPlatformInsiders.Application.Auth.Abstractions;
using BookingPlatformInsiders.Application.Common.Persistence;
using BookingPlatformInsiders.Application.Common.Time;
using BookingPlatformInsiders.Application.Users;
using BookingPlatformInsiders.Domain.Users;
using MediatR;

namespace BookingPlatformInsiders.Application.Auth.Commands;

public sealed record RegisterCommand(string Email, string Password) : IRequest<AuthResult>;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuthTokenIssuer _authTokenIssuer;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        IAuthTokenIssuer authTokenIssuer,
        IUnitOfWork unitOfWork,
        IClock clock)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _authTokenIssuer = authTokenIssuer;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<AuthResult> Handle(
        RegisterCommand command,
        CancellationToken cancellationToken)
    {
        var email = new Email(command.Email);

        if (await _userRepository.EmailExistsAsync(email, cancellationToken))
        {
            throw new EmailAlreadyTakenException();
        }

        var passwordHash = _passwordHasher.Hash(command.Password);
        var user = User.Create(email, passwordHash, UserRole.User, _clock.UtcNow);

        var issued = _authTokenIssuer.Issue(user);

        await _userRepository.AddAsync(user, cancellationToken);
        await _refreshTokenRepository.AddAsync(issued.RefreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return issued.AuthResult;
    }
}
