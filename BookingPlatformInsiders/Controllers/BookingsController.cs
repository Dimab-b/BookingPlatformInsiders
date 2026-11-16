using BookingPlatformInsiders.Application.Bookings.Commands;
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

    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CancelBookingCommand(id), cancellationToken);

        return result switch
        {
            CancelBookingResult.Cancelled => NoContent(),
            CancelBookingResult.NotFound => NotFoundProblem(),
            _ => ConflictProblem()
        };
    }

    private ObjectResult NotFoundProblem()
    {
        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Booking not found."
        };
        problem.Extensions["code"] = "booking.not_found";

        return NotFound(problem);
    }

    private ObjectResult ConflictProblem()
    {
        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "A completed booking cannot be cancelled."
        };
        problem.Extensions["code"] = "booking.cannot_cancel";

        return Conflict(problem);
    }
}
