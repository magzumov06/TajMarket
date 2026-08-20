using System.Net;

namespace Domain.Responses;

public class Response<T>
{
    public T? Data { get; set; }
    public string Message { get; set; }
    public int StatusCode { get; set; }

    public bool Success => StatusCode >= 200 && StatusCode < 300;

    public Response(T data)
    {
        Data = data;
        Message = "Success";
        StatusCode = 200;
    }

    public Response(HttpStatusCode statusCode, string message)
    {
        StatusCode = (int)statusCode;
        Message = message;
        Data = default;
    }
    
    public Response(T data, string message)
    {
        Data = data;
        Message = message;
        StatusCode = (int)HttpStatusCode.OK;
    }
}