using MeetingApp.Core.DTOs;
using MeetingApp.Core.IServices;
using Microsoft.AspNetCore.Mvc;

namespace MeetingApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthenticationService authenticationService) : PrivateController
    {
        [HttpPost]
        public async Task<IActionResult> CreateToken(LoginDto loginDto)
        {
            var result = await authenticationService.CreateTokenAsync(loginDto);
            return ActionResultInstance(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> CreateTokenByRefreshToken(int id)
        {
            var result = await authenticationService.CreateTokenByRefreshToken(id);
            return ActionResultInstance(result);
        }
    }
}