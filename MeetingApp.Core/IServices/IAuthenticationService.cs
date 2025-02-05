using MeetingApp.Core.DTOs;

namespace MeetingApp.Core.IServices;

public interface IAuthenticationService
{
    Task<ServiceResult<TokenDto>> CreateTokenAsync(LoginDto loginDto);
}