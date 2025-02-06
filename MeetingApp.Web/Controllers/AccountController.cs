using FluentValidation;
using MeetingApp.Web.Models;
using MeetingApp.Web.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MeetingApp.Web.Controllers
{
    public class AccountController(IValidator<LoginViewModel> validator, IHttpClientFactory clientFactory, IValidator<CreateUserViewModel> createUserValidator) : Controller
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

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.ReadJwtToken(values.Data.AccessToken);

                var claims = token.Claims.ToList();
                claims.Add(new Claim(ClaimTypes.Name, request.Email));

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24),
                    IsPersistent = true
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity), authProperties);

                HttpContext.Response.Cookies.Append("User", values.Data.AccessToken, new CookieOptions { Expires = DateTime.Now.AddHours(24) });

                return RedirectToAction("Meeting", "Home");
            }
            else
            {
                var resultJsonData = await response.Content.ReadAsStringAsync();
                var values = JsonConvert.DeserializeObject<ServiceResult<EmptyDto>>(resultJsonData);

                foreach (string item in values.ErrorMessage)
                {
                    ModelState.AddModelError("", item);
                }

                return View(request);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromForm] CreateUserViewModel request)
        {
            var result = await createUserValidator.ValidateAsync(request);

            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
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

            var response = await client.PostAsync("https://localhost:7196/api/Users", multipartContent);

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