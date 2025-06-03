namespace weatherCloChase.Core.Interfaces;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(string email, string password);
    Task<AuthResult> LoginAsync(string email, string password);
    Task<bool> ValidateTokenAsync(string token);
}

public class AuthResult
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? Error { get; set; }
    public DateTime? ExpiresAt { get; set; }
}