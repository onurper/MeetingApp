using Azure.Core;
using MeetingApp.Core;
using MeetingApp.Core.DTOs;
using MeetingApp.Core.DTOs.CreateDto;
using MeetingApp.Core.DTOs.UpdateDto;
using MeetingApp.Core.IRepositories;
using MeetingApp.Core.IServices;
using MeetingApp.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace MeetingApp.Service.Services;

public class UserService(IUserRepository userRepository, IUnitOfWork unitOfWork, IPasswordHasher<User> passwordHasher, IEmailService emailService, FileService fileService, ILogger<UserService> logger) : IUserService
{
    public async Task<ServiceResult<UserDto>> UserRegisterAsync(CreateUserDto request)
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

        UserDto userDto = new(

            user.Id,
            user.Name,
            user.Surname,
            user.Email,
            user.Phone,
            user.Password,
            user.ProfileImagePath
        );

        return ServiceResult<UserDto>.Success(userDto, HttpStatusCode.Created);
    }

    public async Task<ServiceResult<EmptyDto>> UserUpdateAsync(int id, UpdateUserDto request)
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

        var user = await userRepository.GetByIdAsync(id);

        user.Name = request.Name;
        user.Surname = request.Surname;
        user.Email = request.Email;
        user.Phone = request.Phone;

        if (!string.IsNullOrEmpty(path))
            user.ProfileImagePath = path;

        if (!string.IsNullOrEmpty(request.Password))
            user.Password = passwordHasher.HashPassword(user, request.Password);

        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync();

        return ServiceResult<EmptyDto>.Success(new EmptyDto(), HttpStatusCode.OK);
    }

    [HttpGet]
    public async Task<ServiceResult<UserDto>> GetUserByIdAsync(int id)
    {
        var user = await userRepository.GetByIdAsync(id);
        if (user == null)
            return ServiceResult<UserDto>.Fail("Kullanıcı bulunamadı.", HttpStatusCode.NotFound);

        UserDto userDto = new(

            user.Id,
            user.Name,
            user.Surname,
            user.Email,
            user.Phone,
            user.Password,
            user.ProfileImagePath
        );

        return ServiceResult<UserDto>.Success(userDto, HttpStatusCode.OK);
    }
}