using FluentValidation;
using MeetingApp.Web.Constants;
using MeetingApp.Web.Models;
using MeetingApp.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Text;

namespace MeetingApp.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IValidator<CreateMeetingViewModel> _validator;
        private HttpClient _client;

        public HomeController(
            IHttpClientFactory clientFactory,
            IValidator<CreateMeetingViewModel> validator
        )
        {
            _validator = validator;
            _client = clientFactory.CreateClient(HttpClientNames.ApiHttpClient);
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetUserMeetings()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var response = await _client.GetAsync($"https://localhost:7196/api/Meetings/{userId}");

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
            var validationResult = await _validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }

                return View(request);
            }
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
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var response = await _client.PostAsync($"https://localhost:7196/api/Meetings/{userId}", multipartContent);

            if (!response.IsSuccessStatusCode)
            {
                var resultJsonData = await response.Content.ReadAsStringAsync();
                var values = JsonConvert.DeserializeObject<ServiceResult<EmptyDto>>(resultJsonData);

                foreach (string item in values.ErrorMessage)
                    ModelState.AddModelError("", item);

                return View(request);
            }

            return RedirectToAction("GetUserMeetings", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> CancelMeeting(int id)
        {
            var patchRequest = new HttpRequestMessage(HttpMethod.Patch, $"https://localhost:7196/api/Meetings/{id}")
            {
                Content = new StringContent("", Encoding.UTF8, "application/json")
            };

            var response = await _client.SendAsync(patchRequest);

            if (!response.IsSuccessStatusCode)
                return Json(new { success = false, message = "Toplantý iptal edilemedi!" });

            return Json(new { success = true, message = "Toplantý baþarýyla iptal edildi!" });
        }
    }
}