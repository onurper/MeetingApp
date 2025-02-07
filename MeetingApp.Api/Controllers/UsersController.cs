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
            await userService.UserRegisterAsync(requestCreateUserDto);

            return Ok();
        }

        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateUser(int id, [FromForm] UpdateUserDto requestUpdateUserDto)
        {
            await userService.UserUpdateAsync(id, requestUpdateUserDto);
            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var result = await userService.GetUserByIdAsync(id);
            return ActionResultInstance(result);
        }
    }
}