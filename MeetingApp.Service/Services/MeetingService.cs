using MeetingApp.Core;
using MeetingApp.Core.DTOs;
using MeetingApp.Core.DTOs.UserMeeting;
using MeetingApp.Core.IRepositories;
using MeetingApp.Core.IServices;
using MeetingApp.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;

namespace MeetingApp.Service.Services;

public class MeetingService(IMeetingRepository meetingRepository, IUnitOfWork unitOfWork, IEmailService emailService, IUserRepository repository, FileService fileService, ILogger<MeetingService> logger) : IMeetingService
{
    public async Task<ServiceResult<List<UserMeeting>>> GetAllByUserIdAsync(int id)
    {
        return ServiceResult<List<UserMeeting>>.Success(await meetingRepository.Where(x => x.UserId == id).ToListAsync(), HttpStatusCode.OK);
    }

    public async Task<ServiceResult<UserMeeting>> CreateAsync(int userId, CreateUserMeetingDto dto)
    {
        var path = string.Empty;

        if (dto.Document != null && dto.Document.Length != 0)
        {
            using var memoryStream = new MemoryStream();
            try
            {
                await dto.Document.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                path = await fileService.SaveAndCompressFileAsync(memoryStream, dto.Document.FileName);
            }
            catch (Exception e)
            {
                logger.LogInformation("Toplantı dokümanı eklenirken bir hata oluştu.");
            }
        }

        var userMeeting = new UserMeeting()
        {
            UserId = userId,
            Description = dto.Description,
            DocumentPath = path,
            EndDate = dto.EndDate,
            StartDate = dto.StartDate,
            Status = true,
            Title = dto.Title
        };

        await meetingRepository.AddAsync(userMeeting);
        await unitOfWork.SaveChangesAsync();

        #region Toplantı bilgilendirmesi

        //var user = await repository.GetByIdAsync(userMeeting.UserId);
        //await emailService.SendEmailAsync(user.Email, "Toplantı Bilgilendirmesi", "Toplantı kaydınız oluşturulmuştur.");

        #endregion Toplantı bilgilendirmesi

        return ServiceResult<UserMeeting>.Success(userMeeting, HttpStatusCode.Created);
    }

    public async Task<ServiceResult<UserMeetingDto>> UpdateAsync(int id, UpdateUserMeetingDto dto)
    {
        var existMeeting = await meetingRepository.Where(x => x.Id == id).AnyAsync();

        if (!existMeeting)
            return ServiceResult<UserMeetingDto>.Fail("Veri tabanında ilgili toplantı bulunamadı", HttpStatusCode.NotFound);

        var meeting = await meetingRepository.GetByIdAsync(id)!;

        meeting.Description = dto.Description;
        meeting.DocumentPath = dto.DocumentPath;
        meeting.EndDate = dto.EndDate;
        meeting.StartDate = dto.StartDate;
        meeting.Title = dto.Title;

        meetingRepository.Update(meeting);
        await unitOfWork.SaveChangesAsync();

        return ServiceResult<UserMeetingDto>.Success(new UserMeetingDto(meeting.Title, meeting.UserId, meeting.Status, meeting.Description,
            meeting.DocumentPath, meeting.StartDate, meeting.EndDate), HttpStatusCode.OK);
    }

    public async Task<ServiceResult<EmptyDto>> CancelMeetingAsync(int id)
    {
        var existMeeting = await meetingRepository.Where(x => x.Id == id).AnyAsync();

        if (!existMeeting)
            return ServiceResult<EmptyDto>.Fail("Veri tabanında ilgili toplantı bulunamadı", HttpStatusCode.NotFound);

        var meeting = await meetingRepository.GetByIdAsync(id)!;

        meeting.Status = false;

        meetingRepository.Update(meeting);
        await unitOfWork.SaveChangesAsync();

        return ServiceResult<EmptyDto>.Success(HttpStatusCode.OK);
    }

    public async Task<ServiceResult<EmptyDto>> DeleteAsync(int id)
    {
        var existMeeting = await meetingRepository.Where(x => x.Id == id).AnyAsync();

        if (!existMeeting)
            return ServiceResult<EmptyDto>.Fail("Veri tabanında ilgili toplantı bulunamadı", HttpStatusCode.NotFound);

        var meeting = await meetingRepository.GetByIdAsync(id)!;

        meetingRepository.Remove(meeting);
        await unitOfWork.SaveChangesAsync();

        return ServiceResult<EmptyDto>.Success(HttpStatusCode.NoContent);
    }
}