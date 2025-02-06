using Hangfire;
using MeetingApp.Core.DTOs;
using MeetingApp.Core.IServices;
using MeetingApp.Core.Models;
using MeetingApp.Service.ExceptionHandlers;
using MeetingApp.Service.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using static MeetingApp.Core.DTOs.TokenOption;

namespace MeetingApp.Service.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddServiceExt(this IServiceCollection services, IConfiguration configuration)
    {
        // CORE SERVİSLER (Temel bağımlılıklar)
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IMeetingService, MeetingService>();
        services.AddScoped<FileService>();

        // AUTHENTICATION & SECURITY (Kimlik doğrulama ve güvenlik)
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<ITokenService, TokenService>();
        services.Configure<CustomTokenOption>(configuration.GetSection("TokenOption"));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opts =>
        {
            var tokenOptions = configuration.GetSection("TokenOption").Get<TokenOption.CustomTokenOption>()!;
            opts.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidIssuer = tokenOptions.Issuer,
                ValidAudience = tokenOptions.Audience[0],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenOptions.SecurityKey)),

                ValidateIssuerSigningKey = true,
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        // EMAIL SERVİSLERİ
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.AddTransient<IEmailService, EmailService>();

        // SWAGGER DOKÜMANTASYONU
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Meeting API", Version = "v1" });

            //  JWT Authentication Ayarları
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT kullanmak için 'Bearer {token}' formatında girin.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] { }
            }
        });
        });

        // HANGFIRE (Arka Plan İşlemleri)
        services.AddHangfire(config =>
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(configuration.GetConnectionString("SqlServer")));

        services.AddHangfireServer();
        services.AddScoped<MeetingCleanupService>();

        // GLOBAL EXCEPTION HANDLING
        services.AddExceptionHandler<GlobalExceptionHandler>();

        return services;
    }
}