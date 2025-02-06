using MeetingApp.Core.DTOs;
using MeetingApp.Core.IServices;
using Microsoft.AspNetCore.Mvc;

namespace MeetingApp.Api.Controllers
{
    public class UsersController(IUserService userService) : PrivateController
    {
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateUser([FromForm] UserDto requestCreateUserDto)
        {
            await userService.UserRegister(requestCreateUserDto);

            return Ok();
        }
    }
}