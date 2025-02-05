using MeetingApp.Core;
using Microsoft.AspNetCore.Mvc;

namespace MeetingApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrivateController : ControllerBase
    {
        public IActionResult ActionResultInstance<T>(ServiceResult<T> response) where T : class
        {
            return new ObjectResult(response)
            {
                StatusCode = response.Status.GetHashCode()
            };
        }
    }
}