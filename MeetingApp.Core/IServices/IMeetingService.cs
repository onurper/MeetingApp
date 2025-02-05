using MeetingApp.Core.DTOs;
using MeetingApp.Core.DTOs.UserMeeting;
using MeetingApp.Core.Models;

namespace MeetingApp.Core.IServices;

public interface IMeetingService
{
    Task<ServiceResult<List<UserMeeting>>> GetAllByUserIdAsync(int id);

    Task<ServiceResult<UserMeeting>> CreateAsync(int userId, CreateUserMeetingDto dto);

    Task<ServiceResult<UserMeetingDto>> UpdateAsync(int id, UpdateUserMeetingDto dto);

    Task<ServiceResult<EmptyDto>> DeleteAsync(int id);

    Task<ServiceResult<EmptyDto>> CancelMeetingAsync(int id);
}