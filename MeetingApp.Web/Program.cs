using FluentValidation;
using MeetingApp.Web.Constants;
using MeetingApp.Web.Handlers;
using MeetingApp.Web.Services;
using MeetingApp.Web.Services.Interfaces;
using MeetingApp.Web.Validations;
using MeetingApp.Web.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient(HttpClientNames.ApiHttpClient, configureClient =>
{
    configureClient.BaseAddress = new Uri("https://localhost:7196");
}).AddHttpMessageHandler<ApiHttpClientHandler>(); ;

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddTransient<ApiHttpClientHandler>();

builder.Services.AddScoped<ICookieAuthService, CookieAuthService>();

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IValidator<LoginViewModel>, LoginValidator>();
builder.Services.AddScoped<IValidator<CreateMeetingViewModel>, CreateMeetingViewModelValidator>();
builder.Services.AddScoped<IValidator<CreateUserViewModel>, CreateUserViewModelValidator>();
builder.Services.AddScoped<IValidator<UpdateUserViewModel>, UpdateUserViewModelValidator>();

builder.Services.AddAuthentication(auth =>
    {
        auth.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        auth.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        auth.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        auth.DefaultSignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        auth.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(1);
        options.SlidingExpiration = true;
    });

var app = builder.Build();

app.UseStatusCodePagesWithReExecute("/Error/NotFound");

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();