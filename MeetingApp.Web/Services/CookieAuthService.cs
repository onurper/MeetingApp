using MeetingApp.Web.Constants;
using MeetingApp.Web.Models;
using MeetingApp.Web.Services.Interfaces;
using MeetingApp.Web.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MeetingApp.Web.Services
{
    public class CookieAuthService(
        IHttpContextAccessor _httpContextAccessor,
        IHttpClientFactory _httpClientFactory
    ) : ICookieAuthService
    {
        public async Task SignInAsync(LoginViewModel login, bool rememberMe)
        {
            var loginResponse = await LoginAsync(login);

            if (loginResponse.ErrorMessage is not null)
                return;

            var token = loginResponse.Data!.AccessToken;

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            var claims = jwtToken.Claims.ToList();

            claims.Add(new Claim(CustomClaimTypes.AccessToken, token));

            var authProperties = new AuthenticationProperties()
            {
                IsPersistent = rememberMe
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await CookieSignInAsync(authProperties, claimsIdentity);
        }

        private async Task CookieSignInAsync(AuthenticationProperties authProperties, ClaimsIdentity claimsIdentity)
        {
            await _httpContextAccessor.HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        public async Task UpdateAccessTokenAsync(string accessToken)
        {
            var user = _httpContextAccessor.HttpContext.User;
            var claimsIdentity = user.Identity as ClaimsIdentity;

            if (claimsIdentity != null)
            {
                var oldClaim = claimsIdentity.FindFirst(CustomClaimTypes.AccessToken);
                if (oldClaim != null)
                {
                    claimsIdentity.RemoveClaim(oldClaim);
                }

                claimsIdentity.AddClaim(new Claim(CustomClaimTypes.AccessToken, accessToken));

                var authResult = await _httpContextAccessor.HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = authResult.Properties;

                await SignOutAsync();

                await CookieSignInAsync(authProperties, claimsIdentity);
            }
        }

        public string GetAccessToken()
        {
            var accessToken = _httpContextAccessor.HttpContext.User.FindFirstValue(CustomClaimTypes.AccessToken);
            return accessToken;
        }

        public Task SignOutAsync() => _httpContextAccessor.HttpContext.SignOutAsync();

        private async Task<ServiceResult<TokenDto>> LoginAsync(LoginViewModel login)
        {
            var client = _httpClientFactory.CreateClient(HttpClientNames.ApiHttpClient);

            var uri = new Uri(client.BaseAddress, "api/auth");

            var response = await client.PostAsJsonAsync(uri, login);

            var result = await response.Content.ReadFromJsonAsync<ServiceResult<TokenDto>>();

            return result;
        }
    }
}