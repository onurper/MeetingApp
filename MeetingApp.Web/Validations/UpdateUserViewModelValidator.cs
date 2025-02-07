using FluentValidation;
using MeetingApp.Web.ViewModels;

namespace MeetingApp.Web.Validations;

public class UpdateUserViewModelValidator : AbstractValidator<UpdateUserViewModel>
{
    public UpdateUserViewModelValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ad boş olamaz.")
            .MaximumLength(50).WithMessage("Ad en fazla 50 karakter olabilir.");

        RuleFor(x => x.Surname)
            .NotEmpty().WithMessage("Soyad boş olamaz.")
            .MaximumLength(50).WithMessage("Soyad en fazla 50 karakter olabilir.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta adresi boş olamaz.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Telefon numarası boş olamaz.")
            .Matches(@"^\+?\d{10,15}$").WithMessage("Geçerli bir telefon numarası giriniz.");

        RuleFor(x => x.Password)
            .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır.")
            .Matches(@"[A-Z]").WithMessage("Şifre en az bir büyük harf içermelidir.")
            .Matches(@"[a-z]").WithMessage("Şifre en az bir küçük harf içermelidir.")
            .Matches(@"\d").WithMessage("Şifre en az bir rakam içermelidir.")
            .Matches(@"[\W]").WithMessage("Şifre en az bir özel karakter içermelidir.")
            .When(x => !string.IsNullOrEmpty(x.Password));

        RuleFor(x => x.ProfileImageFile)
            .Must(file => file == null || IsImageFile(file))
            .WithMessage("Sadece JPEG, PNG, GIF veya WEBP formatında dosya yükleyebilirsiniz.");
    }

    private bool IsImageFile(IFormFile file)
    {
        if (file == null) return true;

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var fileExtension = Path.GetExtension(file.FileName).ToLower();
        return allowedExtensions.Contains(fileExtension);
    }
}