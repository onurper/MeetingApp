using MeetingApp.Core.DTOs;
using MeetingApp.Core.DTOs.CreateDto;
using MeetingApp.Core.DTOs.UpdateDto;
using MeetingApp.Core.Models;

namespace MeetingApp.Core.IServices;

public interface IUserService
{
    Task<ServiceResult<UserDto>> GetUserByIdAsync(int id);
    Task<ServiceResult<UserDto>> UserRegisterAsync(CreateUserDto request);
    Task<ServiceResult<EmptyDto>> UserUpdateAsync(int id, UpdateUserDto request);
}