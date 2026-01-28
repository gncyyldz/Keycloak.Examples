namespace Shared.Modals.Request;

public record SetPasswordRequest(string Password, bool IsTemporary);