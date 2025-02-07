using FluentValidation;
using MeetingApp.Web.Constants;
using MeetingApp.Web.Models;
using MeetingApp.Web.Services.Interfaces;
using MeetingApp.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace MeetingApp.Web.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IValidator<LoginViewModel> _validator;
        private readonly HttpClient _client;
        private readonly IValidator<CreateUserViewModel> _createUserValidator;
        private readonly ICookieAuthService _cookieAuthService;

        public AccountController(IValidator<LoginViewModel> validator, IHttpClientFactory clientFactory, IValidator<CreateUserViewModel> createUserValidator, ICookieAuthService cookieAuthService)
        {
            _validator = validator;
            _client = clientFactory.CreateClient(HttpClientNames.ApiHttpClient);
            _createUserValidator = createUserValidator;
            _cookieAuthService = cookieAuthService;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel request)
        {
            var result = await _validator.ValidateAsync(request);

            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
                return View(request);
            }

            await _cookieAuthService.SignInAsync(request, true);

            return RedirectToAction("GetUserMeetings", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _cookieAuthService.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register([FromForm] CreateUserViewModel request)
        {
            var result = await _createUserValidator.ValidateAsync(request);

            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
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

            var response = await _client.PostAsync("https://localhost:7196/api/Users", multipartContent);

            if (!response.IsSuccessStatusCode)
            {
                var resultJsonData = await response.Content.ReadAsStringAsync();
                var values = JsonConvert.DeserializeObject<ServiceResult<EmptyDto>>(resultJsonData);

                foreach (string item in values.ErrorMessage)
                    ModelState.AddModelError("", item);

                return View(request);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}