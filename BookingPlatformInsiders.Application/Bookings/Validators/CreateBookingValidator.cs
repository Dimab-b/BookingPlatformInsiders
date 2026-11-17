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

        RuleFor(command => command.AmountOriginal)
            .GreaterThan(0)
            .When(command => command.AmountOriginal.HasValue);

        RuleFor(command => command.Currency)
            .Length(3)
            .When(command => !string.IsNullOrWhiteSpace(command.Currency));

        RuleFor(command => command.IdempotencyKey)
            .MaximumLength(100)
            .When(command => !string.IsNullOrWhiteSpace(command.IdempotencyKey));
    }
}
