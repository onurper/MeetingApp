using FluentValidation;
using MeetingApp.Web.Constants;
using MeetingApp.Web.Helpers;
using MeetingApp.Web.Models;
using MeetingApp.Web.Services.Interfaces;
using MeetingApp.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Text;

namespace MeetingApp.Web.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IValidator<LoginViewModel> _validator;
        private readonly HttpClient _client;
        private readonly IValidator<CreateUserViewModel> _createUserValidator;
        private readonly IValidator<UpdateUserViewModel> _updateUserValidator;
        private readonly ICookieAuthService _cookieAuthService;

        public AccountController(IValidator<LoginViewModel> validator, IHttpClientFactory clientFactory, IValidator<CreateUserViewModel> createUserValidator, IValidator<UpdateUserViewModel> updateUserValidator, ICookieAuthService cookieAuthService)
        {
            _validator = validator;
            _client = clientFactory.CreateClient(HttpClientNames.ApiHttpClient);
            _createUserValidator = createUserValidator;
            _cookieAuthService = cookieAuthService;
            _updateUserValidator = updateUserValidator;
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

            var response = await _cookieAuthService.SignInAsync(request, true);

            if (response.ErrorMessage is not null)
            {
                foreach (var item in response.ErrorMessage)
                {
                    ModelState.AddModelError("", item);
                }
                return View(request);
            }

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

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var response = await _client.GetAsync($"https://localhost:7196/api/Users/{userId}");

            var resultJsonData = await response.Content.ReadAsStringAsync();
            var values = JsonConvert.DeserializeObject<ServiceResult<UpdateUserViewModel>>(resultJsonData);
            return View(values.Data);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUser(UpdateUserViewModel request)
        {
            var result = await _updateUserValidator.ValidateAsync(request);

            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
                return View(nameof(AccountController.Profile), request);
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

            var response = await _client.PutAsync($"https://localhost:7196/api/Users/{userId}", multipartContent);

            var resultJsonData = await response.Content.ReadAsStringAsync();
            var values = JsonConvert.DeserializeObject<ServiceResult<EmptyDto>>(resultJsonData);

            return RedirectToAction("Profile", "Account");
        }
    }
}