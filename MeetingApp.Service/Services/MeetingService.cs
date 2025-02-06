using MeetingApp.Core;
using MeetingApp.Core.DTOs;
using MeetingApp.Core.DTOs.Meeting;
using MeetingApp.Core.IRepositories;
using MeetingApp.Core.IServices;
using MeetingApp.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;

namespace MeetingApp.Service.Services;

public class MeetingService(IMeetingRepository meetingRepository, IUnitOfWork unitOfWork, IEmailService emailService, IUserRepository repository, FileService fileService, ILogger<MeetingService> logger) : IMeetingService
{
    public async Task<ServiceResult<List<Meeting>>> GetAllByUserIdAsync(int id)
    {
        var meetings = await meetingRepository.Where(x => x.UserId == id).ToListAsync();

        if (meetings.Count == 0)
            return ServiceResult<List<Meeting>>.Fail("Kullanıcıya ait toplantı bulunamadı", HttpStatusCode.NotFound);

        return ServiceResult<List<Meeting>>.Success(meetings, HttpStatusCode.OK);
    }

    public async Task<ServiceResult<Meeting>> CreateAsync(int userId, CreateMeetingDto dto)
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

        var meeting = new Meeting()
        {
            UserId = userId,
            Description = dto.Description,
            DocumentPath = path,
            EndDate = dto.EndDate,
            StartDate = dto.StartDate,
            Status = true,
            Title = dto.Title
        };

        await meetingRepository.AddAsync(meeting);
        await unitOfWork.SaveChangesAsync();

        #region Toplantı bilgilendirmesi

        var user = await repository.GetByIdAsync(meeting.UserId);
        await emailService.SendEmailAsync(user.Email, "Toplantı Bilgilendirmesi", "Toplantı kaydınız oluşturulmuştur.");

        #endregion Toplantı bilgilendirmesi

        return ServiceResult<Meeting>.Success(meeting, HttpStatusCode.Created);
    }

    public async Task<ServiceResult<MeetingDto>> UpdateAsync(int id, UpdateMeetingDto dto)
    {
        var existMeeting = await meetingRepository.Where(x => x.Id == id).AnyAsync();

        if (!existMeeting)
            return ServiceResult<MeetingDto>.Fail("Veri tabanında ilgili toplantı bulunamadı", HttpStatusCode.NotFound);

        var meeting = await meetingRepository.GetByIdAsync(id)!;

        meeting.Description = dto.Description;
        meeting.DocumentPath = dto.DocumentPath;
        meeting.EndDate = dto.EndDate;
        meeting.StartDate = dto.StartDate;
        meeting.Title = dto.Title;

        meetingRepository.Update(meeting);
        await unitOfWork.SaveChangesAsync();

        return ServiceResult<MeetingDto>.Success(new MeetingDto(meeting.Id, meeting.Title, meeting.UserId, meeting.Status, meeting.Description,
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

        return ServiceResult<EmptyDto>.Success(new EmptyDto(), HttpStatusCode.OK);
    }

    public async Task<ServiceResult<EmptyDto>> DeleteAsync(int id)
    {
        var existMeeting = await meetingRepository.Where(x => x.Id == id).AnyAsync();

        if (!existMeeting)
            return ServiceResult<EmptyDto>.Fail("Veri tabanında ilgili toplantı bulunamadı", HttpStatusCode.NotFound);

        var meeting = await meetingRepository.GetByIdAsync(id)!;

        meetingRepository.Remove(meeting);
        await unitOfWork.SaveChangesAsync();

        return ServiceResult<EmptyDto>.Success(new EmptyDto(), HttpStatusCode.NoContent);
    }
}