using BookingPlatformInsiders.Application.Auth.Dtos;
using BookingPlatformInsiders.Application.Auth.Abstractions;
using BookingPlatformInsiders.Application.Common.Persistence;
using BookingPlatformInsiders.Application.Users;
using BookingPlatformInsiders.Domain.Users;
using MediatR;

namespace BookingPlatformInsiders.Application.Auth.Commands;

public sealed record LoginCommand(string Email, string Password) : IRequest<AuthResult>;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuthTokenIssuer _authTokenIssuer;
    private readonly IUnitOfWork _unitOfWork;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        IAuthTokenIssuer authTokenIssuer,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _authTokenIssuer = authTokenIssuer;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResult> Handle(
        LoginCommand command,
        CancellationToken cancellationToken)
    {
        var email = new Email(command.Email);
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

        if (user is null || !_passwordHasher.Verify(user.PasswordHash, command.Password))
        {
            throw new InvalidCredentialsException();
        }

        var issued = _authTokenIssuer.Issue(user);

        await _refreshTokenRepository.AddAsync(issued.RefreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return issued.AuthResult;
    }
}
