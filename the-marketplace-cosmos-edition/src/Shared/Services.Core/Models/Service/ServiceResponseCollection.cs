using Services.Core.Enums;

namespace Services.Core.Models.Service;

public class ServiceResponseCollection<T> : ServiceResponse<T>
{
    private readonly int _pageSize = 1;
    private readonly int _totalRecords;

    public int TotalRecords 
    { 
        get => _totalRecords;
        init => _totalRecords = Math.Max(0, value);
    }
    
    public int PageSize 
    { 
        get => _pageSize;
        init => _pageSize = value > 0 ? value : 1;
    }
    
    public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);

    public static ServiceResponseCollection<IReadOnlyCollection<T>> Success(IReadOnlyCollection<T> data, int totalRecords, int pageNumber, int pageSize, string message = "Operation completed successfully")
    {
        return new ServiceResponseCollection<IReadOnlyCollection<T>>
        {
            Status = ServiceStatus.Success,
            Message = message,
            Data = data,
            TotalRecords = totalRecords,
            PageSize = pageSize
        };
    }
}
