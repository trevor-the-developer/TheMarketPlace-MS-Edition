using Services.Core.Enums;

namespace Services.Core.Models.Service;

public class ServiceResponse<T>
{
    public required ServiceStatus Status { get; set; }
    
    public required string Message { get; set; }
    
    public T? Data { get; set; }

    public static ServiceResponse<T> Success(T data, string message = "Operation completed successfully")
    {
        return new ServiceResponse<T>
        {
            Status = ServiceStatus.Success,
            Message = message,
            Data = data
        };
    }

    public static ServiceResponse<T> Error(string message, T? data = default)
    {
        return new ServiceResponse<T>
        {
            Status = ServiceStatus.Failure,
            Message = message,
            Data = data
        };
    }
}