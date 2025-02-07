using MeetingApp.Web.Models;
using MeetingApp.Web.ViewModels;

namespace MeetingApp.Web.Services.Interfaces;

public interface ICookieAuthService
{
    Task<ServiceResult<TokenDto>> SignInAsync(LoginViewModel login, bool rememberMe);

    Task UpdateAccessTokenAsync(string accessToken);

    string GetAccessToken();

    Task SignOutAsync();
}