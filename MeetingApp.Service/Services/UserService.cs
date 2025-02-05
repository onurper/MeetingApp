using MeetingApp.Core;
using MeetingApp.Core.DTOs;
using MeetingApp.Core.IRepositories;
using MeetingApp.Core.IServices;
using MeetingApp.Core.Models;
using Microsoft.AspNetCore.Identity;
using System.Net;

namespace MeetingApp.Service.Services;

public class UserService(IUserRepository userRepository, IUnitOfWork unitOfWork, IPasswordHasher<User> passwordHasher, IEmailService emailService) : IUserService
{
    public async Task<ServiceResult<User>> UserRegister(UserDto request)
    {
        var user = User.FromUserDto(request);

        user.CreatedDateTime = DateTime.Now;
        user.Password = passwordHasher.HashPassword(user, request.Password);

        await userRepository.AddAsync(user);
        await unitOfWork.SaveChangesAsync();

        //await emailService.SendEmailAsync(user.Email, "Hoşgeldiniz", "Kayıt işleminiz başarıyla gerçekleştirilmiştir.");

        return ServiceResult<User>.Success(user, HttpStatusCode.Created);
    }
}