namespace Shared.Modals.Request
{
    public record PasswordGrantRequest(string ClientId, string? ClientSecret, string Username, string Password, string? Scope);
}
