using MeetingApp.Web.Helpers;
using MeetingApp.Web.Models;
using MeetingApp.Web.Services.Interfaces;
using System.Net;
using System.Net.Http.Headers;

namespace MeetingApp.Web.Handlers
{
    public class ApiHttpClientHandler(
        ICookieAuthService _cookieAuthService
        ) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var accessToken = _cookieAuthService.GetAccessToken();

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var userId = JwtTokenHelper.GetUserId(accessToken);

                var requestForRefreshToken = new HttpRequestMessage()
                {
                    RequestUri = new Uri($"https://localhost:7196/api/Auth/{userId}")
                };
                var refreshTokenResponse = await SendAsync(requestForRefreshToken, cancellationToken);

                if (refreshTokenResponse.IsSuccessStatusCode)
                {
                    var tokenResult = await refreshTokenResponse.Content.ReadFromJsonAsync<ServiceResult<TokenDto>>();

                    var newAccessToken = tokenResult.Data!.AccessToken;

                    _cookieAuthService.UpdateAccessTokenAsync(newAccessToken);

                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newAccessToken);

                    response = await base.SendAsync(request, cancellationToken);
                }

                await _cookieAuthService.SignOutAsync();
            }

            return response;
        }
    }
}