using MeetingApp.Core.DTOs;
using MeetingApp.Core.IServices;
using Microsoft.AspNetCore.Mvc;

namespace MeetingApp.Api.Controllers
{
    public class UsersController(IUserService userService) : PrivateController
    {
        [HttpPost]
        public async Task<IActionResult> CreateUser(UserDto requestCreateUserDto)
        {
            await userService.UserRegister(requestCreateUserDto);

            return Ok();
        }
    }
}