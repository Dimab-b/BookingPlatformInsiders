using BookingPlatformInsiders.Application.Auth.Commands;
using BookingPlatformInsiders.Domain.Users;
using FluentValidation;

namespace BookingPlatformInsiders.Application.Auth.Validators;

public sealed class RegisterValidator : AbstractValidator<RegisterCommand>
{
    public RegisterValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(Email.MaxLength);

        RuleFor(command => command.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(128);
    }
}
