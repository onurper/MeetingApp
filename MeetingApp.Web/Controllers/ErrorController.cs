using Microsoft.AspNetCore.Mvc;

namespace MeetingApp.Web.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/NotFound")]
        public IActionResult NotFoundPage()
        {
            return View();
        }
    }
}