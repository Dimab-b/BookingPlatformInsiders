using BookingPlatformInsiders.Application.Bookings.Dtos;
using BookingPlatformInsiders.Application.Bookings.Queries;
using BookingPlatformInsiders.Application.Common.Pagination;
using BookingPlatformInsiders.Application.Rooms.Dtos;
using BookingPlatformInsiders.Application.Rooms.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BookingPlatformInsiders.Controllers;

[ApiController]
[Route("api/rooms")]
public sealed class RoomsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RoomsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<RoomDto>>> GetRooms(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetRoomsQuery(pageNumber, pageSize), cancellationToken);

        return Ok(result);
    }

    [HttpGet("{roomId:guid}/bookings")]
    public async Task<ActionResult<PagedResult<BookingDto>>> GetRoomSchedule(
        Guid roomId,
        [FromQuery, BindRequired] DateOnly date,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetRoomScheduleQuery(roomId, date, pageNumber, pageSize),
            cancellationToken);

        if (result is null)
        {
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Room not found."
            };
            problem.Extensions["code"] = "room.not_found";

            return NotFound(problem);
        }

        return Ok(result);
    }
}
