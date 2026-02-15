using System.Text.Json.Serialization;

namespace Shared.Modals.Request
{
    public record ScopeRequest(
       [property: JsonPropertyName("name")] string Name,
       [property: JsonPropertyName("protocol")] string Protocol,
       [property: JsonPropertyName("description")] string Description);
}
