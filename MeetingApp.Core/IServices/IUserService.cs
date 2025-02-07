using MeetingApp.Core.DTOs;
using MeetingApp.Core.Models;

namespace MeetingApp.Core.IServices;

public interface IUserService
{
    Task<ServiceResult<User>> GetUserByIdAsync(int id);
    Task<ServiceResult<User>> UserRegisterAsync(UserDto request);
    Task<ServiceResult<EmptyDto>> UserUpdateAsync(int id, UpdateUserDto request);
}