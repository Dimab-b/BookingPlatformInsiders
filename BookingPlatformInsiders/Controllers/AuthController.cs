using BookingPlatformInsiders.Application.Auth.Commands;
using BookingPlatformInsiders.Auth;
using BookingPlatformInsiders.Application.Auth.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingPlatformInsiders.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly RefreshCookieService _refreshCookies;

    public AuthController(IMediator mediator, RefreshCookieService refreshCookies)
    {
        _mediator = mediator;
        _refreshCookies = refreshCookies;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register(
        [FromBody] RegisterCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        return ToAuthResponse(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        return ToAuthResponse(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Refresh(CancellationToken cancellationToken)
    {
        var refreshToken = _refreshCookies.Read(Request) ?? string.Empty;
        var result = await _mediator.Send(new RefreshTokenCommand(refreshToken), cancellationToken);

        return ToAuthResponse(result);
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var refreshToken = _refreshCookies.Read(Request) ?? string.Empty;
        await _mediator.Send(new LogoutCommand(refreshToken), cancellationToken);

        _refreshCookies.Clear(Response);

        return NoContent();
    }

    [HttpPost("logout-all")]
    [Authorize]
    public async Task<IActionResult> LogoutAll(CancellationToken cancellationToken)
    {
        await _mediator.Send(new LogoutAllCommand(), cancellationToken);

        _refreshCookies.Clear(Response);

        return NoContent();
    }

    private ActionResult<AuthResponse> ToAuthResponse(AuthResult result)
    {
        _refreshCookies.Set(Response, result.RefreshToken);

        return Ok(new AuthResponse(result.AccessToken, result.ExpiresInSeconds, result.User));
    }
}
