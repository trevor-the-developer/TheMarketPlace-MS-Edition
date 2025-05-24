namespace AuthenticationService.Application.Settings;

public class AuthenticationServiceConfiguration
{
    public string ConnectionString { get; set; } = string.Empty;
    public JwtSettings JwtSettings { get; set; } = new JwtSettings();
}

public class JwtSettings
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiresInMinutes { get; set; } = 60;
}