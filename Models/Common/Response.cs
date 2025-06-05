#nullable disable
namespace Radar7D.Models.Common
{
    public class Response<T>
    {
        public bool Succeeded { get; set; }

        public string Message { get; set; }

        public T Data { get; set; }

        public Response()
        {
        }

        public Response(T data, string message = null)
        {
            Succeeded = true;
            Message = message;
            Data = data;
        }
        public Response(string message)
        {
            Succeeded = false;
            Message = message;
        }

    }
}
