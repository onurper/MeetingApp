using FluentValidation;
using MeetingApp.Web.ViewModels;

namespace MeetingApp.Web.Validations;

public class LoginValidator : AbstractValidator<LoginViewModel>
{
    public LoginValidator()
    {
        _ = RuleFor(x => x.Email).NotEmpty().WithMessage("E-posta alanı boş geçilemez.").EmailAddress().WithMessage("Geçerli bir e-posta giriniz.");
        _ = RuleFor(x => x.Password).NotEmpty().WithMessage("Şifre alanı boş geçilemez.");
    }
}