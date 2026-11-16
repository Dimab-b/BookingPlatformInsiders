using BookingPlatformInsiders.Domain.Common.Errors;
using BookingPlatformInsiders.Domain.Common.Exceptions;

namespace BookingPlatformInsiders.Domain.Rooms;

public sealed class InvalidRoomNameException : DomainException
{
    public InvalidRoomNameException()
        : base(ErrorCodes.RoomNameInvalid, "Room name is required and must be at most 100 characters.")
    {
    }
}

public sealed class InvalidRoomCapacityException : DomainException
{
    public InvalidRoomCapacityException()
        : base(ErrorCodes.RoomCapacityInvalid, "Room capacity must be between 1 and 100.")
    {
    }
}

public sealed class InvalidRoomLocationException : DomainException
{
    public InvalidRoomLocationException()
        : base(ErrorCodes.RoomLocationInvalid, "Room location is required and must be at most 100 characters.")
    {
    }
}

public sealed class InvalidRoomDescriptionException : DomainException
{
    public InvalidRoomDescriptionException()
        : base(ErrorCodes.RoomDescriptionInvalid, "Room description must be at most 500 characters.")
    {
    }
}
