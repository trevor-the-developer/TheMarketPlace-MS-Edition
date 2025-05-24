using Microsoft.AspNetCore.Identity;

namespace AuthenticationService.Domain.Entities;

public class TokenResponse
{
    public bool? Succeeded { get; set; }
    public string? Token { get; set; }
    public DateTime? Expiration { get; set; }
    public string? RefreshToken { get; set; }
    public ApiError? ApiError { get; set; }
    public IEnumerable<IdentityError>? Errors { get; set; }
}

public class ApiError
{
    public ApiError(string httpStatusCode, int statusCode, string errorMessage, string? stackTrace = null)
    {
        HttpStatusCode = httpStatusCode;
        StatusCode = statusCode;
        ErrorMessage = errorMessage;
        StackTrace = stackTrace;
    }

    public string HttpStatusCode { get; set; }
    public int StatusCode { get; set; }
    public string ErrorMessage { get; set; }
    public string? StackTrace { get; set; }
}