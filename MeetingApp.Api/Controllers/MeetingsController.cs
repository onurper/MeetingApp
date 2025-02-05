using MeetingApp.Core.DTOs.UserMeeting;
using MeetingApp.Core.IServices;
using MeetingApp.Service.Services;
using Microsoft.AspNetCore.Mvc;

namespace MeetingApp.Api.Controllers
{
    //[Authorize]
    public class MeetingsController(IMeetingService meetingService, FileService fileService) : PrivateController
    {
        [HttpGet]
        public async Task<IActionResult> GetMeetings()
        {
            //var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            return ActionResultInstance(await meetingService.GetAllByUserIdAsync(1));
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateMeeting([FromForm] CreateUserMeetingDto request)
        {
            //var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            await meetingService.CreateAsync(1, request);

            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> UpdateMeeting(UpdateUserMeetingDto request, int id)
        {
            return ActionResultInstance(await meetingService.UpdateAsync(id, request));
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteMeeting(int id)
        {
            return ActionResultInstance(await meetingService.DeleteAsync(id));
        }

        [HttpPatch]
        public async Task<IActionResult> CancelMeeting(int id)
        {
            return ActionResultInstance(await meetingService.CancelMeetingAsync(id));
        }
    }
}