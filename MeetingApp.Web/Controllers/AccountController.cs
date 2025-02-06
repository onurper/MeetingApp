using FluentValidation;
using MeetingApp.Web.Models;
using MeetingApp.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace MeetingApp.Web.Controllers
{
    public class AccountController(IValidator<LoginViewModel> validator, IHttpClientFactory clientFactory) : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel request)
        {
            var result = await validator.ValidateAsync(request);

            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }

                return View(request);
            }

            var client = clientFactory.CreateClient();
            var jsonData = JsonConvert.SerializeObject(request);

            StringContent content = new(jsonData, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://localhost:7196/api/Auth", content);
            if (response.IsSuccessStatusCode)
            {
                var resultJsonData = await response.Content.ReadAsStringAsync();
                var values = JsonConvert.DeserializeObject<ServiceResult<TokenDto>>(resultJsonData);

                var options = new CookieOptions
                {
                    Expires = DateTime.Now.AddHours(24)
                };

                HttpContext.Response.Cookies.Append("User", values.Data.AccessToken.ToString(), options);

                return RedirectToAction("Meeting", "Home");
            }
            else
            {
                var resultJsonData = await response.Content.ReadAsStringAsync();
                var values = JsonConvert.DeserializeObject<ServiceResult<EmptyDto>>(resultJsonData);

                foreach (string item in values.ErrorMessage)
                    ModelState.AddModelError("", item);

                return View(request);
            }
        }

        public IActionResult Logout()
        {
            Response.Cookies.Delete("User");
            return RedirectToAction("Index", "Home");
        }
    }
}