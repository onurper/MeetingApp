using FluentValidation;
using MeetingApp.Web.Models;
using MeetingApp.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
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
            if (Request.Cookies["User"] == null)
                return RedirectToAction("Index", "Home");

            var client = clientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Request.Cookies["User"]);

            var response = await client.GetAsync("https://localhost:7196/api/Meetings");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Logout", "Account");
            }

            var resultJsonData = await response.Content.ReadAsStringAsync();
            var values = JsonConvert.DeserializeObject<ServiceResult<List<UserMeetingDto>>>(resultJsonData)!;

            List<UserMeetingDto> userMeetings = values.Data;

            return View(userMeetings);
        }

        public IActionResult CreateMeeting()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateMeeting(CreateMeetingViewModel request)
        {
            if (Request.Cookies["User"] == null)
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
            if (Request.Cookies["User"] == null) return RedirectToAction("Login", "Account");

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