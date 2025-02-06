using FluentValidation;
using MeetingApp.Web.Models;
using MeetingApp.Web.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace MeetingApp.Web.Controllers
{
    public class HomeController(IHttpClientFactory clientFactory, IValidator<CreateMeetingViewModel> validator) : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Meeting()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            var client = clientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Request.Cookies["User"]);

            var response = await client.GetAsync("https://localhost:7196/api/Meetings");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                response = await client.GetAsync($"https://localhost:7196/api/Auth/{userId}");

                if (!response.IsSuccessStatusCode)
                {
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    return RedirectToAction("Login", "Account");
                }

                var resultJson = await response.Content.ReadAsStringAsync();
                var tokenResult = JsonConvert.DeserializeObject<ServiceResult<TokenDto>>(resultJson)!;

                HttpContext.Response.Cookies.Append("User", tokenResult.Data!.AccessToken, new CookieOptions { Expires = DateTime.Now.AddHours(24) });

                client.DefaultRequestHeaders.Remove("Authorization");
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokenResult.Data!.AccessToken);
                response = await client.GetAsync("https://localhost:7196/api/Meetings");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Logout", "Account"); // Hala yetkilendirme hatasý varsa çýkýþ yap
                }
            }

            var resultJsonData = await response.Content.ReadAsStringAsync();
            var values = JsonConvert.DeserializeObject<ServiceResult<List<MeetingDto>>>(resultJsonData)!;

            List<MeetingDto> meetings = values.Data;
            return View(meetings);
        }

        public IActionResult CreateMeeting()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateMeeting(CreateMeetingViewModel request)
        {
            if (User.Identity != null && !User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }

                return View(request);
            }

            var client = clientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Request.Cookies["User"]);

            using var multipartContent = new MultipartFormDataContent();

            foreach (var property in request.GetType().GetProperties())
            {
                var value = property.GetValue(request);
                if (value == null) continue;

                if (value is IFormFile file)
                {
                    var fileContent = new StreamContent(file.OpenReadStream());
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                    multipartContent.Add(fileContent, property.Name, file.FileName);
                }
                else
                {
                    multipartContent.Add(new StringContent(value.ToString()), property.Name);
                }
            }

            var response = await client.PostAsync("https://localhost:7196/api/Meetings", multipartContent);

            if (!response.IsSuccessStatusCode)
            {
                var resultJsonData = await response.Content.ReadAsStringAsync();
                var values = JsonConvert.DeserializeObject<ServiceResult<EmptyDto>>(resultJsonData);

                foreach (string item in values.ErrorMessage)
                    ModelState.AddModelError("", item);

                return View(request);
            }

            return RedirectToAction("Meeting", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> CancelMeeting(int id)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            var client = clientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Request.Cookies["User"]);

            var patchRequest = new HttpRequestMessage(HttpMethod.Patch, $"https://localhost:7196/api/Meetings/{id}")
            {
                Content = new StringContent("", Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(patchRequest);

            if (!response.IsSuccessStatusCode)
                return Json(new { success = false, message = "Toplantý iptal edilemedi!" });

            return Json(new { success = true, message = "Toplantý baþarýyla iptal edildi!" });
        }
    }
}