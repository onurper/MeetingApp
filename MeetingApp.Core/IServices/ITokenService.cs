using MeetingApp.Core.DTOs;
using MeetingApp.Core.Models;

namespace MeetingApp.Core.IServices;

public interface ITokenService
{
    TokenDto CreateToken(User user);
}