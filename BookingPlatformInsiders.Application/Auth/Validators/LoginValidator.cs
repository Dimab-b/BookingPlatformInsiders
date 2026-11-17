using BookingPlatformInsiders.Application.Auth.Commands;
using FluentValidation;

namespace BookingPlatformInsiders.Application.Auth.Validators;

public sealed class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(command => command.Password)
            .NotEmpty();
    }
}
