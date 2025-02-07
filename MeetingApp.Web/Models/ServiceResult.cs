using System.Net;

namespace MeetingApp.Web.Models
{
    public class ServiceResult<T>
    {
        public bool IsSuccessStatusCode { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public T? Data { get; set; }
        public List<string>? ErrorMessage { get; set; }
    }
}