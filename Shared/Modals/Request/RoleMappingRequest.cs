using System.Text.Json.Serialization;

namespace Shared.Modals.Request
{
    public record RoleMappingRequest(
        [property: JsonPropertyName("id")] Guid Id,
        [property: JsonPropertyName("name")] string Name);
}
