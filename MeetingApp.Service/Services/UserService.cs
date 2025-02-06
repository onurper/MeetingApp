using MeetingApp.Core;
using MeetingApp.Core.DTOs;
using MeetingApp.Core.IRepositories;
using MeetingApp.Core.IServices;
using MeetingApp.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Net;

namespace MeetingApp.Service.Services;

public class UserService(IUserRepository userRepository, IUnitOfWork unitOfWork, IPasswordHasher<User> passwordHasher, IEmailService emailService, FileService fileService, ILogger<UserService> logger) : IUserService
{
    public async Task<ServiceResult<User>> UserRegister(UserDto request)
    {
        var path = string.Empty;

        if (request.ProfileImageFile != null && request.ProfileImageFile.Length != 0)
        {
            using var memoryStream = new MemoryStream();
            try
            {
                await request.ProfileImageFile.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                path = await fileService.SaveFileAsync(memoryStream, request.ProfileImageFile.FileName);
            }
            catch (Exception e)
            {
                logger.LogInformation("Kayıt profil resmi eklenirken bir hata oluştu.");
            }
        }

        var user = new User()
        {
            Name = request.Name,
            Surname = request.Surname,
            Email = request.Email,
            Phone = request.Phone,
            ProfileImagePath = path,
            CreatedDateTime = DateTime.Now
        };

        user.Password = passwordHasher.HashPassword(user, request.Password);

        await userRepository.AddAsync(user);
        await unitOfWork.SaveChangesAsync();

        _ = emailService.SendEmailAsync(user.Email, "Hoşgeldiniz", "Kayıt işleminiz başarıyla gerçekleştirilmiştir.");

        return ServiceResult<User>.Success(user, HttpStatusCode.Created);
    }
}