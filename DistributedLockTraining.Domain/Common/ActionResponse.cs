using System.Text.Json.Serialization;

namespace DistributedLockTraining.Domain.Common
{
    public class ActionResponse<T>
    {
        public T Data { get; set; }
        [JsonIgnore]
        public int StatusCode { get; set; }
        public bool IsSuccessful { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; }
        public ResponseType ResponseType { get; set; }
        public static ActionResponse<T> Success(T data, int statusCode) => new() { Data = data, StatusCode = statusCode, IsSuccessful = true, ResponseType = ResponseType.Ok };
        public static ActionResponse<T> Success(int statusCode) => new() { Data = default, StatusCode = statusCode, IsSuccessful = true, ResponseType = ResponseType.Ok };
        public static ActionResponse<T> Fail(List<string> errors, int statusCode) => new() { Errors = errors, Data = default, StatusCode = statusCode, IsSuccessful = false };
        public static ActionResponse<T> Fail(string error, int statusCode) => new() { Errors = [error], Data = default, StatusCode = statusCode, IsSuccessful = false, ResponseType = ResponseType.Error, Message = error };
    }
}
