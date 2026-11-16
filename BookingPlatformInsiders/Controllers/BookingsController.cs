using BookingPlatformInsiders.Application.Bookings.Commands;
using BookingPlatformInsiders.Application.Bookings.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BookingPlatformInsiders.Controllers;

[ApiController]
[Route("api/bookings")]
public sealed class BookingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BookingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<BookingDto>> Create(
        [FromBody] CreateBookingCommand command,
        CancellationToken cancellationToken)
    {
        var booking = await _mediator.Send(command, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, booking);
    }

    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new CancelBookingCommand(id), cancellationToken);

        return NoContent();
    }
}
