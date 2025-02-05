using MeetingApp.Core.DTOs;
using MeetingApp.Core.Models;

namespace MeetingApp.Core.IServices;

public interface IUserService
{
    Task<ServiceResult<User>> UserRegister(UserDto request);
}