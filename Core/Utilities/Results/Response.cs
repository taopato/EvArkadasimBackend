// Core/Utilities/Results/Response.cs
namespace Core.Utilities.Results
{
    public class Response<T>
    {
        public T Data { get; }
        public bool Success { get; }
        public string Message { get; }

        public Response(T data, bool success, string message = null)
        {
            Data = data;
            Success = success;
            Message = message;
        }
    }
}
