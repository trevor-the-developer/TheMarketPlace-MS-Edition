namespace AuthenticationService.Application.Constants;

public static class AuthConstants
{
    // JWT Settings
    public const string JwtSettingsKey = "AuthenticationService:JwtSettings:Key";
    public const string JwtSettingsIssuer = "AuthenticationService:JwtSettings:Issuer";
    public const string JwtSettingsAudience = "AuthenticationService:JwtSettings:Audience";
    public const string JwtSettingsExpires = "AuthenticationService:JwtSettings:ExpiresInMinutes";
    public const string SecretKeyNotConfigured = "JWT secret key is not configured.";

    // Authentication Constants
    public const string UserDoesntExist = "User doesn't exist.";
    public const string InvalidEmailPassword = "Invalid email or password.";
    public const string UserEmailNotConfirmed = "Email not confirmed.";
    public const string LoginFailed = "Login failed.";
    public const string LoginSucceeded = "Login succeeded.";
    public const string InvalidToken = "Invalid token.";
    public const string InvalidRefreshToken = "Invalid refresh token.";
    public const string RefreshTokenExpired = "Refresh token expired.";
    public const string TokenRefreshFailed = "Token refresh failed.";
    public const string TokenRefreshSucceeded = "Token refresh succeeded.";
    public const string LogoutFailed = "Logout failed.";
    public const string LogoutSucceeded = "Logout succeeded.";

    // Registration Constants
    public const string EmailAlreadyExists = "Email already exists.";
    public const string RegistrationFailed = "Registration failed.";
    public const string RegistrationSucceeded = "Registration succeeded.";
    public const string ConfirmationFailed = "Email confirmation failed.";
    public const string ConfirmationSucceeded = "Email confirmation succeeded.";
    public const string ConfirmationCodeInvalid = "Confirmation code is invalid.";
}