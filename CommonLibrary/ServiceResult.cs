using System.Net;
using System.Text.Json.Serialization;

namespace MeetingApp.Core
{
    public class ServiceResult<T>
    {
        public T? Data { get; set; }
        public List<string>? ErrorMessage { get; set; }
        [JsonIgnore] public bool IsSuccess => ErrorMessage == null || ErrorMessage.Count == 0;
        [JsonIgnore] public bool IsFail => !IsSuccess;
        [JsonIgnore] public HttpStatusCode Status { get; set; }

        [JsonIgnore] public string? UrlAsCreated { get; set; }

        //static factory method
        public static ServiceResult<T> Success(T data, HttpStatusCode status = HttpStatusCode.OK)
        {
            return new ServiceResult<T>()
            {
                Data = data,
                Status = status
            };
        }

        public static ServiceResult<T> Success(HttpStatusCode status = HttpStatusCode.OK)
        {
            return new ServiceResult<T>()
            {
                Data = default,
                Status = status
            };
        }

        public static ServiceResult<T> Fail(List<string> errorMessage,
            HttpStatusCode status = HttpStatusCode.BadRequest)
        {
            return new ServiceResult<T>()
            {
                ErrorMessage = errorMessage,
                Status = status
            };
        }

        public static ServiceResult<T> Fail(string errorMessage, HttpStatusCode status = HttpStatusCode.BadRequest)
        {
            return new ServiceResult<T>()
            {
                ErrorMessage = [errorMessage],
                Status = status
            };
        }
    }
}