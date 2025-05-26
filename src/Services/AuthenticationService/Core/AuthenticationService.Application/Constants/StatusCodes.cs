namespace AuthenticationService.Application.Constants;

public static class StatusCodes
{
    public const int Status200OK = 200;
    public const int Status201Created = 201;
    public const int Status400BadRequest = 400;
    public const int Status401Unauthorized = 401;
    public const int Status404NotFound = 404;
    public const int Status409Conflict = 409;
    public const int Status500InternalServerError = 500;
    public const int EmailNotConfirmed = 403;  // Using Forbidden (403) for email not confirmed
    public const int Status422UnprocessableEntity = 422;
}