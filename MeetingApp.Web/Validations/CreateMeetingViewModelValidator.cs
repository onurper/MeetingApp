using FluentValidation;
using MeetingApp.Web.ViewModels;

namespace MeetingApp.Web.Validations;

public class CreateMeetingViewModelValidator : AbstractValidator<CreateMeetingViewModel>
{
    public CreateMeetingViewModelValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Başlık boş olamaz.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Açıklama boş olamaz.");

        RuleFor(x => x.Document)
            .NotNull().WithMessage("Doküman seçmek zorundasınız.") // Önce null kontrolü
            .Must(file => file == null || file.Length > 0).WithMessage("Geçerli bir dosya yükleyin.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Başlangıç tarihi boş olamaz.")
            .GreaterThanOrEqualTo(DateTime.Now).WithMessage("Başlangıç tarihi geçmiş bir tarih olamaz.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("Bitiş tarihi boş olamaz.")
            .GreaterThan(x => x.StartDate).WithMessage("Bitiş tarihi, başlangıç tarihinden büyük olmalıdır.");
    }
}