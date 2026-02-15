using System.Text.Json.Serialization;

namespace Shared.Modals.Request
{
    public record EmailRequest(
        [property: JsonPropertyName("email")] string Email,
        [property: JsonPropertyName("emailVerified")] bool EmailVerified);
}
