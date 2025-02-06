using MeetingApp.Core.DTOs;
using MeetingApp.Core.DTOs.Meeting;
using MeetingApp.Core.Models;

namespace MeetingApp.Core.IServices;

public interface IMeetingService
{
    Task<ServiceResult<List<Meeting>>> GetAllByUserIdAsync(int id);

    Task<ServiceResult<Meeting>> CreateAsync(int userId, CreateMeetingDto dto);

    Task<ServiceResult<MeetingDto>> UpdateAsync(int id, UpdateMeetingDto dto);

    Task<ServiceResult<EmptyDto>> DeleteAsync(int id);

    Task<ServiceResult<EmptyDto>> CancelMeetingAsync(int id);
}