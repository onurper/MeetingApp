using MeetingApp.Web.ViewModels;

namespace MeetingApp.Web.Services.Interfaces;

public interface ICookieAuthService
{
    Task SignInAsync(LoginViewModel login, bool rememberMe);

    Task UpdateAccessTokenAsync(string accessToken);

    string GetAccessToken();

    Task SignOutAsync();
}