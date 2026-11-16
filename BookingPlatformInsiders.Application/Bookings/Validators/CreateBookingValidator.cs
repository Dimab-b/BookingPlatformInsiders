using BookingPlatformInsiders.Application.Bookings.Commands;
using BookingPlatformInsiders.Domain.Bookings;
using FluentValidation;

namespace BookingPlatformInsiders.Application.Bookings.Validators;

public sealed class CreateBookingValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingValidator()
    {
        RuleFor(command => command.RoomId).NotEmpty();
        RuleFor(command => command.Title).NotEmpty().MaximumLength(BookingTitle.MaxLength);
        RuleFor(command => command.EndUtc).GreaterThan(command => command.StartUtc);
    }
}
