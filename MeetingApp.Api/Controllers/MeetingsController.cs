using MeetingApp.Core.DTOs.Meeting;
using MeetingApp.Core.IServices;
using MeetingApp.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MeetingApp.Api.Controllers
{
    [Authorize]
    public class MeetingsController(IMeetingService meetingService, FileService fileService) : PrivateController
    {
        [HttpGet]
        public async Task<IActionResult> GetMeetings()
        {
            var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            return ActionResultInstance(await meetingService.GetAllByUserIdAsync(userId));
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateMeeting([FromForm] CreateMeetingDto request)
        {
            var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            return ActionResultInstance(await meetingService.CreateAsync(userId, request));
        }

        [HttpPut]
        public async Task<IActionResult> UpdateMeeting(UpdateMeetingDto request, int id) =>
            ActionResultInstance(await meetingService.UpdateAsync(id, request));

        [HttpDelete]
        public async Task<IActionResult> DeleteMeeting(int id) => ActionResultInstance(await meetingService.DeleteAsync(id));

        [HttpPatch("{id}")]
        public async Task<IActionResult> CancelMeeting(int id)
        {
            return ActionResultInstance(await meetingService.CancelMeetingAsync(id));
        }
    }
}